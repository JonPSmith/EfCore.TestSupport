// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.IO;
using Newtonsoft.Json;
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
        /// It uses PreserveReferencesHandling = PreserveReferencesHandling.Objects to keep the object links and
        /// Formatting = Formatting.Indented to make the JSON easier to read (but it does make a bigger file)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string DefaultSerializeToJson<T>(this T data)
        {
            return JsonConvert.SerializeObject(data, new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            });
        }

        /// <summary>
        /// This will read the data from the JSON file using the fileSuffix as a discriminator
        /// </summary>
        /// <typeparam name="T">This is the type of the data you expect to get back, e.g. <code>List{Book}</code></typeparam>
        /// <param name="fileSuffix">This is the name of the seed data, typically the name of the database that the JSON came from</param>
        /// <returns></returns>
        public static T ReadSeedDataFromJsonFile<T>(this string fileSuffix)
        {
            var filePath = FormJsonFilePath(fileSuffix);
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<T>(json);
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

    }
}