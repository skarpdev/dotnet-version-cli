using System;
using System.Diagnostics;

namespace Skarp.Version.Cli.Vcs.Git
{
    public class GitVcs : IVcs
    {
        /// <summary>
        /// Creates a new commit with the given message
        /// </summary>
        /// <param name="csProjFilePath">Path to the cs project file that was version updated</param>
        /// <param name="message">The message to include in the commit</param>
        public void Commit(string csProjFilePath, string message)
        {
            if(!LaunchGitWithArgs($"add \"{csProjFilePath}\""))
            {
                throw new OperationCanceledException($"Unable to add cs proj file {csProjFilePath} to git index");
            }

            if(!LaunchGitWithArgs($"commit -m \"{message}\""))
            {
                throw new OperationCanceledException("Unable to commit");
            }
        }

        /// <summary>
        /// Determines whether the current repository is clean.
        /// </summary>
        /// <returns></returns>
        public bool IsRepositoryClean()
        {
            return LaunchGitWithArgs("diff-index --quiet HEAD --");
        }

        /// <summary>
        /// Determines whether git is present in PATH on the current computer
        /// </summary>
        /// <returns></returns>
        public bool IsVcsToolPresent()
        {
            // launching `git --help` returns exit code 0 where as `git` returns 1 as git wants a cmd line argument
            return LaunchGitWithArgs("--help");
        }

        /// <summary>
        /// Creates a new tag
        /// </summary>
        /// <param name="tagName">Name of the tag</param>
        public void Tag(string tagName)
        {
            if(!LaunchGitWithArgs($"tag -a {tagName} -m {tagName}"))
            {
                throw new OperationCanceledException("Unable to create tag");
            }
        }

        private static bool LaunchGitWithArgs(string args, int waitForExitTimeMs = 1000, int exitCode = 0)
        {
            try
            {
                var startInfo = CreateGitShellStartInfo(args);
                var proc = Process.Start(startInfo);
                proc.WaitForExit(waitForExitTimeMs);
                
                return proc.ExitCode == exitCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }

        private static ProcessStartInfo CreateGitShellStartInfo(string args)
        {
            return new ProcessStartInfo("git")
            {
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };
        }

        public string ToolName()
        {
            return "git";
        }
    }
}