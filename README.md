<img src="https://raw.githubusercontent.com/NoiseStudio/branding/master/NoiseEngine/renders/NoiseEngine-FullLogoColor.png" alt="NoiseEngine logo" height="130">

![Tests](https://github.com/NoiseStudio/NoiseEngine/actions/workflows/tests.yml/badge.svg)
![Issues](https://img.shields.io/github/issues/NoiseStudio/NoiseEngine)
![License](https://img.shields.io/github/license/NoiseStudio/NoiseEngine)
![Milestone](https://img.shields.io/github/milestones/progress-percent/NoiseStudio/NoiseEngine/1)
![GitHub stars](https://img.shields.io/github/stars/NoiseStudio/NoiseEngine)
![GitHub forks](https://img.shields.io/github/forks/NoiseStudio/NoiseEngine)
[![Discord](https://img.shields.io/discord/1154793486164430939.svg?logo=discord)][discord]

[discord]: https://discord.gg/X3Wms5jd2x

Completely free, open-source, cross-platform, and blazingly fast asynchronous engine for creating modern games and apps.

https://noisestudio.net/

> **Warning**
NoiseEngine is still in **very early** development. Frequent changes, missing docs, and instabilities may occur.<br>
You should avoid using it for **any** game development before v0.1.0 is released. [Contributions](#how-can-i-contribute) are welcome!

## Why?
Popular game engines were created many years ago and are based on synchronous APIs which do not take full advantage of modern CPUs. While building the prototype of our own next generation procedural game in 2021 using an existing engine, we had difficulties with optimizing internal things in the engine which we used. We started looking for alternatives, but they all had the same problems, so we made the difficult decision to create our own engine - NoiseEngine.

Our golden goal is to create the most performant engine, one which enables game developers to create games which are hard to do with current generation engines. Also on AAA scale.

![Voxel terrain](/images/voxel-terrain.webp)
*Real-time voxel terrain demo, render distance 8192 meters/blocks - [1 Februrary 2023](https://github.com/NoiseStudio/NoiseEngine/releases/tag/v0.1.0-alpha1)*

## Overview
### Documentation
Currently (before v0.1.0) initial documentation is still being writen. It is not published on any website, but you can read it from [documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments). You can find usage examples in [tests](/docs/project-structure.md). There is currently no user manual.

### Project structure
We have a few repositories which contain parts of the entire project:
- [Current](https://github.com/NoiseStudio/NoiseEngine) - contains the core of the engine, all APIs which you will use when building your application.
- [NoiseEngine.Cli](https://github.com/NoiseStudio/NoiseEngine.Cli) - command line interface tools for building and shipping games and apps with NoiseEngine
- [NoiseEngine.Editor](https://github.com/NoiseStudio/NoiseEngine.Editor) - graphical user interface that simplifies creating games and apps

You can see more details about the structure of this repository [here](/docs/project-structure.md).

### Versioning
We use [sementic versioning](https://semver.org/). After the first version is released (v0.1.0), the plan is to often release patch updates. At the same time we are of the opinion that the `master` branch should always remain backwards compatible with as much code as possible.

### Technology stack
NoiseEngine uses .NET 7 and recent versions of Rust. For rendering we currently use Vulkan. Support for other graphics APIs can be added in the future.

## Contribution
### How can I contribute?
We welcome contributions! Without your help, we won't be able to fulfill our golden goal.
- [Contributing](https://github.com/NoiseStudio/docs/blob/master/Contributing.md) explains what kinds of contributions we welcome.
- [Building and testing](#building) instructions.

In addition, do not forget to format your Rust code with `rustfmt` and check it with `clippy` before filing a pull request:

Formatting ([installation guide](https://github.com/rust-lang/rustfmt#on-the-stable-toolchain)):
```
cargo fmt
```

Clippy ([installation guide](https://github.com/rust-lang/rust-clippy#step-2-install-clippy)):
```
cargo clippy
```

### Building and testing
To compile this project you will need [.NET 7.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0), as well as [the latest version of Rust](https://www.rust-lang.org/learn/get-started).

Once you have these, you can build the project by running the following command in the repository root:
```
dotnet build
```
And you can run tests by:
```
dotnet test
```
Rust compilation will be executed automatically and results will be shown in console output.

## License
NoiseEngine is licensed under the [MIT](/LICENSE) license.
