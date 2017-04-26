using System.IO;

namespace Skarp.Version.Cli.CsProj
{
    public class ProjectFileVersionPatcher
    {
        /// <summary>
        /// Replace the existing version number in the csproj xml with the new version
        /// </summary>
        /// <param name="xml">The csproj xml loaded from disk</param>
        /// <param name="oldVersion">The old version number present in the xml</param>
        /// <param name="newVersion">The new version number to persist in the csproj file</param>
        /// <returns></returns>
        public virtual string Patch(string xml, string oldVersion, string newVersion)
        {
            return xml.Replace(
                $"<Version>{oldVersion}</Version>", 
                $"<Version>{newVersion}</Version>"
            );
        }

        /// <summary>
        /// Save the csproj changes to disk
        /// </summary>
        /// <param name="xml">The csproj xml content</param>
        /// <param name="filePath">The path of the csproj to write to</param>
        public virtual void Flush(string xml, string filePath)
        {
            File.WriteAllText(filePath, xml);
        }
    }
}