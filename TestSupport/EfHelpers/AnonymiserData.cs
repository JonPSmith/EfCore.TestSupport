// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TestSupport.EfHelpers
{
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
        /// This contains all the options provided after the first part, separated by :, e.g "Max=4:Min=100"
        /// You can add more commands that are specific to your your own Anonymiser function, e.g. "Case=Pascal"
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

        /// <summary>
        /// This decodes the replacement string into it component parts
        /// </summary>
        /// <param name="replaceRequest"></param>
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