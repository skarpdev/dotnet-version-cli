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
            _fileParser.Load(
                csProjXml, 
                args.ProjectFilePropertyName);



            var currentSemVer = SemVer.FromString(
                                    _fileParser.VersionSource == ProjectFileProperty.Version ? 
                                        _fileParser.Version : 
                                        _fileParser.VersionPrefix);

            var bumpedSemVer = _bumper.Bump(
                currentSemVer,
                args.VersionBump,
                args.SpecificVersionToApply,
                args.BuildMeta,
                args.PreReleasePrefix
            );

            var theOutput = new VersionInfo
            {
                Product = new ProductOutputInfo
                {
                    Name = ProductInfo.Name,
                    Version = ProductInfo.Version
                },
                OldVersion = currentSemVer.ToSemVerVersionString(_fileParser),
                NewVersion = bumpedSemVer.ToSemVerVersionString(_fileParser),
                ProjectFile = _fileDetector.ResolvedCsProjFile,
                VersionStrategy = args.VersionBump.ToString().ToLowerInvariant()
            };

            if (!args.DryRun) // if we are not in dry run mode, then we should go ahead
            {
                _fileVersionPatcher.Load(csProjXml);

                _fileVersionPatcher.PatchField(
                    bumpedSemVer.ToSemVerVersionString(_fileParser),
                    _fileParser.VersionSource
                );

                _fileVersionPatcher.Flush(
                    _fileDetector.ResolvedCsProjFile
                );

                if (args.DoVcs)
                {
                    _fileParser.Load(csProjXml, ProjectFileProperty.Title);
                    // Run git commands
                    _vcsTool.Commit(_fileDetector.ResolvedCsProjFile, _vcsParser.Commit(theOutput, _fileParser, args.CommitMessage));
                    _vcsTool.Tag(_vcsParser.Tag(theOutput, _fileParser, args.VersionControlTag));
                }
            }

            if (args.OutputFormat == OutputFormat.Json)
            {
                WriteJsonToStdout(theOutput);
            }
            else if (args.OutputFormat == OutputFormat.Bare)
            {
                Console.WriteLine(bumpedSemVer.ToSemVerVersionString(_fileParser));
            }
            else
            {
                Console.WriteLine($"Bumped {_fileDetector.ResolvedCsProjFile} to version {bumpedSemVer.ToSemVerVersionString(_fileParser)}");
            }

            return theOutput;
        }

        public void DumpVersion(VersionCliArgs args)
        {
            var csProjXml = _fileDetector.FindAndLoadCsProj(args.CsProjFilePath);
            _fileParser.Load(csProjXml, ProjectFileProperty.Version, ProjectFileProperty.PackageVersion);

            switch (args.OutputFormat)
            {
                case OutputFormat.Json:
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
                    break;
                case OutputFormat.Bare:
                    Console.WriteLine(_fileParser.PackageVersion);
                    break;
                case OutputFormat.Text:
                default:
                    Console.WriteLine("Project version is: {0}\t{1}", Environment.NewLine, _fileParser.PackageVersion);
                    break;
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