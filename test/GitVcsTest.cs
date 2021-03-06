using Skarp.Version.Cli.Vcs.Git;
using Xunit;

namespace Skarp.Version.Cli.Test
{
    public class GitVcsTest
    {
        private readonly GitVcs _vcs;

        public GitVcsTest()
        {
            _vcs = new GitVcs();
        }

        [Fact(
            Skip = "Dont run on build servers"
        )]
        public void DetectingGitOnMachineWorks()
        {
            Assert.True(_vcs.IsVcsToolPresent());
        }

        [Fact(
            Skip = "Dont run on build servers"
        )]
        public void IsRepositoryCleanWorks()
        {
            Assert.True(_vcs.IsRepositoryClean());
        }
    }
}