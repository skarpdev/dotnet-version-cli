namespace Skarp.Version.Cli
{
    public class VersionCliArgs
    {
        public VersionBump VersionBump { get; set; }

        public string SpecificVersionToApply { get; set; }
        public string CsProjFilePath { get; set; }
    }
}