using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Skarp.Version.Cli.CsProj
{
    public class ProjectFileParser
    {
        public virtual string PackageName { get; private set; }
        
        public virtual string PackageVersion { get; private set; }
        
        public virtual string Version { get; private set; }
        
        private IEnumerable<XElement> _propertyGroup { get; set; }

        public virtual void Load(string xmlDocument, ProjectFileProperty property)
        {
            LoadPropertyGroup(xmlDocument);

            var propertyElement = LoadProperty(property);
            
            switch(property)
            {
                case ProjectFileProperty.Version:
                    Version = propertyElement?.Value ?? "0.0.0";
                    break;
                case ProjectFileProperty.PackageVersion:
                    PackageVersion = propertyElement?.Value ?? Version;
                    break;
                case ProjectFileProperty.Title:
                    var defaultPropertyElement = LoadProperty(ProjectFileProperty.PackageId);
                    PackageName = propertyElement?.Value ?? defaultPropertyElement?.Value ?? string.Empty;
                    
                    break;
            }
        }

        public virtual void Load(string xmlDocument, params ProjectFileProperty[] properties)
        {
            if (properties == null || !properties.Any())
            {
                properties = new[]
                {
                    ProjectFileProperty.Title,
                    ProjectFileProperty.Version,
                    ProjectFileProperty.PackageId,
                    ProjectFileProperty.PackageVersion,
                };
            } 
            // Try to load xmlDocument even if there is no properties to be loaded
            // in order to verify if project file is well formed
            LoadPropertyGroup(xmlDocument);

            foreach (var property in properties)
            {
                Load(xmlDocument, property);
            }
        }

        private XElement LoadProperty(ProjectFileProperty property)
        {
            var propertyElement = (
                from prop in _propertyGroup.Elements()
                where prop.Name == property.ToString("g")
                select prop
            ).FirstOrDefault();
            return propertyElement;
        }

        private void LoadPropertyGroup(string xmlDocument)
        {
            // Check if it has been already loaded
            if (_propertyGroup != null) return;

            var xml = XDocument.Parse(xmlDocument, LoadOptions.PreserveWhitespace);

            // Project should be root of the document
            var project = xml.Elements("Project");
            var xProject = project as IList<XElement> ?? project.ToList();
            if (!xProject.Any())
            {
                throw new ArgumentException(
                    "The provided csproj file seems malformed - no <Project> in the root",
                    paramName: nameof(xmlDocument)
                );
            }

            _propertyGroup = xProject.Elements("PropertyGroup");
        }
    }
}