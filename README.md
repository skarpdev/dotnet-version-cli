[![Build status](https://ci.appveyor.com/api/projects/status/r50rbldhoil6pqk6/branch/master?svg=true)](https://ci.appveyor.com/project/nover/dotnet-version-cli/branch/master)
[![nuget version][nuget-image]][nuget-url]
[![Sonar Quality][sonarqualitylogo]][sonarqubelink]
[![Code coverage][sonarcoveragelogo]][sonarqubelink]
[![Sonar vulnerabilities][sonarvulnerabilitieslogo]][sonarqubelink]
[![Sonar bugs][sonarbugslogo]][sonarqubelink]
[![Sonar code smells][sonarcodesmellslogo]][sonarqubelink]

# dotnet-version-cli

This repository contains the source code for an [npm/yarn version][1] inspired dotnet global tool for dotnet core 2.1 or newer with full [SemVer 2.0][semver2] compatibility!

This used to be a dotnet csproj installable `cli tool` - if you are not ready for the move to dotnet 2.1 global tools, please take a look at the last [0.7.0 release that supports csproj installation](https://github.com/skarpdev/dotnet-version-cli/blob/v0.7.0/README.md).

Once installed it provides a `dotnet version` command which allows you to easily bump `patch`, `minor` and `major` versions on your project. You can also release and manage pre-release
vesions of your packages by using the `prepatch`, `preminor` and `premajor` commands. Once in pre-release mode you can use the `prerelease` option to update the pre-release number.

Alternatively it allows you to call it with the specific version it should set in the target `csproj`.

We do not aim to be 100% feature compatible with `npm version` but provide the bare minimum for working with version numbers on your libraries and applications.

Effectively this means that issuing a `patch` command will

- bump the patch version of your project with 1 - so 1.0.0 becomes 1.0.1
- Create a commit with the message `v1.0.1`
- Create a tag with the name `v1.0.1`

Similarly for `minor` and `major`, but changing different parts of the version number.

When working with pre-releases using the `prepatch`, `preminor` and `premajor` options additional build meta can be passed using the `--build-meta` switch and the default `next` prefix can be changed using `--prefix`.

To control the output format the `--output-format` switch can be used - currently supported values are `json` and `text`. **Please beware** that output is only reformatted for success-cases, so if something is wrong you will get a non 0 exit code and text output!
Changing output format works for both "version bumping" and the "show version" operations of the cli.

The commit and tag can be disabled via the `--skip-vcs` option.

A completely dry run where nothing will be changed but the new version number is output can be enabled with the `--dry-run` switch. Performing a dry run also implies `skip vcs`.

If the current directory does not contain the `csproj` file to work on the `-f|--project-file` switch can be provided.

## Installing the cli tool

To install the tool simply issue

```bash
dotnet tool install -g dotnet-version-cli
```

Now it should be available as

```bash
dotnet version
```

It can also be executed directly as `dotnet-version` - both should produce output similar to

```text
$ dotnet version
dotnet-version-cli
Project version is:
        1.3.0
```

Using json output will produce

```bash
$ dotnet version --output-format=json
{"product":{"name":"dotnet-version-cli","version":"0.7.0.0"},"currentVersion":"1.3.0","projectFile":"C:\\your\\stuff\\project.csproj"}
```

The `product` bit is information about the cli tool itself.

## Standard workflow

You have just merged a PR with a bugfix onto master and you are ready to release a new version of your library / application. The workflow is then

```bash
$ git pull
$ dotnet version -f ./src/my.csproj patch
$ git push && git push --tags
```

## Pre-release workflow

As mentioned in the introduction the version tool allows working with pre-releases. 
Let's assume you have a library in version `1.2.4` and have made merges to master. You are not sure these changes work in the wild and therefore you require a 
pre-release. In the simpelest form you can

```bash
$ dotnet version preminor
``` 

To get a preminor out. This new version tag would become `1.2.5-next.0`.
If additional changes are merged you can roll over the pre-release version number by
```bash
$ dotnet version prerelease
```
To make the release `1.2.5-next.1`.
When ready you can snap out of pre-release mode and deploy the final minor version
```bash
$ dotnet version minor
```
Resulting in the version `1.2.5`.

All other command line flags like `-f` apply, and you can also include `build meta` as per SemVer 2.0 spec, like so:
```bash
dotnet version --build-meta `git rev-parse --short HEAD` preminor # or prerelease etc.
```
To have a resulting version string like `1.2.5-next.1+abcedf`

If the default `next` prefix is not desired it can easily be changed using the `--prefix` switch like so:
```bash
dotnet version --prefix beta preminor # or prerelease etc.
```

Resulting in `1.2.4-beta.0`.

## Possible CI workflow

If you do not care that commits and tags are made with the current version of your library, but simply wish to bump the version of your software when building on master, the tool can be used as (powershell example):

```powershell
dotnet version "1.0.$env:BUILD_ID"
```

replacing `BUILD_ID` with whatever variable your build environment injects.
The total count of commits in your git repo can also be used as a build number:

```powershell
$revCount = & git rev-list HEAD --count | Out-String
dotnet version "1.0.$revCount"
```

## Change commit message

If you want to change defaults commit message, you can use the flag `-m` or `--message`.
```bash
$ dotnet version minor -m "Commit message"
```

There are variables availables to be set in the message

`$projName` will be replaced for package title (or package id if its not defined)

`$oldVer` will be replaced for old version of the package

`$newVer` will be replaced for new version of the package
```bash
$ dotnet version minor -m "$projName bumped from v$oldVer to v$newVer"
# This will be replaced as
# ProjectName bumped from v1.0.0 to v2.0.0
```


## Change tag message

If you want to change defaults tag message, you can use the flag `-t` or `--tag`.
```bash
$ dotnet version minor -t "Tag"
```

There are variables availables to be set in the tag

`$projName` will be replaced for package title (or package id if its not defined)

`$oldVer` will be replaced for old version of the package

`$newVer` will be replaced for new version of the package
```bash
$ dotnet version minor -t "$projName bumped from v$oldVer to v$newVer"
# This will be replaced as
# ProjectName bumped from v1.0.0 to v2.0.0
```


[1]: https://docs.npmjs.com/cli/version
[nuget-image]: https://img.shields.io/nuget/v/dotnet-version-cli.svg
[nuget-url]: https://www.nuget.org/packages/dotnet-version-cli
[semver2]: https://semver.org/spec/v2.0.0.html
[sonarqubelink]: https://sonarcloud.io/dashboard?id=skarpdev_dotnet-version-cli
[sonarqualitylogo]: https://sonarcloud.io/api/project_badges/measure?project=skarpdev_dotnet-version-cli&metric=alert_status
[sonarcoveragelogo]: https://sonarcloud.io/api/project_badges/measure?project=skarpdev_dotnet-version-cli&metric=coverage
[sonarvulnerabilitieslogo]: https://sonarcloud.io/api/project_badges/measure?project=skarpdev_dotnet-version-cli&metric=vulnerabilities
[sonarbugslogo]: https://sonarcloud.io/api/project_badges/measure?project=skarpdev_dotnet-version-cli&metric=bugs
[sonarcodesmellslogo]: https://sonarcloud.io/api/project_badges/measure?project=skarpdev_dotnet-version-cli&metric=code_smells