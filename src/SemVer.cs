using System;
using System.Text.RegularExpressions;

namespace Skarp.Version.Cli
{
    public class SemVer
    {
        private static readonly Regex VersionPartRegex = new Regex(@"^\d$", RegexOptions.Compiled);

        /// <summary>
        /// Bump the currently parsed version information with the specified <paramref name="bump"/>
        /// </summary>
        /// <param name="bump">The bump to apply to the version</param>
        public void Bump(VersionBump bump)
        {
            switch(bump)
            {
                case VersionBump.Major:
                {
                    Major += 1;
                    Minor = 0;
                    Patch = 0;
                    break;
                }    
                case VersionBump.Minor:
                {
                    Minor += 1;
                    Patch = 0;
                    break;
                }
                case VersionBump.Patch:
                {
                    Patch += 1;
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException($"VersionBump : {bump} not supported");
                }
            }
        }

        /// <summary>
        /// Serialize the parsed version information into a version string
        /// </summary>
        /// <returns></returns>
        public string ToVersionString()
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
            var parts = versionString.Split('.');
            if(parts.Length == 0)
            {
                throw new ArgumentException($"Malformed versionString: {versionString}", nameof(versionString));
            }

            return new SemVer
            {
                Major = SafeArrayGet(parts, 0),
                Minor = SafeArrayGet(parts, 1),
                Patch = SafeArrayGet(parts, 2),
            };
        }

        private static int SafeArrayGet(string[] array, int index)
        {
            if(index > array.Length-1) return 0;

            var value = array[index];
            if(!VersionPartRegex.IsMatch(value))
            {
                throw new ArgumentException($"Malformed version part: {value}", "versionString");
            }
            return Convert.ToInt32(value);
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
    }
}