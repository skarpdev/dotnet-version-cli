using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace Skarp.Version.Cli.Test
{
    public class ProgramTest
    {

        [Theory]
        [InlineData("major", VersionBump.Major)]
        [InlineData("minor", VersionBump.Minor)]
        [InlineData("patch", VersionBump.Patch)]
        public void GetVersionBumpFromRemaingArgsWork(string strVersionBump, VersionBump expectedBump)
        {
            var theBump = Program.GetVersionBumpFromRemainingArgs(new List<string>() {strVersionBump});
            Assert.Equal(expectedBump, theBump);
        }

        [Fact]
        public void Get_version_bump_throws_on_missing_value()
        {
            var ex = Assert.Throws<ArgumentException>(() => Program.GetVersionBumpFromRemainingArgs(new List<string>()));
            Assert.Equal($"No version bump specified, please specify one of:\n\tmajor | minor | patch{Environment.NewLine}Parameter name: versionBump", ex.Message);
            Assert.Equal("versionBump", ex.ParamName);
        }

        [Fact]
        public void Get_version_bump_throws_on_invalid_value()
        {
            const string invalidVersion = "invalid-version";

            var ex = Assert.Throws<ArgumentException>(() => Program.GetVersionBumpFromRemainingArgs(new List<string>{invalidVersion}));
            Assert.Equal($"Invalid version bump specified: {invalidVersion}{Environment.NewLine}Parameter name: versionBump", ex.Message);
            Assert.Equal("versionBump", ex.ParamName);
        }
    }
}
