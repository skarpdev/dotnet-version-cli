using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Skarp.Version.Cli.CsProj;
using Skarp.Version.Cli.Model;
using Skarp.Version.Cli.Vcs;
using Skarp.Version.Cli.Versioning;

namespace Skarp.Version.Cli
{
    public class VersionCli
    {
        private readonly IVcs _vcsTool;
        private readonly ProjectFileDetector _fileDetector;
        private readonly ProjectFileParser _fileParser;
        private readonly VcsParser _vcsParser;
        private readonly ProjectFileVersionPatcher _fileVersionPatcher;
        private readonly SemVerBumper _bumper;

        public VersionCli(
            IVcs vcsClient,
            ProjectFileDetector fileDetector,
            ProjectFileParser fileParser,
            VcsParser vcsParser,
            ProjectFileVersionPatcher fileVersionPatcher,
            SemVerBumper bumper
        )
        {
            _vcsTool = vcsClient;
            _fileDetector = fileDetector;
            _fileParser = fileParser;
            _vcsParser = vcsParser;
            _fileVersionPatcher = fileVersionPatcher;
            _bumper = bumper;
        }

        public VersionInfo Execute(VersionCliArgs args)
        {
            if (!args.DryRun && args.DoVcs && !_vcsTool.IsVcsToolPresent())
            {
                throw new OperationCanceledException(
                    $"Unable to find the vcs tool {_vcsTool.ToolName()} in your path");
            }

            if (!args.DryRun && args.DoVcs && !_vcsTool.IsRepositoryClean())
            {
                throw new OperationCanceledException(
                    "You currently have uncomitted changes in your repository, please commit these and try again");
            }

            var csProjXml = _fileDetector.FindAndLoadCsProj(args.CsProjFilePath);
            _fileParser.Load(csProjXml);

            var semVer = _bumper.Bump(
                SemVer.FromString(_fileParser.PackageVersion),
                args.VersionBump,
                args.SpecificVersionToApply,
                args.BuildMeta,
                args.PreReleasePrefix
            );
            var versionString = semVer.ToSemVerVersionString();

            var theOutput = new VersionInfo
            {
                Product = new ProductOutputInfo
                {
                    Name = ProductInfo.Name,
                    Version = ProductInfo.Version
                },
                OldVersion = _fileParser.PackageVersion,
                NewVersion = versionString,
                ProjectFile = _fileDetector.ResolvedCsProjFile,
                VersionStrategy = args.VersionBump.ToString().ToLowerInvariant()
            };

            if (!args.DryRun) // if we are not in dry run mode, then we should go ahead
            {
                _fileVersionPatcher.Load(csProjXml);

                _fileVersionPatcher.PatchVersionField(
                    _fileParser.Version,
                    versionString
                );

                _fileVersionPatcher.Flush(
                    _fileDetector.ResolvedCsProjFile
                );

                if (args.DoVcs)
                {
                    // Run git commands
                    _vcsTool.Commit(_fileDetector.ResolvedCsProjFile, _vcsParser.Commit(theOutput, _fileParser, args.CommitMessage));
                    _vcsTool.Tag(_vcsParser.Tag(theOutput, _fileParser, args.VersionControlTag));
                }
            }

            if (args.OutputFormat == OutputFormat.Json)
            {
                WriteJsonToStdout(theOutput);
            }
            else
            {
                Console.WriteLine($"Bumped {_fileDetector.ResolvedCsProjFile} to version {versionString}");
            }

            return theOutput;
        }

        public void DumpVersion(VersionCliArgs args)
        {
            var csProjXml = _fileDetector.FindAndLoadCsProj(args.CsProjFilePath);
            _fileParser.Load(csProjXml);

            if (args.OutputFormat == OutputFormat.Json)
            {
                var theOutput = new
                {
                    Product = new
                    {
                        Name = ProductInfo.Name,
                        Version = ProductInfo.Version
                    },
                    CurrentVersion = _fileParser.PackageVersion,
                    ProjectFile = _fileDetector.ResolvedCsProjFile,
                };
                WriteJsonToStdout(theOutput);
            }
            else
            {
                Console.WriteLine("Project version is: {0}\t{1}", Environment.NewLine, _fileParser.PackageVersion);
            }
        }

        private static void WriteJsonToStdout(object theOutput)
        {
            Console.WriteLine(
                JsonConvert.SerializeObject(
                    theOutput, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    }));
        }
    }
}