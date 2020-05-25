using System.Collections.Generic;

namespace Skarp.Version.Cli.CsProj.FileSystem
{
    public interface IFileSystemProvider
    {
        /// <summary>
        /// List the items in the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IEnumerable<string> List(string path);

        /// <summary>
        /// Determines whether the given path is actually a csproj file or a path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool IsCsProjectFile(string path);

        /// <summary>
        /// Get the current working directory
        /// </summary>
        /// <returns></returns>
        string Cwd();

        /// <summary>
        /// Loads all the content from the given file path as a string
        /// </summary>
        /// <param name="filePath"></param>
        string LoadContent(string filePath);

        /// <summary>
        /// Writes all the content to the given file as a strings
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        void WriteAllContent(string filePath, string data);
    }
}