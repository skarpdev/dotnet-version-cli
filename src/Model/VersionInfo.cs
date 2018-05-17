namespace Skarp.Version.Cli.Model
{
    public class VersionInfo
    {
        public ProductOutputInfo Product { get; set; }
        public string OldVersion { get; set; }
        public string NewVersion { get; set; }
        public string ProjectFile { get; set; }
        public string VersionStrategy { get; set; }
    }
}