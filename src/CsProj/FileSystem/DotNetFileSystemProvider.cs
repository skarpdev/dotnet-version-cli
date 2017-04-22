using System;
using System.Collections.Generic;
using System.IO;

namespace Skarpdev.DotnetVersion.CsProj.FileSystem
{
    public class DotNetFileSystemProvider : IFileSystemProvider
    {
        public IEnumerable<string> List(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        public bool IsCsProjectFile(string path)
        {
            return File.Exists(path) && path.EndsWith(".csproj");
        }

        public string GetPathFromProjectFile(string path)
        {
            throw new NotImplementedException();
        }

        public string Cwd()
        {
            throw new NotImplementedException();
        }

        public string LoadContent(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}