using System;
using System.Linq;
using System.Xml.Linq;
using Skarp.Version.Cli.CsProj.FileSystem;

namespace Skarp.Version.Cli.CsProj
{
    public class ProjectFileVersionPatcher
    {
        private readonly IFileSystemProvider _fileSystem;
        private XDocument _doc;

        public ProjectFileVersionPatcher(IFileSystemProvider fileSystem)
        {
            _fileSystem = fileSystem;
        }
        
        public virtual void Load(string xmlDocument)
        {
            _doc = XDocument.Parse(xmlDocument, LoadOptions.PreserveWhitespace);
        }

        /// <summary>
        /// Replace the existing version number in the csproj xml with the new version
        /// </summary>
        /// <param name="oldVersion">The old version number present in the xml</param>
        /// <param name="newVersion">The new version number to persist in the csproj file</param>
        /// <returns></returns>
        public virtual void PatchVersionField(string oldVersion, string newVersion)
        {
            var elementName = "Version";
            PatchGenericField(elementName, oldVersion, newVersion);
        }

        /// <summary>
        /// Helper method for patching up a generic XML field in the loaded XML
        /// </summary>
        /// <param name="elementName">The name to find and update or add it to the tree</param>
        /// <param name="oldVal">Old value</param>
        /// <param name="newVal">New value</param>
        /// <exception cref="InvalidOperationException"></exception>
        private void PatchGenericField(string elementName, string oldVal, string newVal)
        {
            if (_doc == null)
            {
                throw new InvalidOperationException("Please call Load(string xml) before invoking patch operations");
            }

            // If the element is not present, add it to the XML document (csproj file
            if (!ContainsElement(elementName))
            {
                AddMissingElementToCsProj(elementName, oldVal);
            }

            var elm = _doc.Descendants(elementName).First();
            elm.Value = newVal;
        }

        private bool ContainsElement(string elementName)
        {
            var nodes = _doc.Descendants(elementName);
            return nodes.Any();
        }
        
        private void AddMissingElementToCsProj(string elementName, string value)
        {
            // try to locate the PropertyGroup where the element belongs 
            var node = _doc.Descendants("TargetFramework").FirstOrDefault();
            if (node == null)
            {
                node = _doc.Descendants("TargetFrameworks").FirstOrDefault();

                if (node == null)
                {
                    throw new ArgumentException(
                        $"Given XML does not contain {elementName} and cannot locate existing PropertyGroup to add it to - is this a valid csproj file?");
                }
            }

            var propertyGroup = node.Parent;
            propertyGroup.Add(new XElement(elementName, value));
        }

        /// <summary>
        /// Save the csproj changes to disk
        /// </summary>
        /// <param name="xml">The csproj xml content</param>
        /// <param name="filePath">The path of the csproj to write to</param>
        public virtual void Flush(string filePath)
        {
            _fileSystem.WriteAllContent(filePath, ToXmlString());
        }

        /// <summary>
        /// Get the underlying csproj XML back from the patcher as a string
        /// </summary>
        /// <returns></returns>
        public virtual string ToXmlString()
        {
            return _doc.ToString();
        }
    }
}