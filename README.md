[![Build status](https://ci.appveyor.com/api/projects/status/r50rbldhoil6pqk6/branch/master?svg=true)](https://ci.appveyor.com/project/nover/dotnet-version-cli/branch/master)
[![nuget version][nuget-image]][nuget-url]

# dotnet-version-cli

This repository contains the source code for an [npm version][1] inspired dotnet global tool for dotnet core 2.1 or newer.

This used to be a dotnet csproj installable `cli tool` - if you are not ready for the move to dotnet 2.1 global tools, please take a look at the last [0.7.0 release that supports csproj installation](https://github.com/skarpdev/dotnet-version-cli/blob/v0.7.0/README.md).

Once installed it provides a `dotnet version` command which allows you to easily bump `patch`, `minor` and `major` versions on your project.
Alternatively it allows you to call it with the specific version it should set in the target `csproj`.

We do not aim to be 100% feature compatible with `npm version` but provide the bare minimum for working with version numbers on your libraries and applications.

Effectively this means that issuing a `patch` command will

- bump the patch version of your project with 1 - so 1.0.0 becomes 1.0.1
- Create a commit with the message `v1.0.1`
- Create a tag with the name `v1.0.1`

Similarly for `minor` and `major`, but changing different parts of the version number.

To control the output format the `--output-format` switch can be used - currently supported values are `json` and `text`. **Please beware** that output is only reformatted for success-cases, so if something is wrong you will get a non 0 exit code and text output!
Changing output format works for both "version bumping" and the "show version" operations of the cli.

The commit and tag can be disabled via the `--skip-vcs` option.

A completely dry run where nothing will be changed but the new version number is output can be enabled with the `--dry-run` switch. Performing a dry run also implies `skip vcs`.

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

[1]: https://docs.npmjs.com/cli/version
[nuget-image]: https://img.shields.io/nuget/v/dotnet-version-cli.svg
[nuget-url]: https://www.nuget.org/packages/dotnet-version-cli
