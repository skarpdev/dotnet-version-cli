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
                    "</PropertyGroup>" +
                    "</Project>";

        private readonly ProjectFileVersionPatcher _patcher;

        public ProjectFileVersionPatcherTest()
        {
            _patcher = new ProjectFileVersionPatcher();
        }

        [Fact]
        public void CanPatchVersionOnWellFormedXml()
        {
            var newXml = _patcher.Patch(_projectXml, "1.0.0", "1.1.0");

            Assert.NotEqual(_projectXml, newXml);
            Assert.Contains("<Version>1.1.0</Version>", newXml);
        }
    }
}