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

        /// <summary>
        /// Bump the currently parsed version information with the specified <paramref name="bump"/>
        /// </summary>
        /// <param name="bump">The bump to apply to the version</param>
        /// <param name="specificVersionToApply">The specific version to apply if bump is Specific</param>
        /// <param name="buildMeta"></param>
        public void Bump(VersionBump bump, string specificVersionToApply = "", string buildMeta = "")
        {
            BuildMeta = buildMeta;
            switch (bump)
            {
                case VersionBump.Major:
                {
                    if (!IsPreRelease)
                    {
                        Major += 1;
                        Minor = 0;
                        Patch = 0;
                    }
                    else
                    {
                        PreRelease = string.Empty;
                        BuildMeta = string.Empty;
                    }

                    break;
                }
                case VersionBump.PreMajor:
                {
                    Major += 1;
                    Minor = 0;
                    Patch = 0;
                    PreRelease = "0";
                    break;
                }
                case VersionBump.Minor:
                {
                    if (!IsPreRelease)
                    {
                        Minor += 1;
                        Patch = 0;
                    }
                    else
                    {
                        PreRelease = string.Empty;
                        BuildMeta = string.Empty;
                    }

                    break;
                }
                case VersionBump.PreMinor:
                {
                    Minor += 1;
                    Patch = 0;
                    PreRelease = "0";
                    break;
                }
                case VersionBump.Patch:
                {
                    if (!IsPreRelease)
                    {
                        Patch += 1;
                    }
                    else
                    {
                        PreRelease = string.Empty;
                        BuildMeta = string.Empty;
                    }

                    break;
                }
                case VersionBump.PrePatch:
                {
                    Patch += 1;
                    PreRelease = "0";
                    break;
                }
                case VersionBump.PreRelease:
                {
                    if (!IsPreRelease)
                    {
                        throw new InvalidOperationException("Cannot Prerelease bump when not already a prerelease. Please use prepatch, preminor or prepatch to prepare");
                    }

                    if (!int.TryParse(PreRelease, out var preReleaseNumber))
                    {
                        throw new ArgumentException("Pre-release part is not numeric, cannot apply automatic prerelease roll");
                    }

                    preReleaseNumber += 1;
                    PreRelease = (preReleaseNumber).ToString();
                    break;
                }
                case VersionBump.Specific:
                {
                    if (string.IsNullOrEmpty(specificVersionToApply))
                    {
                        throw new ArgumentException($"When bump is specific, specificVersionToApply must be provided");
                    }

                    var specific = FromString(specificVersionToApply);
                    Major = specific.Major;
                    Minor = specific.Minor;
                    Patch = specific.Patch;
                    PreRelease = specific.PreRelease;
                    BuildMeta = specific.BuildMeta;
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException($"VersionBump : {bump} not supported");
                }
            }
            
        }

        public bool IsPreRelease => !string.IsNullOrWhiteSpace(PreRelease);

        /// <summary>
        /// Serialize the parsed version information into a SemVer version string including pre and build meta
        /// </summary>
        /// <returns></returns>
        public string ToSemVerVersionString()
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

            return sb.ToString();
        }

        /// <summary>
        /// Returns a simple version string including only major.minor.patch
        /// </summary>
        /// <returns></returns>
        public string ToSimpleVersionString()
        {
            return $"{Major}.{Minor}.{Patch}";
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
                BuildMeta = matches.Groups[5].Value,
            };
        }

        /// <summary>
        /// The parsed major version
        /// </summary>
        /// <returns></returns>
        public int Major { get; private set; }

        /// <summary>
        /// The parsed minor version
        /// </summary>
        /// <returns></returns>
        public int Minor { get; private set; }

        /// <summary>
        /// The parsed patch version
        /// </summary>
        /// <returns></returns>
        public int Patch { get; private set; }

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