using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Skarp.Version.Cli.CsProj
{
    public class ProjectFileParser
    {
        public virtual string Version { get; private set; }
        
        public virtual string PackageVersion { get; private set; }

        public virtual string PackageName { get; private set; }

        public virtual void Load(string xmlDocument)
        {
            var xml = XDocument.Parse(xmlDocument, LoadOptions.PreserveWhitespace);

            // Project should be root of the document
            var project = xml.Elements("Project");
            var xProject = project as IList<XElement> ?? project.ToList();
            if(!xProject.Any())
            {
                throw new ArgumentException(
                    "The provided csproj file seems malformed - no <Project> in the root", 
                    paramName: nameof(xmlDocument)
                );
            }

            var propertyGroup = xProject.Elements("PropertyGroup");
            
            var xVersion = (
                from prop in propertyGroup.Elements()
                where prop.Name == "Version"
                select prop
            ).FirstOrDefault();
            Version = xVersion?.Value ?? "0.0.0";
            
            var xPackageVersion = (
                from prop in propertyGroup.Elements()
                where prop.Name == "PackageVersion"
                select prop
            ).FirstOrDefault();
            PackageVersion = xPackageVersion?.Value ?? Version;

            var packageId = (
                from prop in propertyGroup.Elements()
                where prop.Name == "PackageId"
                select prop
            ).FirstOrDefault();

            var title = (
                from prop in propertyGroup.Elements()
                where prop.Name == "Title"
                select prop
            ).FirstOrDefault();
            PackageName = title?.Value ?? packageId.Value;

            if (string.IsNullOrEmpty(PackageName))
            {
                throw new ArgumentException(
                    "The provided csproj file seems malformed - no <Title> or <PackageId> in the <PropertyGroup>",
                    paramName: nameof(xmlDocument)
                );
            }
        }
    }
}