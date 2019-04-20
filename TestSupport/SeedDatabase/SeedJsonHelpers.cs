// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TestSupport.Helpers;

namespace TestSupport.SeedDatabase
{
    /// <summary>
    /// Set of extensions that help with serialize and save data to JSON, and read+deserialise data back from the JSON files
    /// </summary>
    public static class SeedJsonHelpers
    {
        /// <summary>
        /// This serialises the data you provide into a JSON string.
        /// You may want to build your own if you have specific requirements
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string DefaultSerializeToJson<T>(this T data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore, //NOTE: turning this on can cause the serialization to duplicate objects.
                Formatting = Formatting.Indented
            });
        }

        /// <summary>
        /// This will read the data from the JSON file using the fileSuffix as a discriminator
        /// You may want to build your own if you have specific requirements
        /// </summary>
        /// <typeparam name="T">This is the type of the data you expect to get back, e.g. <code>List{Book}</code></typeparam>
        /// <param name="fileSuffix">This is the name of the seed data, typically the name of the database that the JSON came from</param>
        /// <returns></returns>
        public static T ReadSeedDataFromJsonFile<T>(this string fileSuffix)
        {
            var filePath = FormJsonFilePath(fileSuffix);
            var json = File.ReadAllText(filePath);
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new ResolvePrivateSetters()
            };
            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        /// <summary>
        /// This writes the JSON string to a JSON file using the fileSuffix as part of the file name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileSuffix">This should be different for each seed data. Suggest using the name of the database that produced it.</param>
        /// <param name="json">The json string to save</param>
        public static void WriteJsonToJsonFile(this string fileSuffix, string json)
        {
            var filePath = FormJsonFilePath(fileSuffix);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// This forms the name of the json file using the fileSuffix
        /// This is of the form $"SeedData-{fileSuffix}.json"
        /// </summary>
        /// <param name="fileSuffix">This is the name of the seed data, typically the name of the database that the JSON came from</param>
        /// <returns></returns>
        public static string FormJsonFilePath(string fileSuffix)
        {
            return Path.Combine(TestData.GetTestDataDir(), $"SeedData-{fileSuffix}.json");
        }

        //-----------------------------------------------------------------
        //private

        //Thanks to https://bartwullems.blogspot.com/2018/02/jsonnetresolve-private-setters.html
        private class ResolvePrivateSetters : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(
                MemberInfo member,
                MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);

                if (!prop.Writable)
                {
                    var property = member as PropertyInfo;
                    if (property != null)
                    {
                        var hasPrivateSetter = property.GetSetMethod(true) != null;
                        prop.Writable = hasPrivateSetter;
                    }
                }

                return prop;
            }
        }
    }
}