using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Skarp.Version.Cli.Vcs.Git;

namespace Skarp.Version.Cli.Test.Vcs.Git
{
    public class GitVcsFixture : IDisposable
    {
        public readonly string GitTestDir;
        public readonly GitVcs Vcs;
        public readonly string AbsolutePathToGitTestDir;

        public GitVcsFixture()
        {
            GitTestDir = "./target-git-dir";
            AbsolutePathToGitTestDir = Path.Combine(
                Directory.GetCurrentDirectory(),
                GitTestDir
            );
            DirectoryDelete(GitTestDir, recursive: true);

            ZipFile.ExtractToDirectory("./target-git.zip", "./");
            Vcs = new GitVcs();

            var (_, stdOut, _) =
                GitVcs.LaunchGitWithArgsInner(
                    "config user.email",
                    1000,
                    AbsolutePathToGitTestDir
                );
            if (string.IsNullOrWhiteSpace(stdOut))
            {
                GitVcs.LaunchGitWithArgs("config user.email nicklas@skarp.dk");
                GitVcs.LaunchGitWithArgs("config user.name Nicklas Laine Overgaard");
            }
        }


        private void DirectoryDelete(string dir, bool recursive)
        {
            try
            {
                Directory.Delete(dir, recursive);
            }
            // we don't want to fail at all if deleting the dir fails
            catch (Exception ex)
            {
            }
        }

        public void Dispose()
        {
            DirectoryDelete(GitTestDir, recursive: true);
        }
    }
}