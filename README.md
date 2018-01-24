[![Build status](https://ci.appveyor.com/api/projects/status/r50rbldhoil6pqk6/branch/master?svg=true)](https://ci.appveyor.com/project/nover/dotnet-version-cli/branch/master)
[![nuget version][nuget-image]][nuget-url]

# dotnet-version-cli

This repository contains the source code for an [npm version][1] inspired cli tool for the dotnet core SDK 1.0.1 or newer (csproj based).

Once installed it provides a `dotnet version` cli extension which allows you to easily bump `patch`, `minor` and `major` versions on your project. Alternatively it allows you to call it with the specific version it should set in the target `csproj`

We do not aim to be 100% feature compatible with `npm version` but provide the bare minimum for working with version numbers on your libraries and applications.

Effectively this means that issuing a `patch` command will

- bump the patch version of your project with 1 - so 1.0.0 becomes 1.0.1
- Create a commit with the message `v1.0.1`
- Create a tag with the name `v1.0.1`

Similarly for `minor` and `major`, but changing different parts of the version number.

To control the output format the `--output-format` switch can be used - currently supported values are `json` and `text`. **Please beware** that output is only reformatted for success-cases, so if something is wrong you will get a non 0 exit code and text output!
Changing output format works for both "version bumping" and the "show version" operations of the cli.

The commit and tag can be disabled via the `--skip-vcs` option.

## Installing the cli tool

To install the cli tool add it to the `csproj` file of your library / application:

```xml
<ItemGroup>
    <DotNetCliToolReference Include="dotnet-version-cli" Version="0.5.0" />
</ItemGroup>
```

And issue `dotnet restore`. To check if the tool works, run the command (in the same folder as your `csproj` file)

```bash
dotnet version
```

Which should produce output similar to

```text
dotnet-version-cli
Project version is:
        1.3.0
```

Using json output will produce

```bash
$ dotnet version --output-format=json
{"product":{"name":"dotnet-version-cli","version":"0.5.0.0"},"currentVersion":"1.3.0","projectFile":"C:\\your\\stuff\\project.csproj"}
```

The `product` bit is information about the cli tool itself.

## Standard workflow

You have just merged a PR with a bugfix onto master and you are ready to release a new version of your library / application. The workflow is then

```bash
$ git pull
$ dotnet version patch
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