using System;
using System.Text.RegularExpressions;

namespace Skarpdev.DotnetVersion
{
    public class SemVer
    {
        private static Regex _versionPartRegex = new Regex(@"^\d$", RegexOptions.Compiled);

        public SemVer()
        {
        }

        public string Bump(VersionBump bump)
        {
            throw new NotImplementedException("Bumping is not yet implemented");
        }

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

            string value = array[index];
            if(!_versionPartRegex.IsMatch(value))
            {
                throw new ArgumentException($"Malformed version part: {value}", "versionString");
            }
            return Convert.ToInt32(value);
        }

        public int Major { get; set; }

        public int Minor { get; set; }

        public int Patch { get; set; }

        public string Additional {get; set;}

    }
}