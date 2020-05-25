namespace Skarp.Version.Cli
{
    /// <summary>
    /// Enumerates the possible version bumps
    /// </summary>
    public enum VersionBump
    {
        // Not supplied or parsing error or something - we can't bump `unknown`
        Unknown, 
        
        Major,

        Minor,

        Patch,

        PreMajor,
        
        PreMinor,
        
        PrePatch,
     
        /// <summary>
        /// Increment the PreRelease indetifier (if it is numeric and rolled by this tool)
        /// </summary>
        PreRelease,
        
        /// <summary>
        /// Apply a specific, given, version to the project file
        /// </summary>
        Specific
    }
}