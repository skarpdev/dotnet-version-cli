using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.CommandLineUtils;
using Skarp.Version.Cli.CsProj;
using Skarp.Version.Cli.CsProj.FileSystem;
using Skarp.Version.Cli.Vcs.Git;

namespace Skarp.Version.Cli
{
    class Program
    {
        private static VersionCli _cli;

        static void Main(string[] args)
        {
            Console.WriteLine("dotnet-version-cli");
            SetUpLogging();
            SetUpDependencies();

            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);

            /*CommandOption versionBump = commandLineApplication.Option(
              "major | minor | patch",
              "The version bump to apply",
              CommandOptionType.NoValue);*/


            commandLineApplication.HelpOption("-? | -h | --help");
            commandLineApplication.OnExecute(() =>
            {
                if (commandLineApplication.RemainingArguments.Count == 0)
                {
                    DumpVersion();
                    return 0;
                }

                var versionBump = GetVersionBumpFromRemainingArgs(commandLineApplication.RemainingArguments);
                PatchVersion(versionBump);

                return 0;
            });
            commandLineApplication.Execute(args);
        }

        private static VersionBump GetVersionBumpFromRemainingArgs(List<string> remainingArguments)
        {
            var regex = new Regex(@"(major)|(minor)|(patch)", RegexOptions.Compiled);
            foreach (var arg in remainingArguments)
            {
                if (!regex.IsMatch(arg)) continue;
                VersionBump bump;
                if (Enum.TryParse(arg, true, out bump)) return bump;

                Console.WriteLine($"Invalid version bump specified: {arg}");
                Environment.Exit(1);
                return bump;
            }
            Console.WriteLine("No version bump specified, please specify one of:\n\tmajor | minor | patch");
            Environment.Exit(1);

            return VersionBump.Patch;
        }

        private static void SetUpLogging()
        {
        }

        private static void SetUpDependencies()
        {
            _cli = new VersionCli(
                new GitVcs(),
                new ProjectFileDetector(
                    new DotNetFileSystemProvider()
                ),
                new ProjectFileParser(),
                new ProjectFileVersionPatcher()
            );
        }

        private static void DumpVersion(string path = "")
        {
            _cli.DumpVersion(path);
            Environment.Exit(0);
        }

        private static void PatchVersion(VersionBump bump, string commitMsg = "", string path = "")
        {
            _cli.Execute(
                bump,
                commitMsg,
                path
            );
        }
    }
}