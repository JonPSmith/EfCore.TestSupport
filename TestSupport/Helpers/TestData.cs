// Copyright (c) 2016 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT licence. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TestSupport.Helpers
{
    /// <summary>
    /// A static class containing extentions methods for accessing files
    /// </summary>
    public static class TestData
    {
        private const string TestFileDirectoryName = @"TestData";

        //-------------------------------------------------------------------

        /// <summary>
        /// This returns the filepath of the file found by the searchPattern. If more than one file found that throws and exception
        /// </summary>
        /// <param name="searchPattern">If the search pattern starts with a @"\", then it will look in a subdirectory for the file</param>
        /// <param name="callingAssembly">optional: provide the calling assembly. default is to use the current calling assembly</param>
        /// <returns>The absolute filepath to the found file</returns>
        public static string GetFilePath(string searchPattern, Assembly callingAssembly = null)
        {
            callingAssembly = callingAssembly ?? Assembly.GetCallingAssembly();
            string[] fileList = GetFilePaths(searchPattern, callingAssembly);

            if (fileList.Length != 1)
                throw new Exception(string.Format("GetTestDataFilePath: The searchString {0} found {1} file. Either not there or ambiguous",
                    searchPattern, fileList.Length));

            return fileList[0];
        }

        /// <summary>
        /// This returns all the filepaths of file that fit the search pattern
        /// </summary>
        /// <param name="searchPattern">If the search pattern starts with a @"\", then it will look for an alternate testdata directory</param>
        /// <param name="callingAssembly">optional: provide the calling assembly. default is to use the current calling assembly</param>
        /// <returns>array of absolute filepaths that match the filepath</returns>
        public static string[] GetFilePaths(string searchPattern = "", Assembly callingAssembly = null)
        {
            callingAssembly = callingAssembly ?? Assembly.GetCallingAssembly();
            string testDir = null;
            if (searchPattern.Any() && searchPattern[0] == Path.DirectorySeparatorChar)
            {
                //has alternate test directory name, so set that and remove from search string
                var indexNextDirSep = searchPattern.Substring(1).IndexOf(Path.DirectorySeparatorChar);
                if (indexNextDirSep < 0 )
                    throw new InvalidOperationException(@"Your search pattern started with an alternative directory, but did have a closing directory separator.");
                testDir = searchPattern.Substring(1, indexNextDirSep);
                searchPattern = searchPattern.Substring(indexNextDirSep + 2);
            }
            var directory = GetTestDataDir(testDir, callingAssembly);

            string[] fileList = Directory.GetFiles(directory, searchPattern);

            return fileList;
        }

        /// <summary>
        /// This returns the content of the file found by the searchPattern. If more than one file found that throws and exception
        /// </summary>
        /// <param name="searchPattern">If the search pattern starts with a @"\", then it will look in a subdirectory for the file</param>
        /// <param name="callingAssembly">optional: provide the calling assembly. default is to use the current calling assembly</param>
        /// <returns>The content of the file as text of the found file</returns>
        public static string GetFileContent(string searchPattern, Assembly callingAssembly = null)
        {
            callingAssembly = callingAssembly ?? Assembly.GetCallingAssembly();
            var filePath = GetFilePath(searchPattern, callingAssembly);
            return File.ReadAllText(filePath);
        }

        /// <summary>
        /// This will ensure that a file in the TestData directory is deleted
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <param name="callingAssembly">optional: provide the calling assembly. default is to use the current calling assembly</param>
        /// <returns></returns>
        public static bool EnsureFileDeleted(string searchPattern, Assembly callingAssembly = null)
        {
            callingAssembly = callingAssembly ?? Assembly.GetCallingAssembly();
            var fileList = GetFilePaths(searchPattern, callingAssembly);
            if (fileList.Length == 0) return false;
            if (fileList.Length != 1)
                throw new Exception(string.Format("TestFileDeleteIfPresent: The searchString {0} found {1} files!",
                    searchPattern, fileList.Length));

            File.Delete(fileList[0]);
            return true;
        }

        /// <summary>
        /// This will delete a directory and any files inside that directory.
        /// If no directory exists then it simply returns
        /// </summary>
        /// <param name="topDirPath"></param>
        public static void DeleteDirectoryAndAnyContent(string topDirPath)
        {
            if (!Directory.Exists(topDirPath)) return;
            Directory.Delete(topDirPath, true);
        }

        /// <summary>
        /// This deletes all files and directories (and subdirectories) in the given topDir.
        /// It does NOT delete the topDir directory
        /// </summary>
        /// <param name="topDirPath"></param>
        public static void DeleteAllFilesAndSubDirsInDir(string topDirPath)
        {
            if (!Directory.Exists(topDirPath)) return;

            var files = Directory.GetFiles(topDirPath);
            foreach (var file in files)
                File.Delete(file);
            var dirs = Directory.GetDirectories(topDirPath);
            foreach (var dir in dirs)
                Directory.Delete(dir, true);
        }

        //------------------------------------------------------------------------------

        /// <summary>
        /// This will return the absolute file path to the TestData directory in the calling method's project 
        /// </summary>
        /// <param name="alternateTestDir">optional. If given then it can be relative or absolute path, which 
        /// replaces the default TestData directly</param>
        /// <param name="callingAssembly">optional: provide the calling assembly. default is to use the current calling assembly</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetTestDataDir(string alternateTestDir = null, Assembly callingAssembly = null)
        {
            alternateTestDir = alternateTestDir ?? TestFileDirectoryName;
            //see https://stackoverflow.com/questions/670566/path-combine-absolute-with-relative-path-strings
            return Path.Combine(
                Path.GetFullPath(
                    GetCallingAssemblyTopLevelDir(callingAssembly ?? Assembly.GetCallingAssembly()) 
                    + Path.DirectorySeparatorChar + alternateTestDir));
        }

        /// <summary>
        /// This will return the absolute file path of the calling assembly, or the assembly provided 
        /// </summary>
        /// <param name="callingAssembly">optional: provide the calling assembly. default is to use the current calling assembly</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)] //see https://docs.microsoft.com/en-gb/dotnet/api/system.reflection.assembly.getcallingassembly?view=netstandard-2.0#System_Reflection_Assembly_GetCallingAssembly
        public static string GetCallingAssemblyTopLevelDir(Assembly callingAssembly = null)
        {
            var binDir = $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}";
            var pathToManipulate = (callingAssembly ?? Assembly.GetCallingAssembly()).Location;

            var indexOfPart = pathToManipulate.IndexOf(binDir, StringComparison.OrdinalIgnoreCase);
            if (indexOfPart <= 0)
                throw new Exception($"Did not find '{binDir}' in the assembly. Do you need to provide the callingAssembly parameter?");

            return pathToManipulate.Substring(0, indexOfPart);
        }
    }
}
