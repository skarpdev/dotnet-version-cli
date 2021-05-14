using System;
using System.Diagnostics;
using System.IO;

namespace Skarp.Version.Cli.Vcs.Git
{
    public class GitVcs : IVcs
    {
        /// <summary>
        /// Creates a new commit with the given message
        /// </summary>
        /// <param name="csProjFilePath">Path to the cs project file that was version updated</param>
        /// <param name="message">The message to include in the commit</param>
        /// <param name="cwd"></param>
        public void Commit(string csProjFilePath, string message, string cwd = null)
        {
            if(!LaunchGitWithArgs($"add \"{csProjFilePath}\"", cwd: cwd))
            {
                throw new OperationCanceledException($"Unable to add cs proj file {csProjFilePath} to git index");
            }

            if(!LaunchGitWithArgs($"commit -m \"{message}\"", cwd: cwd))
            {
                throw new OperationCanceledException("Unable to commit");
            }
        }

        /// <summary>
        /// Determines whether the current repository is clean.
        /// </summary>
        /// <returns></returns>
        public bool IsRepositoryClean(string cwd = null)
        {
            return LaunchGitWithArgs("diff-index HEAD --", cwd: cwd);
        }

        /// <summary>
        /// Determines whether git is present in PATH on the current computer
        /// </summary>
        /// <returns></returns>
        public bool IsVcsToolPresent(string cwd = null)
        {
            // launching `git --help` returns exit code 0 where as `git` returns 1 as git wants a cmd line argument
            return LaunchGitWithArgs("--help", cwd: cwd);
        }

        /// <summary>
        /// Creates a new tag
        /// </summary>
        /// <param name="tagName">Name of the tag</param>
        /// <param name="cwd"></param>
        public void Tag(string tagName, string cwd = null)
        {
            if(!LaunchGitWithArgs($"tag {tagName}", cwd: cwd))
            {
                throw new OperationCanceledException("Unable to create tag");
            }
        }

        /// <summary>
        /// Helper method for launching git with different arguments while returning just a boolean of whether the
        /// "command" was successful
        /// </summary>
        /// <param name="args">The args to pass onto git, e.g `diff` to launch `git diff`</param>
        /// <param name="waitForExitTimeMs">How long to wait for the git operation to complete</param>
        /// <param name="exitCode">The expected exit code</param>
        /// <param name="cwd">The working directory to change into, if any. Leave null for "current directory" </param>
        /// <returns></returns>
        internal static bool LaunchGitWithArgs(
            string args,
            int waitForExitTimeMs = 1000, 
            int exitCode = 0,
            string cwd = null
        )
        {
            try
            {
                var (procExitCode, stdOut, stdErr) = LaunchGitWithArgsInner(args, waitForExitTimeMs, cwd);
                return procExitCode == exitCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }

        internal static (int ExitCode, string stdOut, string stdErr) LaunchGitWithArgsInner(
            string args,
            int waitForExitTimeMs,
            string cwd = null
        )
        {
            var startInfo = CreateGitShellStartInfo(args, cwd);
            var proc = Process.Start(startInfo);
            proc.WaitForExit(waitForExitTimeMs);

            var stdOut = proc.StandardOutput.ReadToEnd();
            var stdErr = proc.StandardError.ReadToEnd();
            return (proc.ExitCode, stdOut,stdErr);
        }

        internal static ProcessStartInfo CreateGitShellStartInfo(string args, string cwd = null)
        {
            var procInfo =  new ProcessStartInfo("git")
            {
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };

            if (!string.IsNullOrWhiteSpace(cwd))
            {
                procInfo.WorkingDirectory = cwd;
            }
            return procInfo;
        }

        public string ToolName()
        {
            return "git";
        }
    }
}