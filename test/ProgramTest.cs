﻿using System;
using System.Collections.Generic;
using Skarp.Version.Cli.CsProj;
using Xunit;

namespace Skarp.Version.Cli.Test
{
    public class ProgramTest
    {
        [Theory]
        [InlineData("major", VersionBump.Major)]
        [InlineData("premajor", VersionBump.PreMajor)]
        [InlineData("minor", VersionBump.Minor)]
        [InlineData("preminor", VersionBump.PreMinor)]
        [InlineData("patch", VersionBump.Patch)]
        [InlineData("prepatch", VersionBump.PrePatch)]
        [InlineData("1.0.1", VersionBump.Specific)]
        [InlineData("1.0.1-0", VersionBump.Specific)]
        [InlineData("1.0.1-0+master", VersionBump.Specific)]
        [InlineData("1.0.1-alpha.43+4432fsd", VersionBump.Specific)]
        public void GetVersionBumpFromRemainingArgsWork(string strVersionBump, VersionBump expectedBump)
        {
            var args = Program.GetVersionBumpFromRemainingArgs(
                new List<string>() {strVersionBump},
                OutputFormat.Text,
                true,
                true,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                true
            );
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
                Program.GetVersionBumpFromRemainingArgs(
                    new List<string>(),
                    OutputFormat.Text,
                    true,
                    true,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    true
                )
            );
            Assert.Contains(
                $"No version bump specified, please specify one of:\n\tmajor | minor | patch | premajor | preminor | prepatch | prerelease | <specific version>",
                ex.Message);
        }

        [Fact]
        public void Get_version_bump_throws_on_invalid_value()
        {
            const string invalidVersion = "invalid-version";

            var ex = Assert.Throws<ArgumentException>(() =>
                Program.GetVersionBumpFromRemainingArgs(
                    new List<string> {invalidVersion},
                    OutputFormat.Text,
                    true,
                    true,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    true
                )
            );
            Assert.Contains($"Invalid SemVer version string: {invalidVersion}",
                ex.Message);
            Assert.Equal("versionString", ex.ParamName);
        }
        
        [Fact]
        public void DefaultsToReadingVersionStringFromVersionProperty()
        {
            var args = Program.GetVersionBumpFromRemainingArgs(
                new List<string>() {"patch"},
                OutputFormat.Text,
                true,
                true,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                true
            );
            
            Assert.Equal(ProjectFileProperty.Version, args.ProjectFilePropertyName);
        }
        
        [Theory]
        [InlineData(null, ProjectFileProperty.Version)]
        [InlineData("", ProjectFileProperty.Version)]
        [InlineData("verSION", ProjectFileProperty.Version)]
        [InlineData("packageversion", ProjectFileProperty.PackageVersion)]
        public void CanOverrideTheVersionPropertyName(string input, ProjectFileProperty expected)
        {
            var args = Program.GetVersionBumpFromRemainingArgs(
                new List<string>() {"patch"},
                OutputFormat.Text,
                true,
                true,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                input,
                true
            );
            
            Assert.Equal(expected, args.ProjectFilePropertyName);
        }
    }
}