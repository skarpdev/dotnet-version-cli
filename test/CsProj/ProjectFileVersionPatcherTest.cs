using System;
using Skarp.Version.Cli.CsProj;
using Xunit;

namespace Skarp.Version.Cli.Test.CsProj
{
    public class ProjectFileVersionPatcherTest
    {
        private static string _projectXml =
                    "<Project Sdk=\"Microsoft.NET.Sdk\">" +
                    "<PropertyGroup>" +
                    "<TargetFramework>netstandard1.6</TargetFramework>" +
                    "<RootNamespace>Unit.For.The.Win</RootNamespace>" +
                    "<PackageId>Unit.Testing.Library</PackageId>" +
                    "<Version>1.0.0</Version>" +
                    "<PackageVersion>1.0.0</PackageVersion>" +
                    "</PropertyGroup>" +
                    "</Project>";

        private readonly ProjectFileVersionPatcher _patcher;

        public ProjectFileVersionPatcherTest()
        {
            _patcher = new ProjectFileVersionPatcher();
        }

        [Fact]
        public void Throws_when_load_not_called()
        {
            var ex = Record.Exception((() => _patcher.PatchVersionField("1.0.0", "2.0.0")));

            Assert.IsAssignableFrom<InvalidOperationException>(ex);
        }
        [Fact]
        public void CanPatchVersionOnWellFormedXml()
        {
            _patcher.Load(_projectXml);
            _patcher.PatchVersionField("1.0.0", "1.1.0-0");

            var newXml = _patcher.ToXml();
            Assert.NotEqual(_projectXml, newXml);
            Assert.Contains("<Version>1.1.0-0</Version>", newXml);
        }
        
        [Fact]
        public void CanPatchWhenVersionIsMissing()
        {
            var xml = 
            "<Project Sdk=\"Microsoft.NET.Sdk\">" +
            "<PropertyGroup>" +
            "<TargetFramework>netstandard1.6</TargetFramework>" +
            "<RootNamespace>Unit.For.The.Win</RootNamespace>" +
            "<PackageId>Unit.Testing.Library</PackageId>" +
            "</PropertyGroup>" +
            "</Project>";

            _patcher.Load(xml);
            _patcher.PatchVersionField("1.0.0", "2.0.0");
            var newXml = _patcher.ToXml();
            Assert.Contains("<Version>2.0.0</Version>", newXml);
        }
    }
}