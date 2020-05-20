using System;
using Xunit;

namespace Skarp.Version.Cli.Test
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

        [Theory]
        [InlineData("1.1.0", VersionBump.Major, 2, 0, 0, "", "")]
        [InlineData("1.1.0", VersionBump.PreMajor, 2, 0, 0, "0", "")]
        [InlineData("4.1.3", VersionBump.Minor, 4, 2, 0, "", "")]
        [InlineData("4.1.3", VersionBump.PreMinor, 4, 2, 0, "0", "")]
        [InlineData("2.1.0", VersionBump.Patch, 2, 1, 1, "", "")]
        [InlineData("2.1.0", VersionBump.PrePatch, 2, 1, 1, "0", "")]
        [InlineData("3.2.1", VersionBump.Specific, 3, 2, 1, "", "")]
        [InlineData("3.2.1-0+master", VersionBump.Specific, 3, 2, 1, "0", "master")]
        public void CanBumpVersions(
            string version,
            VersionBump bump,
            int expectedMajor,
            int expectedMinor,
            int expectedPatch,
            string expectedPreRelease,
            string expectedBuildMeta
        )
        {
            var semver = SemVer.FromString(version);
            semver.Bump(bump, version);

            Assert.Equal(expectedMajor, semver.Major);
            Assert.Equal(expectedMinor, semver.Minor);
            Assert.Equal(expectedPatch, semver.Patch);
            Assert.Equal(expectedPreRelease, semver.PreRelease);
            Assert.Equal(expectedBuildMeta, semver.BuildMeta);
        }

        [Theory]
        [InlineData("1.0.0", VersionBump.Major, "2.0.0")]
        [InlineData("1.0.0", VersionBump.PreMajor, "2.0.0-0")]
        [InlineData("4.1.3", VersionBump.Minor, "4.2.0")]
        [InlineData("4.1.3", VersionBump.PreMinor, "4.2.0-0")]
        [InlineData("2.1.0", VersionBump.Patch, "2.1.1")]
        [InlineData("2.1.0", VersionBump.PrePatch, "2.1.1-0")]
        [InlineData("1.1.1-42", VersionBump.Patch, "1.1.1")] // snap out of pre-release mode
        [InlineData("1.1.1-42+master", VersionBump.Patch, "1.1.1")] // snap out of pre-release mode
        [InlineData("1.1.1-42", VersionBump.PreRelease, "1.1.1-43")] // increment prerelease number
        
        public void CanBumpAndSerializeStringVersion(string version, VersionBump bump, string expectedVersion)
        {
            var semver = SemVer.FromString(version);
            semver.Bump(bump);

            Assert.Equal(expectedVersion, semver.ToSemVerVersionString());
        }
    }
}