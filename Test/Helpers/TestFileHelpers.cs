// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.PlatformAbstractions;

namespace test.Helpers
{
    internal static class TestFileHelpers
    {
        private const string TestFileDirectoryName = @"TestData";

        //-------------------------------------------------------------------

        internal static string GetTestFileFilePath(string searchPattern)
        {
            string[] fileList = GetTestFileFilesOfGivenName(searchPattern);

            if (fileList.Length != 1)
                throw new Exception(string.Format("GetTestFileFilePath: The searchString {0} found {1} file. Either not there or ambiguous",
                    searchPattern, fileList.Length));

            return fileList[0];
        }

        internal static string GetTestFileContent(string searchPattern)
        {
            var filePath = GetTestFileFilePath(searchPattern);
            return File.ReadAllText(filePath);
        }

        internal static bool TestFileDeleteIfPresent(string searchPattern)
        {
            var fileList = GetTestFileFilesOfGivenName(searchPattern);
            if (fileList.Length == 0) return false;
            if (fileList.Length != 1)
                throw new Exception(string.Format("TestFileDeleteIfPresent: The searchString {0} found {1} files!",
                    searchPattern, fileList.Length));

            File.Delete(fileList[0]);
            return true;
        }

        internal static void DeleteDirectoryAndAnyContent(string topDir)
        {
            if (!Directory.Exists(topDir)) return;
            Directory.Delete(topDir, true);
        }

        /// <summary>
        /// This deletes all files and directories (and subdirectories) in the given topDir.
        /// It does NOT delete the topDir directory
        /// </summary>
        /// <param name="topDir"></param>
        internal static void DeleteAllFilesAndSubDirsInDir(string topDir)
        {
            if (!Directory.Exists(topDir)) return;

            var files = Directory.GetFiles(topDir);
            foreach (var file in files)
                File.Delete(file);
            var dirs = Directory.GetDirectories(topDir);
            foreach (var dir in dirs)
                Directory.Delete(dir, true);
        }

        internal static string[] GetTestFileFilesOfGivenName(string searchPattern = "")
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

        public static string GetTestDataFileDirectory(string alternateTestDir = TestFileDirectoryName)
        {
            return Path.Combine(GetSolutionDirectory(), "test", alternateTestDir);
        }


        public static string GetSolutionDirectory()
        {
            var host = new ApplicationEnvironment();
            var pathToManipulate = host.ApplicationBasePath;

            var partToEndOn = typeof(TestFileHelpers).FullName.Split('.').First() + @"\bin\";
            var indexOfPart = pathToManipulate.IndexOf(partToEndOn, StringComparison.OrdinalIgnoreCase);
            if (indexOfPart < 0)
                throw new Exception($"Did not find '{partToEndOn}' in the ApplicationBasePath");

            return pathToManipulate.Substring(0, indexOfPart-1);

        }
    }
}
