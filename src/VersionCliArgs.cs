namespace Skarp.Version.Cli
{
    public class VersionCliArgs
    {
        public VersionBump VersionBump { get; set; }

        public string SpecificVersionToApply { get; set; }

        public string CsProjFilePath { get; set; }

        public OutputFormat OutputFormat { get; set; }
        
        /// <summary>
        /// Whether or not to do version control changes like
        /// commit and tag.
        /// </summary>
        public bool DoVcs { get; set; }
    }
}