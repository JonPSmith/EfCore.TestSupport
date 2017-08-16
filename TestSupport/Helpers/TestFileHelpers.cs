// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TestSupport.Helpers
{
    public static class TestFileHelpers
    {
        private const string TestFileDirectoryName = @"TestData";

        //-------------------------------------------------------------------

        public static string GetTestDataFilePath(string searchPattern)
        {
            string[] fileList = GetPathFilesOfGivenName(searchPattern);

            if (fileList.Length != 1)
                throw new Exception(string.Format("GetTestDataFilePath: The searchString {0} found {1} file. Either not there or ambiguous",
                    searchPattern, fileList.Length));

            return fileList[0];
        }

        public static string GetTestDataFileContent(string searchPattern)
        {
            var filePath = GetTestDataFilePath(searchPattern);
            return File.ReadAllText(filePath);
        }

        internal static bool TestFileDeleteIfPresent(string searchPattern)
        {
            var fileList = GetPathFilesOfGivenName(searchPattern);
            if (fileList.Length == 0) return false;
            if (fileList.Length != 1)
                throw new Exception(string.Format("TestFileDeleteIfPresent: The searchString {0} found {1} files!",
                    searchPattern, fileList.Length));

            File.Delete(fileList[0]);
            return true;
        }

        public static void DeleteDirectoryAndAnyContent(string topDir)
        {
            if (!Directory.Exists(topDir)) return;
            Directory.Delete(topDir, true);
        }

        /// <summary>
        /// This deletes all files and directories (and subdirectories) in the given topDir.
        /// It does NOT delete the topDir directory
        /// </summary>
        /// <param name="topDir"></param>
        public static void DeleteAllFilesAndSubDirsInDir(string topDir)
        {
            if (!Directory.Exists(topDir)) return;

            var files = Directory.GetFiles(topDir);
            foreach (var file in files)
                File.Delete(file);
            var dirs = Directory.GetDirectories(topDir);
            foreach (var dir in dirs)
                Directory.Delete(dir, true);
        }

        public static string[] GetPathFilesOfGivenName(string searchPattern = "")
        {
            var directory = GetTestDataFileDirectory();
            if (searchPattern.Contains(@"\"))
            {
                //Has subdirectory in search pattern, so change directory
                directory = Path.Combine(directory, searchPattern.Substring(0, searchPattern.LastIndexOf('\\')));
                searchPattern = searchPattern.Substring(searchPattern.LastIndexOf('\\')+1);
            }

            string[] fileList = Directory.GetFiles(directory, searchPattern);

            return fileList;
        }

        //------------------------------------------------------------------------------

        /// <summary>
        /// This will return the adbsolue file path to the TestData directory in the calling method's project 
        /// </summary>
        /// <param name="alternateTestDir">optional. If given then it can be relative or absolute and replaes the default TestData directly</param>
        /// <returns></returns>
        public static string GetTestDataFileDirectory(string alternateTestDir = TestFileDirectoryName, Assembly callingAssembly = null)
        {
            //see https://stackoverflow.com/questions/670566/path-combine-absolute-with-relative-path-strings
            return Path.Combine(
                Path.GetFullPath(
                    GetCallingAssemblyTopLevelDirectory(callingAssembly ?? Assembly.GetCallingAssembly()) 
                    + "\\" + alternateTestDir));
        }

        public static string GetCallingAssemblyTopLevelDirectory(Assembly callingAssembly = null)
        {
            const string binDir = @"\bin\";
            var pathToManipulate = (callingAssembly ?? Assembly.GetCallingAssembly()).Location;

            var indexOfPart = pathToManipulate.IndexOf(binDir, StringComparison.OrdinalIgnoreCase)+1;
            if (indexOfPart < 0)
                throw new Exception($"Did not find '{binDir}' in the ApplicationBasePath");

            return pathToManipulate.Substring(0, indexOfPart - 1);
        }
    }
}
