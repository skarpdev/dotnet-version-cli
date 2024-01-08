using Skarp.Version.Cli.CsProj;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Skarp.Version.Cli.Versioning
{
    public class SemVer
    {
        // this lovely little regex comes from the SemVer spec:
        // https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
        private static readonly Regex VersionPartRegex = new Regex(
            @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(2)
        );

        public bool IsPreRelease => !string.IsNullOrWhiteSpace(PreRelease);

        /// <summary>
        /// Serialize the parsed version information into a SemVer version string including pre and build meta
        /// </summary>
        /// <returns></returns>
        public string ToSemVerVersionString(ProjectFileParser projectFileParser)
        {
            var sb = new StringBuilder();
            sb.Append($"{Major}.{Minor}.{Patch}");

            if (!string.IsNullOrWhiteSpace(PreRelease))
            {
                sb.AppendFormat("-{0}", PreRelease);
                if (!string.IsNullOrWhiteSpace(BuildMeta))
                {
                    sb.AppendFormat("+{0}", BuildMeta);
                }
            }

            if (projectFileParser != null 
                && projectFileParser.VersionSource == ProjectFileProperty.VersionPrefix
                && !string.IsNullOrWhiteSpace(projectFileParser.VersionSuffix))
            {
                sb.AppendFormat("-{0}", projectFileParser.VersionSuffix);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create a new instance of a SemVer based off the version string
        /// </summary>
        /// <param name="versionString">The version string to parse into a SemVer instance</param>
        /// <returns></returns>
        public static SemVer FromString(string versionString)
        {
            var matches = VersionPartRegex.Match(versionString);
            if (!matches.Success)
            {
                throw new ArgumentException($"Invalid SemVer version string: {versionString}", nameof(versionString));
            }

            // Groups [0] is the full string , then we have the version parts after that
            return new SemVer
            {
                Major = Convert.ToInt32(matches.Groups[1].Value),
                Minor = Convert.ToInt32(matches.Groups[2].Value),
                Patch = Convert.ToInt32(matches.Groups[3].Value),
                PreRelease = matches.Groups[4].Value,
                BuildMeta = matches.Groups[5].Value
            };
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// The parsed major version
        /// </summary>
        /// <returns></returns>
        public int Major { get; set; }

        /// <summary>
        /// The parsed minor version
        /// </summary>
        /// <returns></returns>
        public int Minor { get; set; }

        /// <summary>
        /// The parsed patch version
        /// </summary>
        /// <returns></returns>
        public int Patch { get; set; }

        /// <summary>
        /// Pre-release semver 2 information (the stuff added with a dash after version)
        /// </summary>
        public string PreRelease { get; set; }

        /// <summary>
        /// Build mtadata semver 2 information (the stuff added with a + sign after PreRelease info)
        /// </summary>
        public string BuildMeta { get; set; }

    }
}