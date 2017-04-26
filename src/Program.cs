using System;
using System.Collections.Generic;
using System.Linq;
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
                try
                {
                    if (commandLineApplication.RemainingArguments.Count == 0)
                    {
                        _cli.DumpVersion();
                        return 0;
                    }

                    var versionBump = GetVersionBumpFromRemainingArgs(commandLineApplication.RemainingArguments);
                    _cli.Execute(versionBump);

                    return 0;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"ERR: {ex.Message}");
                    return 1;
                }

                catch (OperationCanceledException oce)
                {
                    Console.WriteLine($"ERR {oce.Message}");
                    return 1;
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERR Something went completly haywire, developer zen:");
                    Console.WriteLine($"\t{e.Message} STACK: {Environment.NewLine}{e.StackTrace}");
                    return 1;
                }
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
    }
}