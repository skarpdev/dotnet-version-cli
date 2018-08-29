using System;
using System.Collections.Generic;
using System.Text;
using FakeItEasy;
using Microsoft.Extensions.CommandLineUtils;
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
        [InlineData("1.0.1", VersionBump.Specific)]
        public void GetVersionBumpFromRemaingArgsWork(string strVersionBump, VersionBump expectedBump)
        {
            var args = Program.GetVersionBumpFromRemainingArgs(new List<string>() {strVersionBump}, OutputFormat.Text,
                true, true, string.Empty);
            Assert.Equal(expectedBump, args.VersionBump);
            if (expectedBump == VersionBump.Specific)
            {
                Assert.Equal(strVersionBump, args.SpecificVersionToApply);
            }
        }

        [Fact]
        public void Get_version_bump_throws_on_missing_value()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                Program.GetVersionBumpFromRemainingArgs(new List<string>(), OutputFormat.Text, true, true,
                    string.Empty));
            Assert.Equal(
                $"No version bump specified, please specify one of:\n\tmajor | minor | patch | <specific version>{Environment.NewLine}Parameter name: versionBump",
                ex.Message);
            Assert.Equal("versionBump", ex.ParamName);
        }

        [Fact]
        public void Get_version_bump_throws_on_invalid_value()
        {
            const string invalidVersion = "invalid-version";

            var ex = Assert.Throws<ArgumentException>(() =>
                Program.GetVersionBumpFromRemainingArgs(new List<string> {invalidVersion}, OutputFormat.Text, true,
                    true, string.Empty));
            Assert.Equal($"Malformed version part: {invalidVersion}{Environment.NewLine}Parameter name: versionString",
                ex.Message);
            Assert.Equal("versionString", ex.ParamName);
        }
    }
}