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

        internal static VersionBump GetVersionBumpFromRemainingArgs(List<string> remainingArguments)
        {
            if (remainingArguments == null || !remainingArguments.Any())
            {
                var msgEx = "No version bump specified, please specify one of:\n\tmajor | minor | patch";
                // ReSharper disable once NotResolvedInText
                throw new ArgumentException(msgEx, "versionBump");
            }

            VersionBump bump = VersionBump.Patch;
            foreach (var arg in remainingArguments)
            {
                if (Enum.TryParse(arg, true, out bump)) break;

                var msg = $"Invalid version bump specified: {arg}";
                // ReSharper disable once NotResolvedInText
                throw new ArgumentException(msg, "versionBump");
            }

            return bump;
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