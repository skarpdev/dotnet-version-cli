﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using Skarp.Version.Cli.CsProj;
using Skarp.Version.Cli.CsProj.FileSystem;
using Skarp.Version.Cli.Vcs;
using Skarp.Version.Cli.Vcs.Git;
using Skarp.Version.Cli.Versioning;

namespace Skarp.Version.Cli
{
    static class Program
    {
        private static VersionCli _cli;

        static int Main(string[] args)
        {
            SetUpDependencies();

            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false)
            {
                Name = "dotnet version",
                ExtendedHelpText =
                    $"{Environment.NewLine}Available commands after [options] to control the version bump are: {Environment.NewLine}\tmajor | minor | patch | premajor | preminor | prepatch | prerelease | <specific version>"
            };

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

            var csProjectFileOption = commandLineApplication.Option(
                "-f | --project-file <path/to/csproj>",
                "The project file to work on. Defaults to auto-locating in current directory",
                CommandOptionType.SingleValue);

            var buildMetaOption = commandLineApplication.Option(
                "-b | --build-meta <the-build-meta>",
                "Additional build metadata to add to a `premajor`, `preminor` or `prepatch` version bump",
                CommandOptionType.SingleValue);

            var prefixOption = commandLineApplication.Option(
                "-p | --prefix <the-prerelease-prefix>",
                "Override the default next prefix/label for a  `premajor`, `preminor` or `prepatch` version bump",
                CommandOptionType.SingleValue);

            var commitMessage = commandLineApplication.Option(
                "-m | --message <the-commit-message>",
                "Set commit's message - default is 'v<version>'. Available variables: $projName, $oldVer, $newVer",
                CommandOptionType.SingleValue);

            var vcsTag = commandLineApplication.Option(
                "-t | --tag <git-tag>",
                "Set tag's name - default is 'v<version>'. Available variables: $projName, $oldVer, $newVer",
                CommandOptionType.SingleValue);

            var projectFilePropertyName = commandLineApplication.Option(
                "-v | --version-property-name <property-name>",
                "Specify which tag from <PropertyGroup> to use as the version tag. Default is Version. Available values: Version, PackageVersion.",
                CommandOptionType.SingleValue);

            var skipHooks = commandLineApplication.Option(
                "-sh | --skip-hooks",
                "Skip all commit hooks during commit",
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
                    var skipHooksEnabled = skipHooks.HasValue();

                    if (commandLineApplication.RemainingArguments.Count == 0)
                    {
                        _cli.DumpVersion(new VersionCliArgs
                        {
                            OutputFormat = outputFormat,
                            CsProjFilePath = csProjectFileOption.Value(),
                            ProjectFilePropertyName = Enum.Parse<ProjectFileProperty>(projectFilePropertyName.Value() ?? "Version", ignoreCase: true),
                        });
                        
                        return 0;
                    }

                    var cliArgs = GetVersionBumpFromRemainingArgs(
                        commandLineApplication.RemainingArguments,
                        outputFormat,
                        doVcs,
                        dryRunEnabled,
                        csProjectFileOption.Value(),
                        buildMetaOption.Value(),
                        prefixOption.Value(),
                        commitMessage.Value(),
                        vcsTag.Value(),
                        projectFilePropertyName.Value(),
                        skipHooksEnabled
                    );
                    _cli.Execute(cliArgs);

                    return 0;
                }
                catch (ArgumentException ex)
                {
                    Console.Error.WriteLine($"ERR {ex.Message}");

                    commandLineApplication.ShowHelp();
                    return 1;
                }

                catch (OperationCanceledException oce)
                {
                    Console.Error.WriteLine($"ERR {oce.Message}");

                    commandLineApplication.ShowHelp();
                    return 1;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("ERR Something went completely haywire, developer zen:");
                    Console.Error.WriteLine($"\t{e.Message} STACK: {Environment.NewLine}{e.StackTrace}");
                    return 1;
                }
            });
            return commandLineApplication.Execute(args);
        }

        internal static VersionCliArgs GetVersionBumpFromRemainingArgs(
            List<string> remainingArguments,
            OutputFormat outputFormat,
            bool doVcs,
            bool dryRunEnabled,
            string userSpecifiedCsProjFilePath,
            string userSpecifiedBuildMeta,
            string preReleasePrefix,
            string commitMessage,
            string vcsTag,
            string projectFilePropertyName,
            bool skipHooks
        )
        {
            if (remainingArguments == null || !remainingArguments.Any())
            {
                var msgEx =
                    "No version bump specified, please specify one of:\n\tmajor | minor | patch | premajor | preminor | prepatch | prerelease | <specific version>";
                // ReSharper disable once NotResolvedInText
                throw new ArgumentException(msgEx);
            }

            var args = new VersionCliArgs
            {
                OutputFormat = outputFormat,
                DoVcs = doVcs,
                DryRun = dryRunEnabled,
                BuildMeta = userSpecifiedBuildMeta,
                PreReleasePrefix = preReleasePrefix,
                CommitMessage = commitMessage,
                VersionControlTag = vcsTag,
                SkipHooks = skipHooks,
            };
            
            var bump = VersionBump.Patch;

            foreach (var arg in remainingArguments)
            {
                if (Enum.TryParse(arg, true, out bump)) break;

                var ver = SemVer.FromString(arg);
                args.SpecificVersionToApply = ver.ToSemVerVersionString(null);
                bump = VersionBump.Specific;
            }

            args.VersionBump = bump;
            args.CsProjFilePath = userSpecifiedCsProjFilePath;
            
            if (!string.IsNullOrEmpty(projectFilePropertyName))
            {
                args.ProjectFilePropertyName = Enum.Parse<ProjectFileProperty>(projectFilePropertyName, ignoreCase: true);
            }

            return args;
        }

        private static void SetUpDependencies()
        {
            var dotNetFileSystemProvider = new DotNetFileSystemProvider();
            _cli = new VersionCli(
                new GitVcs(),
                new ProjectFileDetector(
                    dotNetFileSystemProvider
                ),
                new ProjectFileParser(),
                new VcsParser(),
                new ProjectFileVersionPatcher(dotNetFileSystemProvider),
                new SemVerBumper()
            );
        }
    }
}