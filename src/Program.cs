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
            SetUpLogging();
            SetUpDependencies();

            var commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg: false);

            /*CommandOption versionBump = commandLineApplication.Option(
              "major | minor | patch",
              "The version bump to apply",
              CommandOptionType.NoValue);*/


            commandLineApplication.HelpOption("-? | -h | --help");
            var outputFormatOption = commandLineApplication.Option(
                "-o | --output-format <format>", "Change output format, allowed values: json, text - default value is text",
                CommandOptionType.SingleValue);

            commandLineApplication.OnExecute(() =>
            {
                try
                {
                    var outputFormat = OutputFormat.Text;
                    if (outputFormatOption.HasValue())
                    {
                        outputFormat = (OutputFormat) Enum.Parse(typeof(OutputFormat), outputFormatOption.Value(), true);
                    }

                    if (outputFormat == OutputFormat.Text)
                    {
                        Console.WriteLine("dotnet-version-cli");
                    }

                    if (commandLineApplication.RemainingArguments.Count == 0)
                    {
                        _cli.DumpVersion(new VersionCliArgs());
                        return 0;
                    }

                    var cliArgs = GetVersionBumpFromRemainingArgs(commandLineApplication.RemainingArguments, outputFormat);
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

        internal static VersionCliArgs GetVersionBumpFromRemainingArgs(List<string> remainingArguments, OutputFormat outputFormat)
        {
            if (remainingArguments == null || !remainingArguments.Any())
            {
                var msgEx = "No version bump specified, please specify one of:\n\tmajor | minor | patch | <specific version>";
                // ReSharper disable once NotResolvedInText
                throw new ArgumentException(msgEx, "versionBump");
            }

            var args = new VersionCliArgs { OutputFormat = outputFormat };
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