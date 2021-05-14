using System;
using System.IO;
using Skarp.Version.Cli.Vcs.Git;
using Xunit;

namespace Skarp.Version.Cli.Test.Vcs.Git
{
    public class GitVcsTest : IClassFixture<GitVcsFixture>
    {
        private readonly GitVcsFixture _fixture;


        public GitVcsTest(GitVcsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void ReturnsProperToolname()
        {
            Assert.Equal("git", _fixture.Vcs.ToolName());
        }

        [Fact]
        public void DetectingGitOnMachineWorks()
        {
            Assert.True(_fixture.Vcs.IsVcsToolPresent(_fixture.AbsolutePathToGitTestDir));
        }

        [Fact]
        public void IsRepositoryCleanWorks()
        {
            Assert.True(_fixture.Vcs.IsRepositoryClean(_fixture.AbsolutePathToGitTestDir));
        }

        [Fact]
        public void CanCommit()
        {
            // arrange
            var commitMessage = Guid.NewGuid().ToString("N");
            var fileToCommit = "dotnet-version.dll";
            File.Copy(fileToCommit, Path.Combine(_fixture.GitTestDir, fileToCommit));

            // act
            _fixture.Vcs.Commit(fileToCommit, commitMessage, _fixture.AbsolutePathToGitTestDir);

            // assert

            // grep the git-log for messages containing our guid message
            var (
                exitCode,
                stdOut,
                _
                ) = GitVcs.LaunchGitWithArgsInner($"log --grep={commitMessage}", 1000,
                _fixture.AbsolutePathToGitTestDir);
            Assert.Equal(0, exitCode);
            Assert.Contains(commitMessage, stdOut);
        }

        [Fact]
        public void CanCreateTags()
        {
            var tagToMake = Guid.NewGuid().ToString("N");

            _fixture.Vcs.Tag(tagToMake, _fixture.AbsolutePathToGitTestDir);

            var (exitCode, stdOut, _) =
                GitVcs.LaunchGitWithArgsInner(
                    "tag -l"
                    , 1000,
                    _fixture.AbsolutePathToGitTestDir
                );

            Assert.Equal(0, exitCode);
            Assert.Contains(tagToMake, stdOut);
        }
    }
}