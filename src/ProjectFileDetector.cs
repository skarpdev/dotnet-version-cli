using System;

namespace Skarpdev.DotnetVersion
{
    public class ProjectFileDetector
    {
        public ProjectFileDetector()
        {
            
        }

        /// <summary>
        /// Method tries to find the nearest cs project file and loads up the 
        /// xml and returns it
        /// </summary>
        /// <remarks>
        /// If the given bootstrap path is empty, it will try to detect the nearest (current dir)
        /// cs project file. If bootstrapPath is given, and is a csproj file it will load this
        /// if bootstrap path is a folder, it will try to search for a csproj file there.
        /// </remarks>
        /// <param name="bootstrapPath"></param>
        /// <returns></returns>
        public string FindAndLoadCsProj(string bootstrapPath)
        {
            throw new NotImplementedException();
        }

        public string ResolveCsProjFile { get; set; }
    }
}