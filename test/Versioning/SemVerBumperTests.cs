using Skarp.Version.Cli.Versioning;
using Xunit;

namespace Skarp.Version.Cli.Test.Versioning
{
    public class SemVerBumperTests
    {
        private readonly SemVerBumper _bumper;

        public SemVerBumperTests()
        {
            _bumper = new SemVerBumper();
        }
        [Theory]
        [InlineData("1.1.0", VersionBump.Major, 2, 0, 0, "", "")]
        [InlineData("1.1.0", VersionBump.PreMajor, 2, 0, 0, "next.0", "")]
        [InlineData("4.1.3", VersionBump.Minor, 4, 2, 0, "", "")]
        [InlineData("4.1.3", VersionBump.PreMinor, 4, 2, 0, "next.0", "")]
        [InlineData("2.1.0", VersionBump.Patch, 2, 1, 1, "", "")]
        [InlineData("2.1.0", VersionBump.PrePatch, 2, 1, 1, "next.0", "")]
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
            var semver = _bumper.Bump(
                SemVer.FromString(version),
                bump,
                version
            );

            Assert.Equal(expectedMajor, semver.Major);
            Assert.Equal(expectedMinor, semver.Minor);
            Assert.Equal(expectedPatch, semver.Patch);
            Assert.Equal(expectedPreRelease, semver.PreRelease);
            Assert.Equal(expectedBuildMeta, semver.BuildMeta);
        }

        [Theory]
        [InlineData("1.0.0", VersionBump.Major, "2.0.0")]
        [InlineData("1.0.0", VersionBump.PreMajor, "2.0.0-next.0")]
        [InlineData("4.1.3", VersionBump.Minor, "4.2.0")]
        [InlineData("4.1.3", VersionBump.PreMinor, "4.2.0-next.0")]
        [InlineData("2.1.0", VersionBump.Patch, "2.1.1")]
        [InlineData("2.1.0", VersionBump.PrePatch, "2.1.1-next.0")]
        [InlineData("1.1.1-42", VersionBump.Patch, "1.1.1")] // snap out of pre-release mode
        [InlineData("1.1.1-42+master", VersionBump.Patch, "1.1.1")] // snap out of pre-release mode
        [InlineData("1.1.1-42", VersionBump.PreRelease, "1.1.1-next.43")] // increment prerelease number
        [InlineData("1.1.1-next.42", VersionBump.PreRelease, "1.1.1-next.43")] // increment prerelease number
        public void CanBumpAndSerializeStringVersion(string version, VersionBump bump, string expectedVersion)
        {
            var semver = _bumper.Bump(SemVer.FromString(version), bump);
            Assert.Equal(expectedVersion, semver.ToSemVerVersionString());
        }
    }
}