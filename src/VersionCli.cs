using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Skarp.Version.Cli.CsProj;
using Skarp.Version.Cli.Vcs;

namespace Skarp.Version.Cli
{
    public class VersionCli
    {
        private readonly IVcs _vcsTool;
        private readonly ProjectFileDetector _fileDetector;
        private readonly ProjectFileParser _fileParser;
        private readonly ProjectFileVersionPatcher _fileVersionPatcher;

        public VersionCli(
            IVcs vcsClient,
            ProjectFileDetector fileDetector,
            ProjectFileParser fileParser,
            ProjectFileVersionPatcher fileVersionPatcher
        )
        {
            _vcsTool = vcsClient;
            _fileDetector = fileDetector;
            _fileParser = fileParser;
            _fileVersionPatcher = fileVersionPatcher;
        }

        public void Execute(VersionCliArgs args)
        {
            if (!_vcsTool.IsVcsToolPresent())
            {
                throw new OperationCanceledException(
                    $"Unable to find the vcs tool {_vcsTool.ToolName()} in your path");
            }

            if (!_vcsTool.IsRepositoryClean())
            {
                throw new OperationCanceledException(
                    "You currently have uncomitted changes in your repository, please commit these and try again");
            }

            var csProjXml = _fileDetector.FindAndLoadCsProj(args.CsProjFilePath);
            _fileParser.Load(csProjXml);

            var semVer = SemVer.FromString(_fileParser.Version);
            semVer.Bump(args.VersionBump, args.SpecificVersionToApply);
            var newVersion = semVer.ToVersionString();
            var patchedCsProjXml = _fileVersionPatcher.Patch(
                csProjXml,
                _fileParser.Version,
                newVersion
            );
            _fileVersionPatcher.Flush(
                patchedCsProjXml,
                _fileDetector.ResolvedCsProjFile
            );

            // Run git commands
            _vcsTool.Commit(_fileDetector.ResolvedCsProjFile, $"v{newVersion}");
            _vcsTool.Tag($"v{newVersion}");

            if (args.OutputFormat == OutputFormat.Json)
            {
                var theOutput = new
                {
                    Product = new
                    {
                        Name = ProductInfo.Name,
                        Version = ProductInfo.Version
                    },
                    OldVersion = _fileParser.Version,
                    NewVersion = newVersion,
                    ProjectFile = _fileDetector.ResolvedCsProjFile,
                    VersionStrategy = args.VersionBump.ToString().ToLowerInvariant()
                };

                Console.WriteLine(
                    JsonConvert.SerializeObject(
                        theOutput, new JsonSerializerSettings
                        {
                            ContractResolver = new CamelCasePropertyNamesContractResolver()
                        }));
            }
            else
            {
                Console.WriteLine($"Bumped {_fileDetector.ResolvedCsProjFile} to version {newVersion}");
            }
        }

        public void DumpVersion(VersionCliArgs args)
        {
            var csProjXml = _fileDetector.FindAndLoadCsProj(args.CsProjFilePath);
            _fileParser.Load(csProjXml);

            Console.WriteLine("Project version is: {0}\t{1}", Environment.NewLine, _fileParser.Version);
        }
    }
}