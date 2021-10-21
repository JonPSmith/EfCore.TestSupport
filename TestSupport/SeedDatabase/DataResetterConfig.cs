// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TestSupport.SeedDatabase.Internal;

namespace TestSupport.SeedDatabase
{
    /// <summary>
    /// This provides configuration information for the DataResetter
    /// </summary>
    public class DataResetterConfig
    {
        internal const string EmailSuffix = "@gmail.com";

        internal List<MemberAnonymiseData> AnonymiseRequests { get; private set; } = new List<MemberAnonymiseData>();

        /// <summary>
        /// If true any Alternative keys will not be reset
        /// </summary>
        public bool DoNotResetAlternativeKey { get; set; }

        /// <summary>
        /// This function is called on whenever a property you have added via the AnonymiseThisMember config method
        /// </summary>
        public Func<AnonymiserData, object, string> AnonymiserFunc { get; set; } = DefaultAnonymiser;

        /// <summary>
        /// This allows you to add a property in an class to be registered to be anonymised
        /// </summary>
        /// <typeparam name="TEntity">The class the field or property is in</typeparam>
        /// <param name="expression">An expression such as "p => p.PropertyInYourClass"</param>
        /// <param name="replaceRequest">Provide usage and config of the replacement string, e.g. "Email" or "FirstName:Max=10:Min=5"
        /// - First part is the name says what you want, e.g. FirstName, Email, Address1, Country, etc. 
        /// - You can then add properties like :Max=10,:Min=2
        /// NOTE: The default anonymiser uses guids for everything, but add @ana.com if "Email". It also applies the Max=nn if guid is longer
        /// </param>
        public void AddToAnonymiseList<TEntity>(Expression<Func<TEntity, string>> expression, string replaceRequest)
        {
            var member = MemberAnonymiseData.GetPropertyViaLambda(expression);
            AnonymiseRequests.Add(new MemberAnonymiseData(typeof(TEntity), member, replaceRequest ));
        }

        /// <summary>
        /// This is a simple Anonymiser using guids
        /// It adds "@ana.com" to end of guid if "Email"
        /// It applies both "Min=nn" and "Max=nn", but max is applied only if the guid string is longer than than the max
        /// </summary>
        /// <param name="data">This is the AnonymiserData produced by when you called <see cref="AddToAnonymiseList{TEntity}"/> </param>
        /// <param name="classInstance">This is the instance of the class it is updating. Useful if you want to use matching data in the same instance.</param>
        /// <returns></returns>
        public static string DefaultAnonymiser(AnonymiserData data, object classInstance)
        {
            var anoString = Guid.NewGuid().ToString("N");
            while (data.MinLength > 0 && anoString.Length < data.MinLength)
                anoString += anoString;
            if(data.ReplacementType.Equals("Email", StringComparison.InvariantCultureIgnoreCase))
                anoString += EmailSuffix;
            if (data.MaxLength > 0 && data.MaxLength < anoString.Length)
                //we trim from the end so that an email will still end in @ano.com
                return anoString.Substring(anoString.Length - data.MaxLength);
            return anoString;
        }
    }
}