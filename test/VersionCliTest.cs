using System;
using FakeItEasy;
using Skarp.Version.Cli.CsProj;
using Skarp.Version.Cli.Vcs;
using Xunit;

namespace Skarp.Version.Cli.Test
{
    public class VersionCliTest
    {
        private IVcs _vcsTool;
        private ProjectFileDetector _fileDetector;
        private ProjectFileParser _fileParser;
        private ProjectFileVersionPatcher _filePatcher;
        private VersionCli _cli;

        public VersionCliTest()
        {
            _vcsTool = A.Fake<IVcs>(opts => opts.Strict());
            A.CallTo(() => _vcsTool.ToolName()).Returns("_FAKE_");

            _fileDetector = A.Fake<ProjectFileDetector>();
            _fileParser = A.Fake<ProjectFileParser>();
            _filePatcher = A.Fake<ProjectFileVersionPatcher>();
            
            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");

            _cli = new VersionCli(
                    _vcsTool,
                    _fileDetector,
                    _fileParser,
                    _filePatcher
                );

        }

        [Fact]
        public void VersionCli_throws_when_vcs_tool_is_not_present_and_doVcs_is_true()
        {
            A.CallTo(() => _vcsTool.IsVcsToolPresent()).Returns(false);

            var ex = Assert.Throws<OperationCanceledException>(() => _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.Major, DoVcs = true}));
            Assert.Equal("Unable to find the vcs tool _FAKE_ in your path", ex.Message);
        }
        
        [Fact]
        public void VersionCli_doesNotThrow_when_vcs_tool_is_not_present_if_doVcs_is_false()
        {
            A.CallTo(() => _vcsTool.IsVcsToolPresent()).Returns(false);
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            _cli.Execute(new VersionCliArgs {VersionBump = VersionBump.Major, DoVcs = false});
        }

        [Fact]
        public void VersionCli_throws_when_repo_is_not_clean_and_doVcs_is_true()
        {
            A.CallTo(() => _vcsTool.IsVcsToolPresent()).Returns(true);
            A.CallTo(() => _vcsTool.IsRepositoryClean()).Returns(false);

            var ex = Assert.Throws<OperationCanceledException>(() => _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.Major, DoVcs = true}));
            Assert.Equal("You currently have uncomitted changes in your repository, please commit these and try again",
                ex.Message);
        }
        
        [Fact]
        public void VersionCli_doesNotThrow_when_repo_is_not_clean_if_doVcs_is_false()
        {
            A.CallTo(() => _vcsTool.IsVcsToolPresent()).Returns(true);
            A.CallTo(() => _vcsTool.IsRepositoryClean()).Returns(false);
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.Major, DoVcs = false});
        }

        [Fact]
        public void VersionCli_can_bump_versions()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean()).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent()).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.Major, DoVcs = true, DryRun = false});

            // Verify
            A.CallTo(() => _filePatcher.PatchVersionField(
                    A<string>.That.Matches(ver => ver == "1.2.1"),
                    A<string>.That.Matches(newVer => newVer == "2.0.0")
                ))
                .MustHaveHappened(Repeated.Exactly.Once);
            
            A.CallTo(() => _filePatcher.PatchPackageVersionField(
                    A<string>.That.Matches(ver => ver == "1.2.1"),
                    A<string>.That.Matches(newVer => newVer == "2.0.0")
                ))
                .MustHaveHappened(Repeated.Exactly.Once);
            
            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(
                    A<string>.That.Matches(path => path == csProjFilePath),
                    A<string>.That.Matches(ver => ver == "v2.0.0")))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Tag(
                    A<string>.That.Matches(tag => tag == "v2.0.0")))
                .MustHaveHappened(Repeated.Exactly.Once);
        } 
        
        [Fact]
        public void VersionCli_can_bump_pre_release_versions()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean()).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent()).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.PreMajor, DoVcs = true, DryRun = false});

            // Verify
            A.CallTo(() => _filePatcher.PatchVersionField(
                    A<string>.That.Matches(ver => ver == "1.2.1"),
                    A<string>.That.Matches(newVer => newVer == "2.0.0")
                ))
                .MustHaveHappened(Repeated.Exactly.Once);
            
            A.CallTo(() => _filePatcher.PatchPackageVersionField(
                    A<string>.That.Matches(ver => ver == "1.2.1"),
                    A<string>.That.Matches(newVer => newVer == "2.0.0-0")
                ))
                .MustHaveHappened(Repeated.Exactly.Once);
            
            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(
                    A<string>.That.Matches(path => path == csProjFilePath),
                    A<string>.That.Matches(ver => ver == "v2.0.0-0")))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Tag(
                    A<string>.That.Matches(tag => tag == "v2.0.0-0")))
                .MustHaveHappened(Repeated.Exactly.Once);
        }  
        [Fact]
        public void VersionCli_can_bump_pre_release_with_build_meta_versions()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean()).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent()).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.PreMajor, DoVcs = true, DryRun = false, BuildMeta = "master"});

            // Verify
            A.CallTo(() => _filePatcher.PatchVersionField(
                    A<string>.That.Matches(ver => ver == "1.2.1"),
                    A<string>.That.Matches(newVer => newVer == "2.0.0")
                ))
                .MustHaveHappened(Repeated.Exactly.Once);
            
            A.CallTo(() => _filePatcher.PatchPackageVersionField(
                    A<string>.That.Matches(ver => ver == "1.2.1"),
                    A<string>.That.Matches(newVer => newVer == "2.0.0-0+master")
                ))
                .MustHaveHappened(Repeated.Exactly.Once);
            
            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(
                    A<string>.That.Matches(path => path == csProjFilePath),
                    A<string>.That.Matches(ver => ver == "v2.0.0-0+master")))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Tag(
                    A<string>.That.Matches(tag => tag == "v2.0.0-0+master")))
                .MustHaveHappened(Repeated.Exactly.Once);
        }
        
        [Fact]
        public void VersionCli_can_bump_versions_can_skip_vcs()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean()).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent()).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.Major, DoVcs = false, DryRun = false});

            // Verify
            A.CallTo(() => _filePatcher.PatchVersionField(
                    A<string>.That.Matches(ver => ver == "1.2.1"),
                    A<string>.That.Matches(newVer => newVer == "2.0.0")
                ))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _vcsTool.Tag(A<string>._)).MustNotHaveHappened();
        }
        
        [Fact]
        public void VersionCli_can_bump_versions_can_dry_run()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean()).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent()).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            var info = _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.Major, DoVcs = true, DryRun = true});

            Assert.NotEqual(info.OldVersion, info.NewVersion);
            Assert.Equal("2.0.0", info.NewVersion);
            
            // Verify
            A.CallTo(() => _filePatcher.PatchVersionField(
                    A<string>.That.Matches(ver => ver == "1.2.1"),
                    A<string>.That.Matches(newVer => newVer == "2.0.0")
                ))
                .MustNotHaveHappened();
            
            A.CallTo(() => _filePatcher.PatchPackageVersionField(
                    A<string>.That.Matches(ver => ver == "1.2.1"),
                    A<string>.That.Matches(newVer => newVer == "2.0.0")
                ))
                .MustNotHaveHappened();
            
            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustNotHaveHappened();
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._)).MustNotHaveHappened();
            A.CallTo(() => _vcsTool.Tag(A<string>._)).MustNotHaveHappened();
        }
    }
}