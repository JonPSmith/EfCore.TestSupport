// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TestSupport.SeedDatabase.Internal;

namespace TestSupport.SeedDatabase
{
    /// <summary>
    /// This class contains methods to help you save database data for later loading into a database for unit testing.
    /// It can scan a linked set of EF Core database classes and do the following:
    /// 1. It can reset the primary, alternative and foreign keys to a default state so that the 
    /// </summary>
    public class DataResetter
    {
        private readonly IModel _model;
        private readonly DataResetterConfig _config;
        private readonly Dictionary<Type, List<MemberAnonymiseData>> _anonymiseThese;

        /// <summary>
        /// This takes in the DbContext of the database you want to save
        /// </summary>
        /// <param name="context">DbContext of the database you want to save</param>
        /// <param name="config">optional configuration data</param>
        public DataResetter(DbContext context, DataResetterConfig config = null)
        {
            _model = context?.Model ?? throw new ArgumentNullException(nameof(context));
            _config = config ?? new DataResetterConfig();
            _anonymiseThese = _config.AnonymiseRequests.GroupBy(x => x.ClassType)
                .ToDictionary(x => x.Key, y => y.ToList());
        }

        /// <summary>
        /// This resets the primary keys and foreign keys to default value in a single entity.
        /// It also will anonymise any properties that you registered with the config
        /// </summary>
        /// <param name="entityToReset">The entity class to reset</param>
        public void ResetKeysSingleEntity(object entityToReset)
        {
            var entityType = entityToReset.GetType();
            var entityModel = _model.FindEntityType(entityType);
            if (entityModel == null)
                throw new InvalidOperationException($"The class {entityType.Name} is not a class that the provided DbContext knows about.");

            ResetPkAndIndexesSingleEntity(entityModel, entityToReset);
        }

        /// <summary>
        /// This will work through an EF Core entity class and its relationships and set
        /// all primary keys and foreign keys to default value so that the entities will be
        /// added as new entries to the database.
        /// It also will anonymise any properties that you registered with the config
        /// </summary>
        /// <param name="entityToReset">The entity class to reset, or an collection of entity classes to </param>
        public void ResetKeysEntityAndRelationships(object entityToReset)
        {
            var stopCircularLook = new HashSet<object>();
            if (entityToReset is IEnumerable enumerable)
            {
                // Collection
                foreach (var item in enumerable)
                {
                    ResetPkAndIndexesEntityAndRelationships(item, stopCircularLook, false);
                }
            }
            else
            {
                // Single class
                ResetPkAndIndexesEntityAndRelationships(entityToReset, stopCircularLook, false);
            }
        }

        //--------------------------------------------------------------------
        // private

        /// <summary>
        /// This will work through an EF Core entity class and its relationships and set
        /// all primary keys and foreign keys to default value so that the entities will be
        /// added as new entries to the database.
        /// </summary>
        /// <param name="entityToReset"></param>
        /// <param name="stopCircularLook"></param>
        /// <param name="ignoreNonEntityClasses">if false then throw exception if the class isn't found in entity class</param>
        private void ResetPkAndIndexesEntityAndRelationships(object entityToReset, 
            HashSet<object> stopCircularLook, bool ignoreNonEntityClasses = true)
        {

            if (stopCircularLook.Contains(entityToReset))
                //If we have seen it before then don't go any further.
                return;

            stopCircularLook.Add(entityToReset);

            var entityModel = _model.FindEntityType(entityToReset.GetType());
            if (entityModel == null && ignoreNonEntityClasses)
                //We ignore non-entity classes found in our scan
                return;

            ResetPkAndIndexesSingleEntity(entityModel, entityToReset);
            //now go through dependent relationships 
            foreach (var navigation in entityModel.GetNavigations())
            {
                var navProp = navigation.PropertyInfo;
                if (navProp == null || navigation.ClrType.IsValueType)
                    continue;

                var navValue = navProp.GetValue(entityToReset);
                if (navValue == null)
                    continue;


                if (navValue is IEnumerable enumerable)
                {
                    // Many navigational property
                    foreach (var item in enumerable)
                    {
                        ResetPkAndIndexesEntityAndRelationships(item, stopCircularLook);
                    }
                }
                else
                {
                    // Single navigational property
                    ResetPkAndIndexesEntityAndRelationships(navValue, stopCircularLook);
                }
            }
        }

        private void ResetPkAndIndexesSingleEntity(IEntityType entityModel, object entityToReset)
        {
            foreach (var keyProp in entityModel.GetKeys()
                .Where(x => x.IsPrimaryKey() || !_config.DoNotResetAlternativeKey ).SelectMany(x => x.Properties))
            {
                keyProp.PropertyInfo.SetValue(entityToReset, GetDefaultValue(entityToReset.GetType()));
            }

            foreach (var keyProp in entityModel.GetForeignKeys().SelectMany(x => x.Properties))
            {
                keyProp.PropertyInfo.SetValue(entityToReset, GetDefaultValue(entityToReset.GetType()));
            }

            if (_anonymiseThese.TryGetValue(entityModel.ClrType, out List<MemberAnonymiseData> membersToAno))
            {
                foreach (var memberData in membersToAno)
                {
                    memberData.AnonymiseMember(entityToReset, _config);
                }
            }
        }

        // Thanks to StackOverflow https://stackoverflow.com/a/2490274/1434764
        private static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }
    }
}