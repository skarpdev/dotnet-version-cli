using System;
using System.Linq;
using System.Xml.Linq;

namespace Skarpdev.DotnetVersion.Parser
{
    public class ProjectFileParser
    {
        public string Version { get; set; }

        public void Load(string xmlDocument)
        {
            var xml = XDocument.Parse(xmlDocument);

            // Project should be root of the document
            var project = xml.Elements("Project");
            if(!project.Any())
            {
                throw new ArgumentException(
                    "The provided csproj file seems malformed - no <Project> in the root", 
                    paramName: "project"
                );
            }

            var propertyGroup = project.Elements("PropertyGroup");
            
            var xVersion = (
                from prop in propertyGroup.Elements()
                where prop.Name == "Version"
                select prop
            ).FirstOrDefault();

            if(xVersion == null)
            {
                throw new ArgumentException("Provided csproj file does not contain a <Version>", paramName: "version");
            }
            Version = xVersion.Value;            
        }
    }
}