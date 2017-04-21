using System;
using Xunit;

namespace Skarpdev.DotnetVersion.Test
{
    public class SemVerTest
    {

        [Theory]
        [InlineData("1.0.0", 1, 0, 0)]
        [InlineData("4.1.3", 4, 1, 3)]
        [InlineData("2.1", 2, 1, 0)]
        [InlineData("2", 2, 0, 0)]
        [InlineData("4.1.3.6.7.8.9", 4, 1, 3)] // too many version numbers
        public void CanParseValidSemVers(string version, int expectedMajor, int expectedMinor, int expectedPatch)
        {
            var semver = SemVer.FromString(version);

            Assert.Equal(expectedMajor, semver.Major);
            Assert.Equal(expectedMinor, semver.Minor);
            Assert.Equal(expectedPatch, semver.Patch);
        }

        [Theory]
        [InlineData("is-this-a-version")]
        [InlineData("1,0")]
        public void BailsOnInvalidSemVers(string version)
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVer.FromString(version));
            Assert.Equal(ex.ParamName, "versionString");
            Assert.Equal(ex.Message, $"Malformed version part: {version}\nParameter name: versionString");
        }
    }
}