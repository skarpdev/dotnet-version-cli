namespace Skarp.Version.Cli
{
    /// <summary>
    /// Enumerates the possible version bumps
    /// </summary>
    public enum VersionBump
    {
        Major,

        Minor,

        Patch,

        /// <summary>
        /// Apply a specific, given, version to the project file
        /// </summary>
        Specific
    }
}