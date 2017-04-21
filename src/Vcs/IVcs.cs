namespace Skarpdev.DotnetVersion.Vcs
{
    /// <summary>
    /// Version Control System abstraction interface
    /// </summary>
    public interface IVcs
    {
        /// <summary>
        /// When implemented by a concrete class it returns the name of the VCS tool
        /// </summary>
        /// <returns></returns>
        string ToolName();

        /// <summary>
        /// When implemented by a concrete class it determines whether the necessary tools
        /// are available in the current CLI contenxt - i.e check that `git` command can be found
        /// and executed
        /// </summary>
        /// <returns><c>true</c> if the tool exists, <c>false</c> otherwise</returns>
        bool IsVcsToolPresent();
        
        /// <summary>
        /// When implemented by a concrete class it returns <c>true</c> if the 
        /// current HEAD of the local repository is clean - i.e no pending changes
        /// </summary>
        /// <returns></returns>
        bool IsRepositoryClean();

        /// <summary>
        /// When implemented by a concrete class it allows to create a commit with the 
        /// changed version in the project file
        /// </summary>
        /// <param name="csProjFilePath">Path to the cs project file</param>
        /// <param name="message">The message to create the commit message with</param>
        void Commit(string csProjFilePath, string message);

        /// <summary>
        /// When implemented by a concrete class it will tag the latest commit with the
        /// given tag name
        /// </summary>
        /// <param name="tagName">The name of the tag to create - i.e v1.0.2 </param>
        void Tag(string tagName);
    }
}