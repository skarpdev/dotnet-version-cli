# dotnet-version-cli

This repository contains the source code for an [npm version][1] inspired cli tool for the dotnet core SDK 1.0.1 (csproj based).

Once installed it provides a `dotnet version` cli extension which allows you to easily bump `patch`, `minor` and `major` versions on your project.

We do not aim to be 100% feature compatible with `npm version` but provide the bare minimum for working with version numbers on your libraries and applications.

Effectively this means that issuing a `patch` command will

- bump the patch version of your project with 1 - so 1.0.0 becomes 1.0.1
- Create a commit with the message `v1.0.1`
- Create a tag with the name `v1.0.1`

Similarly for `minor` and `major`, but changing different parts of the version number.

## Installing the cli tool

To install the cli tool add it to the `csproj` file of your library / application:

```xml
<ItemGroup>
    <DotNetCliToolReference Include="dotnet-version-cli" Version="0.1.2" />
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

## Using the tool / workflow

You have just merged a PR with a bugfix onto master and you are ready to release a new version of your library / application. The workflow is then

```bash
$ git pull
$ dotnet version patch
$ git push --tags && git push
```

[1]: https://docs.npmjs.com/cli/version