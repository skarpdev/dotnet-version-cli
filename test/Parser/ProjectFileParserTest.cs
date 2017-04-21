using System;
using Skarpdev.DotnetVersion.Parser;
using Xunit;

namespace Skarpdev.DotnetVersion.Test
{
    public class ProjectFileParserTest
    {
        private readonly ProjectFileParser parser;

        public ProjectFileParserTest()
        {
            parser = new ProjectFileParser();
            
        }
        [Fact]
        public void CanParseWellFormedProjectFiles()
        {
            var csProjXml = "<Project Sdk=\"Microsoft.NET.Sdk\">"+
                            "<PropertyGroup>" + 
                            "<TargetFramework>netstandard1.6</TargetFramework>" +
                            "<RootNamespace>Unit.For.The.Win</RootNamespace>" +
                            "<PackageId>Unit.Testing.Library</PackageId>" +
                            "<Version>1.0.0</Version>" +
                            "</PropertyGroup>" +
                            "</Project>";

            parser.Load(csProjXml);
            Assert.Equal("1.0.0", parser.Version);
        }
    }
}
