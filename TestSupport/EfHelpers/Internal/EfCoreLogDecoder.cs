// Copyright (c) 2020 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace TestSupport.EfHelpers.Internal
{
    internal class EfCoreLogDecoder
    {
        private const string ParameterStart = "[Parameters=[";

        private static readonly Regex ParamRegex = new Regex(@"(@p\d+|@__\w*?_\d+)=('(.*?)'|NULL)(\s\(\w*?\s=\s\w*\))*(?:,\s|\]).*?");

        private readonly string _paramName;
        private readonly string[] _paramTypes;
        private readonly string _paramValue;

        private EfCoreLogDecoder(Match matchedParam)
        {
            _paramName = matchedParam.Groups[1].Value;
            _paramValue = matchedParam.Groups[3].Value;
            _paramTypes = matchedParam.Groups[4].Captures.Cast<Capture>().Select(x => x.Value).ToArray();
        }

        private string ValueToInsert()
        {
            if (_paramValue == string.Empty)
                //If no value we assume NULL 
                //NOTE: this fails with empty string
                return "NULL";

            if (_paramTypes.Any())
                //We assume its something that needs to be a string
                //NOTE: This will get byte[] wrong
                return $"'{_paramValue.Replace("'","''")}'";

            if (_paramValue == "True" || _paramValue == "False")
                return _paramValue == "True" ? "1" : "0";

            //NOTE: numbers are presented as a string, but SQL Server handles that OK.
            return $"'{_paramValue}'";
        }

        public override string ToString()
        {
            var paramTypes = string.Join(",", _paramTypes);
            return $"{_paramName}={ValueToInsert()}, {paramTypes}";
        }

        /// <summary>
        /// This will try and decode an EF Core "CommandExecuted" 
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static string DecodeMessage(LogOutput log)
        {
            if (log.EventId.Name != RelationalEventId.CommandExecuted.Name)
                return log.Message;

            var messageLines = log.Message.Split('\n').Select(x => x.Trim()).ToArray();
            var parametersIndex = messageLines[0].IndexOf(ParameterStart);
            if (parametersIndex <= 0)
                return log.Message;

            var decodedMatches = ParamRegex.Matches(messageLines[0].Substring(parametersIndex + ParameterStart.Length))
                .Cast<Match>().Select(x => new EfCoreLogDecoder(x)).ToList();
            //is sensitive logging isn't enabled then all the param values will '?', so we just return the message
            if (decodedMatches.All(x => x._paramValue == "?"))
                return log.Message;

            decodedMatches.Reverse();   //Need to reverse so that @p10 comes before @p1

            for (int i = 1; i < messageLines.Length; i++)
            {
                var lineToUpdate = messageLines[i];
                foreach (var param in decodedMatches)
                {
                    lineToUpdate = lineToUpdate.Replace(param._paramName, param.ValueToInsert());
                }
                messageLines[i] = lineToUpdate;
            }

            return string.Join("\r\n", messageLines);
        }
    }
}