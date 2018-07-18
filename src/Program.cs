﻿using System;
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
            SetUpLogging();
            SetUpDependencies();

            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);

            /*CommandOption versionBump = commandLineApplication.Option(
              "major | minor | patch",
              "The version bump to apply",
              CommandOptionType.NoValue);*/


            commandLineApplication.HelpOption("-? | -h | --help");
            var outputFormatOption = commandLineApplication.Option(
                "-o | --output-format <format>",
                "Change output format, allowed values: json, text - default value is text",
                CommandOptionType.SingleValue);

            var skipVcsOption = commandLineApplication.Option(
                "-s | --skip-vcs", "Disable version control system changes - default is to tag and commit new version",
                CommandOptionType.NoValue);

            var doDryRun = commandLineApplication.Option(
                "-d | --dry-run",
                "Disable all changes to disk and vcs. Use to see what the changes would have been but without changing the csproj file nor committing or tagging.",
                CommandOptionType.NoValue);


            commandLineApplication.OnExecute(() =>
            {
                try
                {
                    var outputFormat = OutputFormat.Text;
                    if (outputFormatOption.HasValue())
                    {
                        outputFormat =
                            (OutputFormat) Enum.Parse(typeof(OutputFormat), outputFormatOption.Value(), true);
                    }

                    if (outputFormat == OutputFormat.Text)
                    {
                        Console.WriteLine($"{ProductInfo.Name} version {ProductInfo.Version}");
                    }

                    var doVcs = !skipVcsOption.HasValue();
                    var dryRunEnabled = doDryRun.HasValue();

                    if (commandLineApplication.RemainingArguments.Count == 0)
                    {
                        _cli.DumpVersion(new VersionCliArgs
                        {
                            OutputFormat = outputFormat
                        });
                        return 0;
                    }

                    var cliArgs = GetVersionBumpFromRemainingArgs(commandLineApplication.RemainingArguments,
                        outputFormat, doVcs, dryRunEnabled);
                    _cli.Execute(cliArgs);

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

        internal static VersionCliArgs GetVersionBumpFromRemainingArgs(List<string> remainingArguments,
            OutputFormat outputFormat, bool doVcs, bool dryRunEnabled)
        {
            if (remainingArguments == null || !remainingArguments.Any())
            {
                var msgEx =
                    "No version bump specified, please specify one of:\n\tmajor | minor | patch | <specific version>";
                // ReSharper disable once NotResolvedInText
                throw new ArgumentException(msgEx, "versionBump");
            }

            var args = new VersionCliArgs {OutputFormat = outputFormat, DoVcs = doVcs, DryRun = dryRunEnabled};
            var bump = VersionBump.Patch;

            foreach (var arg in remainingArguments)
            {
                if (Enum.TryParse(arg, true, out bump)) break;

                var ver = SemVer.FromString(arg);
                args.SpecificVersionToApply = ver.ToVersionString();
                bump = VersionBump.Specific;
            }

            args.VersionBump = bump;
            return args;
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