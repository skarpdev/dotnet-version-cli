using System;
using System.Linq;
using System.Text;
using Skarp.Version.Cli.CsProj.FileSystem;

namespace Skarp.Version.Cli.CsProj
{
    public class ProjectFileDetector
    {
        private readonly IFileSystemProvider _fileSystem;
        public ProjectFileDetector(
            IFileSystemProvider fileSystem)
        {
            _fileSystem = fileSystem;
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
            var path = bootstrapPath;
            string csProjFile;

            if (string.IsNullOrEmpty(bootstrapPath))
            {
                path = _fileSystem.Cwd();
            }
            if (_fileSystem.IsCsProjectFile(path))
            {
                csProjFile = path;
            }
            else
            {
                var files = _fileSystem.List(path);
                var csProjFiles = files.Where(q => q.EndsWith(".csproj"));
                if (csProjFiles.Count() > 1)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Multiple csproj files found - aborting:");
                    foreach (var project in csProjFiles)
                    {
                        sb.AppendLine($"\t{project}");
                    }

                    throw new OperationCanceledException(sb.ToString());
                }

                csProjFile = csProjFiles.Single();
            }
            ResolvedCsProjFile = csProjFile;
            
            var xml = _fileSystem.LoadContent(csProjFile);
            return xml;
        }

        public string ResolvedCsProjFile { get; private set; }
    }
}