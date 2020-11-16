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

        /// <summary>
        /// Whether dry run is enabled and thus all mutations should be disabled
        /// </summary>
        public bool DryRun { get; set; }

        /// <summary>
        /// Build meta for a pre-release tag passed via CLI arguments
        /// </summary>
        public string BuildMeta { get; set; }

        /// <summary>
        /// Override for the default `next` pre-release prefix/label
        /// </summary>
        public string PreReleasePrefix  { get; set; }

        /// <summary>
        /// Set commit's message
        /// </summary>
        public string CommitMessage { get; set; }

        /// <summary>
        /// Override for the default `v<version>` vcs tag
        /// </summary>
        public string VersionControlTag { get; set; }
    }
}