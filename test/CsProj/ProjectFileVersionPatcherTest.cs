using System;
using FakeItEasy;
using Skarp.Version.Cli.CsProj;
using Skarp.Version.Cli.CsProj.FileSystem;
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
        private readonly IFileSystemProvider _fileSystem;

        public ProjectFileVersionPatcherTest()
        {
            _fileSystem = A.Fake<IFileSystemProvider>();
            _patcher = new ProjectFileVersionPatcher(_fileSystem);
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

            var newXml = _patcher.ToXmlString();
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
            var newXml = _patcher.ToXmlString();
            Assert.Contains("<Version>2.0.0</Version>", newXml);
        } 
        
        [Fact]
        public void PreservesWhiteSpaceWhilePatching()
        {
            var xml = 
            "<Project Sdk=\"Microsoft.NET.Sdk\">" +
            "<PropertyGroup>" +
            "<Version>1.0.0</Version>" +
            "</PropertyGroup>" +
            $"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}" +
            "</Project>";

            _patcher.Load(xml);
            _patcher.PatchVersionField("1.0.0", "2.0.0");
            var newXml = _patcher.ToXmlString();
            Assert.Contains($"{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}", newXml);
        }

        [Fact]
        public void Flush_calls_filesystem()
        {
            _patcher.Load(_projectXml);

            var thePath = "/some/path.txt";
            _patcher.Flush(thePath);

            A.CallTo(() => _fileSystem.WriteAllContent(thePath, A<string>._)).MustHaveHappenedOnceExactly();
        }
    }
}