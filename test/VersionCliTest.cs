using System;
using FakeItEasy;
using Skarp.Version.Cli.CsProj;
using Skarp.Version.Cli.Vcs;
using Skarp.Version.Cli.Versioning;
using Xunit;

namespace Skarp.Version.Cli.Test
{
    public class VersionCliTest
    {
        private IVcs _vcsTool;
        private ProjectFileDetector _fileDetector;
        private ProjectFileParser _fileParser;
        private VcsParser _vcsParser;
        private ProjectFileVersionPatcher _filePatcher;
        private VersionCli _cli;

        public VersionCliTest()
        {
            _vcsTool = A.Fake<IVcs>(opts => opts.Strict());
            A.CallTo(() => _vcsTool.ToolName()).Returns("_FAKE_");

            _fileDetector = A.Fake<ProjectFileDetector>();
            _fileParser = A.Fake<ProjectFileParser>();
            _vcsParser = A.Fake<VcsParser>();
            _filePatcher = A.Fake<ProjectFileVersionPatcher>();
            
            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.VersionPrefix).Returns("2.0.1");
            A.CallTo(() => _fileParser.VersionSuffix).Returns("DEVELOPMENT");

            _cli = new VersionCli(
                    _vcsTool,
                    _fileDetector,
                    _fileParser,
                    _vcsParser,
                    _filePatcher,
                    new SemVerBumper()
                );
        }

        [Fact]
        public void VersionCli_Bump_VersionPrefix()
        {
            A.CallTo(() => _fileParser.Version).Returns(null);
            A.CallTo(() => _fileParser.VersionPrefix).Returns("2.0.1");
            A.CallTo(() => _fileParser.VersionSuffix).Returns("DEVELOPMENT");

            var output = _cli.Execute(new VersionCliArgs { OutputFormat = OutputFormat.Bare, VersionBump = VersionBump.None, DryRun = true });

            Assert.Equal("2.0.1-DEVELOPMENT", output.OldVersion);
        }

        [Fact]
        public void VersionCli_throws_when_vcs_tool_is_not_present_and_doVcs_is_true()
        {
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(false);

            var ex = Assert.Throws<OperationCanceledException>(() => _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.Major, DoVcs = true}));
            Assert.Equal("Unable to find the vcs tool _FAKE_ in your path", ex.Message);
        }
        
        [Fact]
        public void VersionCli_doesNotThrow_when_vcs_tool_is_not_present_if_doVcs_is_false()
        {
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(false);
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            _cli.Execute(new VersionCliArgs {VersionBump = VersionBump.Major, DoVcs = false});
        }

        [Fact]
        public void VersionCli_throws_when_repo_is_not_clean_and_doVcs_is_true()
        {
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(false);

            var ex = Assert.Throws<OperationCanceledException>(() => _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.Major, DoVcs = true}));
            Assert.Equal("You currently have uncomitted changes in your repository, please commit these and try again",
                ex.Message);
        }
        
        [Fact]
        public void VersionCli_doesNotThrow_when_repo_is_not_clean_if_doVcs_is_false()
        {
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(false);
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.Major, DoVcs = false});
        }

        [Fact]
        public void VersionCli_can_bump_versions()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._, null)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.Major, DoVcs = true, DryRun = false});

            // Verify
            A.CallTo(() => _filePatcher.PatchField(
                    A<string>.That.Matches(newVer => newVer == "2.0.0"),
                    ProjectFileProperty.Version
                ))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(
                    A<string>.That.Matches(path => path == csProjFilePath),
                    A<string>.That.Matches(msg => msg == "v2.0.0"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Tag(
                    A<string>.That.Matches(tag => tag == "v2.0.0"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
        } 
        
        [Fact]
        public void VersionCli_can_bump_pre_release_versions()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._ ,null)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.PreMajor, DoVcs = true, DryRun = false});

            // Verify
            A.CallTo(() => _filePatcher.PatchField(
                    A<string>.That.Matches(newVer => newVer == "2.0.0-next.0"),
                    ProjectFileProperty.Version
                ))
                .MustHaveHappened(Repeated.Exactly.Once);
            
            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(
                    A<string>.That.Matches(path => path == csProjFilePath),
                    A<string>.That.Matches(msg => msg == "v2.0.0-next.0"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Tag(
                    A<string>.That.Matches(tag => tag == "v2.0.0-next.0"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }  
        
        [Fact]
        public void VersionCli_can_bump_pre_release_with_custom_prefix()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._, null)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.PreMajor, DoVcs = true, DryRun = false, PreReleasePrefix = "beta"});

            // Verify
            A.CallTo(() => _filePatcher.PatchField(
                    "2.0.0-beta.0",
                    ProjectFileProperty.Version
                ))
                .MustHaveHappened(Repeated.Exactly.Once);
            
            A.CallTo(() => _filePatcher.Flush(
                    csProjFilePath))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(
                    csProjFilePath,
                    "v2.0.0-beta.0", null))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Tag(
                    "v2.0.0-beta.0", null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }  
        
        [Fact]
        public void VersionCli_can_bump_pre_release_with_build_meta_versions()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._ ,null)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.PreMajor, DoVcs = true, DryRun = false, BuildMeta = "master"});

            // Verify
            A.CallTo(() => _filePatcher.PatchField(
                    A<string>.That.Matches(newVer => newVer == "2.0.0-next.0+master"),
                    ProjectFileProperty.Version
                ))
                .MustHaveHappened(Repeated.Exactly.Once);
            
            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(
                    A<string>.That.Matches(path => path == csProjFilePath),
                    A<string>.That.Matches(msg => msg == "v2.0.0-next.0+master"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Tag(
                    A<string>.That.Matches(tag => tag == "v2.0.0-next.0+master"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }
        
        [Fact]
        public void VersionCli_can_bump_versions_can_skip_vcs()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._, null)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            _cli.Execute(new VersionCliArgs{VersionBump = VersionBump.Major, DoVcs = false, DryRun = false});

            // Verify
            A.CallTo(() => _filePatcher.PatchField(
                    A<string>.That.Matches(newVer => newVer == "2.0.0"),
                    ProjectFileProperty.Version
                ))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._, null)).MustNotHaveHappened();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).MustNotHaveHappened();
        }
        
        [Fact]
        public void VersionCli_can_bump_versions_can_dry_run()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._, null)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).DoesNothing();

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
            A.CallTo(() => _filePatcher.PatchField(
                    A<string>.That.Matches(newVer => newVer == "2.0.0"),
                    ProjectFileProperty.Version
                ))
                .MustNotHaveHappened();

            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustNotHaveHappened();
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._, null)).MustNotHaveHappened();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).MustNotHaveHappened();
        }

        [Fact]
        public void VersionCli_can_set_vcs_commit_message()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._, null)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            _cli.Execute(new VersionCliArgs { VersionBump = VersionBump.Major, DoVcs = true, DryRun = false, CommitMessage = "commit message" });

            // Verify
            A.CallTo(() => _filePatcher.PatchField(
                    A<string>.That.Matches(newVer => newVer == "2.0.0"),
                    ProjectFileProperty.Version
                ))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(
                    A<string>.That.Matches(path => path == csProjFilePath),
                    A<string>.That.Matches(msg => msg == "commit message"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Tag(
                    A<string>.That.Matches(tag => tag == "v2.0.0"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void VersionCli_can_set_vcs_commit_message_with_variables()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._, null)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageName).Returns("unit-test");

            // Act
            _cli.Execute(new VersionCliArgs { VersionBump = VersionBump.Major, DoVcs = true, DryRun = false, CommitMessage = "bump from v$oldVer to v$newVer at $projName" });

            // Verify
            A.CallTo(() => _filePatcher.PatchField(
                    A<string>.That.Matches(newVer => newVer == "2.0.0"),
                    ProjectFileProperty.Version
                ))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(
                    A<string>.That.Matches(path => path == csProjFilePath),
                    A<string>.That.Matches(msg => msg == "bump from v1.2.1 to v2.0.0 at unit-test"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Tag(
                    A<string>.That.Matches(tag => tag == "v2.0.0"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void VersionCli_can_set_vcs_tag()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._, null)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");

            // Act
            _cli.Execute(new VersionCliArgs { VersionBump = VersionBump.Major, DoVcs = true, DryRun = false, VersionControlTag = "vcs tag" });

            // Verify
            A.CallTo(() => _filePatcher.PatchField(
                    A<string>.That.Matches(newVer => newVer == "2.0.0"),
                    ProjectFileProperty.Version
                ))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(
                    A<string>.That.Matches(path => path == csProjFilePath),
                    A<string>.That.Matches(msg => msg == "v2.0.0"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Tag(
                    A<string>.That.Matches(tag => tag == "vcs tag"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public void VersionCli_can_set_vcs_tag_with_variables()
        {
            // Configure
            A.CallTo(() => _vcsTool.IsRepositoryClean(null)).Returns(true);
            A.CallTo(() => _vcsTool.IsVcsToolPresent(null)).Returns(true);
            A.CallTo(() => _vcsTool.Commit(A<string>._, A<string>._, null)).DoesNothing();
            A.CallTo(() => _vcsTool.Tag(A<string>._, null)).DoesNothing();

            A.CallTo(() => _fileDetector.FindAndLoadCsProj(A<string>._)).Returns("<Project/>");
            const string csProjFilePath = "/unit-test/test.csproj";
            A.CallTo(() => _fileDetector.ResolvedCsProjFile).Returns(csProjFilePath);

            A.CallTo(() => _fileParser.Load(A<string>._)).DoesNothing();
            A.CallTo(() => _fileParser.Version).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageVersion).Returns("1.2.1");
            A.CallTo(() => _fileParser.PackageName).Returns("unit-test");

            // Act
            _cli.Execute(new VersionCliArgs { VersionBump = VersionBump.Major, DoVcs = true, DryRun = false, VersionControlTag = "bump from v$oldVer to v$newVer at $projName" });

            // Verify
            A.CallTo(() => _filePatcher.PatchField(
                    A<string>.That.Matches(newVer => newVer == "2.0.0"),
                    ProjectFileProperty.Version
                ))
                .MustHaveHappened(Repeated.Exactly.Once);

            A.CallTo(() => _filePatcher.Flush(
                    A<string>.That.Matches(path => path == csProjFilePath)))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Commit(
                    A<string>.That.Matches(path => path == csProjFilePath),
                    A<string>.That.Matches(msg => msg == "v2.0.0"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(() => _vcsTool.Tag(
                    A<string>.That.Matches(tag => tag == "bump from v1.2.1 to v2.0.0 at unit-test"), null))
                .MustHaveHappened(Repeated.Exactly.Once);
        }
    }
}