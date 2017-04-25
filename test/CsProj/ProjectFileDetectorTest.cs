using System;
using System.Collections.Generic;
using FakeItEasy;
using Skarp.Version.Cli.CsProj;
using Skarp.Version.Cli.CsProj.FileSystem;
using Xunit;

namespace Skarp.Version.Cli.Test.CsProj
{
    public class ProjectFileDetectorTest
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

        [Fact]
        public void CanDetectCsProjFileWithGivenBootstrapFolder()
        {
            const string rootPath = "/unit-test";
            var theCsProjFile = $"{rootPath}/test.csproj";

            var fakeFileSystem = A.Fake<IFileSystemProvider>(opts => opts.Strict());

            A.CallTo(() => fakeFileSystem.List(A<string>._)).Returns(
                new List<string>{
                    theCsProjFile,
                    $"{rootPath}/Test.cs",
                }
            );
            A.CallTo(() =>
                fakeFileSystem.IsCsProjectFile(
                    A<string>.That.Matches(str => str == $"{rootPath}")))
                .Returns(false);
            A.CallTo(() =>
                fakeFileSystem.LoadContent(A<string>.That.Matches(str => str == theCsProjFile))
            ).Returns(_projectXml);

            var detect = new ProjectFileDetector(fakeFileSystem);
            var xml = detect.FindAndLoadCsProj(rootPath);

            Assert.Equal(_projectXml, xml);
            Assert.Equal(theCsProjFile, detect.ResolvedCsProjFile);
        }

        [Fact]
        public void AbortsWhenMoreThanOneCsprojFile()
        {
            const string rootPath = "/unit-test";

            var fakeFileSystem = A.Fake<IFileSystemProvider>(opts => opts.Strict());
            A.CallTo(() => fakeFileSystem.List(A<string>._)).Returns(
                new List<string>{
                    $"{rootPath}/test.csproj",
                    $"{rootPath}/other.csproj",
                    $"{rootPath}/Test.cs",
                }
            );
            A.CallTo(() =>
                fakeFileSystem.IsCsProjectFile(
                    A<string>.That.Matches(str => str == $"{rootPath}")))
                .Returns(false);


            var detect = new ProjectFileDetector(fakeFileSystem);
            Assert.Throws<OperationCanceledException>(() => detect.FindAndLoadCsProj(rootPath));
        }

        [Fact]
        public void Aborts_when_no_csproj_file()
        {
            const string rootPath = "/unit-test";

            var fakeFileSystem = A.Fake<IFileSystemProvider>(opts => opts.Strict());
            A.CallTo(() => fakeFileSystem.List(A<string>._)).Returns(
                new List<string>{
                    $"{rootPath}/Test.cs",
                }
            );
            A.CallTo(() =>
                    fakeFileSystem.IsCsProjectFile(
                        A<string>.That.Matches(str => str == $"{rootPath}")))
                .Returns(false);


            var detect = new ProjectFileDetector(fakeFileSystem);
            Assert.Throws<OperationCanceledException>(() => detect.FindAndLoadCsProj(rootPath));
        }

        [Fact]
        public void CanDetectCsProjFileWithGivenBootstrapCsProj()
        {
            const string rootPath = "/unit-test";
            var theCsProjFile = $"{rootPath}/test.csproj";

            var fakeFileSystem = A.Fake<IFileSystemProvider>(opts => opts.Strict());
            A.CallTo(() => fakeFileSystem.List(A<string>._)).Returns(
                new List<string>{
                    theCsProjFile,
                    $"{rootPath}/Test.cs",
                }
            );
            A.CallTo(() =>
                fakeFileSystem.IsCsProjectFile(
                    A<string>.That.Matches(str => str == theCsProjFile)))
                .Returns(true);
            A.CallTo(() =>
                fakeFileSystem.LoadContent(A<string>.That.Matches(str => str == theCsProjFile))
            ).Returns(_projectXml);

            var detect = new ProjectFileDetector(fakeFileSystem);
            var xml = detect.FindAndLoadCsProj(theCsProjFile);

            Assert.Equal(_projectXml, xml);
            Assert.Equal(theCsProjFile, detect.ResolvedCsProjFile);
        }

        [Fact]
        public void CanDetectProjectFileWithEmptyBootstrapPath()
        {
            const string rootPath = "/unit-test";
            var theCsProjFile = $"{rootPath}/test.csproj";

            var fakeFileSystem = A.Fake<IFileSystemProvider>(opts => opts.Strict());
            A.CallTo(() => fakeFileSystem.List(A<string>._)).Returns(
                new List<string>{
                    theCsProjFile,
                    $"{rootPath}/Test.cs",
                }
            );
            A.CallTo(() =>
                fakeFileSystem.Cwd()
            ).Returns(rootPath);
            A.CallTo(() =>
                fakeFileSystem.IsCsProjectFile(
                    A<string>.That.Matches(str => str == $"{rootPath}")))
                .Returns(false);
            A.CallTo(() =>
                fakeFileSystem.LoadContent(A<string>.That.Matches(str => str == theCsProjFile))
            ).Returns(_projectXml);

            var detect = new ProjectFileDetector(fakeFileSystem);
            var xml = detect.FindAndLoadCsProj("");

            Assert.Equal(_projectXml, xml);
            Assert.Equal(theCsProjFile, detect.ResolvedCsProjFile);
        }

    }
}