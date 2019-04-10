// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using TestSupport.EfHelpers.Internal;

namespace TestSupport.EfHelpers
{
    /// <summary>
    /// This provides configuration information for the DataResetter
    /// </summary>
    public class DataResetterConfig
    {
        internal List<MemberAnonymiseData> AnonymiseRequests { get; private set; } = new List<MemberAnonymiseData>();

        /// <summary>
        /// If true any Alternative keys will not be reset
        /// </summary>
        public bool DoNotResetAlternativeKey { get; set; }

        /// <summary>
        /// This function is called on whenever a property/field you have added via the AnonymiseThisMember config method
        /// </summary>
        public Func<AnonymiserData, string> AnonymiserFunc { get; set; } = DefaultAnonymiser;

        /// <summary>
        /// This allows you to add a property or field in an class to be registered to be anonymised
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
        /// It applies the "Max=nn" only if the returned string is longer than that
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string DefaultAnonymiser(AnonymiserData data)
        {
            var anoString = Guid.NewGuid().ToString("N");
            if(data.ReplacementType.Equals("Email", StringComparison.InvariantCultureIgnoreCase))
                anoString += "@ano.com";
            if (data.MaxLength > 0 && data.MaxLength < anoString.Length)
                //we trim from the end so that an email will still end in @ano.com
                return anoString.Substring(anoString.Length - data.MaxLength);
            return anoString;
        }
    }

    /// <summary>
    /// This is used to provide the AnonymiserFunc with extra information on how to create the anonymised string
    /// </summary>
    public class AnonymiserData
    {
        /// <summary>
        /// This is the first part of the replacementRequest, e.g. "Name:Max=4" would set this to "Name"
        /// </summary>
        public string ReplacementType { get; private set; }

        /// <summary>
        /// This contains all the options provided after the first part, separated by :, e.g Max=4, Min=100
        /// </summary>
        public IImmutableList<string> ReplaceOptions { get; private set; }

        /// <summary>
        /// This holds the max length, or -1 if not set
        /// </summary>
        public int MaxLength { get; private set; } = -1; 

        /// <summary>
        /// This holds the min length, or -1 if not set
        /// </summary>
        public int MinLength { get; private set; } = -1;

        internal AnonymiserData(string replaceRequest)
        {
            var parts = replaceRequest.Split(':');
            ReplacementType = parts[0];
            var options = new List<string>();
            for (int i = 1; i < parts.Length; i++)
            {
                options.Add(parts[i]); 
                var config = parts[i].Split('=');
                if (config[0].Equals("max", StringComparison.InvariantCultureIgnoreCase))
                    MaxLength = int.Parse(config[1]);
                else if (config[0].Equals("min", StringComparison.InvariantCultureIgnoreCase))
                    MinLength = int.Parse(config[1]);
                //we don't error on other options as the caller might want to add other options.
            }
            ReplaceOptions = options.ToImmutableList();
        }
    }
}