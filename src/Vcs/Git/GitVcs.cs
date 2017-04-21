using System;

namespace Skarpdev.DotnetVersion.Vcs.Git
{
    public class GitVcs : IVcs
    {
        public void Commit(string message)
        {
            throw new NotImplementedException();
        }

        public bool IsRepositoryClean()
        {
            // run : git diff-index --quiet HEAD --
            // exit code 0 means that all is good!
            throw new NotImplementedException();
        }

        public void Tag(string tagName)
        {
            throw new NotImplementedException();
        }
    }
}