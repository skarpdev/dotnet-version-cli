using System;
using Skarp.Version.Cli.Versioning;
using Xunit;

namespace Skarp.Version.Cli.Test.Versioning
{
    public class SemVerTest
    {
        [Theory]
        // valid cases - Semver 2.0 is crazy!
        [InlineData("1.0.0", 1, 0, 0, "", "", true)]
        [InlineData("0.10.0", 0, 10, 0, "", "", true)]
        [InlineData("1.12.99", 1, 12, 99, "", "", true)]
        [InlineData("4.99.43245", 4, 99, 43245, "", "", true)]
        [InlineData("4.1.3", 4, 1, 3, "", "", true)]
        [InlineData("42.4.554", 42, 4, 554, "", "", true)]
        [InlineData("3.1.554-alpha.33", 3, 1, 554, "alpha.33", "", true)]
        [InlineData("3.1.554-alpha.33+master", 3, 1, 554, "alpha.33", "master", true)]
        public void CanParseValidSemVers(
            string version,
            int expectedMajor,
            int expectedMinor,
            int expectedPatch,
            string expectedPreReleaseBuildInfo,
            string expectedBuildMeta,
            bool isValid
        )
        {
            if (!isValid)
            {
                var ex = Record.Exception(() => SemVer.FromString(version));
                Assert.IsAssignableFrom<ArgumentException>(ex);

                return;
            }

            var semver = SemVer.FromString(version);

            Assert.Equal(expectedMajor, semver.Major);
            Assert.Equal(expectedMinor, semver.Minor);
            Assert.Equal(expectedPatch, semver.Patch);
            Assert.Equal(expectedPreReleaseBuildInfo, semver.PreRelease);
            Assert.Equal(expectedBuildMeta, semver.BuildMeta);

            if (!string.IsNullOrWhiteSpace(semver.PreRelease))
            {
                Assert.True(semver.IsPreRelease);
            }
        }

        [Theory]
        [InlineData("is-this-a-version")]
        [InlineData("1,0")]
        [InlineData("2")]
        [InlineData("2.0")]
        [InlineData("2.0.1.2.3.4")]
        public void BailsOnInvalidSemVers(string version)
        {
            var ex = Assert.Throws<ArgumentException>(() => SemVer.FromString(version));
            Assert.Equal("versionString", ex.ParamName);
            Assert.Contains($"Invalid SemVer version string: {version}", ex.Message);
        }
    }
}