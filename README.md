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

> WARNING:<br>
NoiseEngine is still in VERY EARLY development. Frequent changes, missing docs, and instability may occur.<br>
You SHOULD avoid using it for any game development before v0.1.0 was released. But [contributions](#how-can-i-contribute) are welcome.

## Why?
Popular game engines was created many years ago based on synchronous API which does not use all power of modern CPU's. When we creating own next generation procedural game prototype in 2021 with existing engine we started having difficulties to optimize internal things in engine which we used. We started looking for a new engine, but they all had the same problems, so we rethought them and made the difficult decision to create our own engine - NoiseEngine.

Our golden goal is create most performant engine, which relatively easy to game developers create games which we currently cannot do. Also in AAA scale.

NoiseEngine is not a story of 9th generation of consoles. Is not a story of primitive tool like current engines. Is a story of future with new perspective.

![Voxel terrain](/images/voxel-terrain.webp)
*Real-time voxel terrain demo, render distance 8192 meters/blocks - [1 Februrary 2023](https://github.com/NoiseStudio/NoiseEngine/releases/tag/v0.1.0-alpha1)*

## Overview
### Documentation
Currently (before v0.1.0) initial documentation still is writen. It is not published on any website, you can read it from [documentation comments](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments). And manuals currently not exists, and examples you can find in [tests](/docs/project-structure.md). 

### Project structure
We have a few repositories which contain parts of the entire project:
- [Current](https://github.com/NoiseStudio/NoiseEngine) - contains core of the engine, all API which you will be used in your application.
- [NoiseEngine.Cli](https://github.com/NoiseStudio/NoiseEngine.Cli) - command line interface tools for building and shipping games or apps with NoiseEngine
- [NoiseEngine.Editor](https://github.com/NoiseStudio/NoiseEngine.Editor) - graphical user interface for simpler game or apps creation

You can see more specify structure about this repository [here](/docs/project-structure.md).

### Versioning
We want to use [sementic versioning](https://semver.org/), and after first version release (v0.1.0) we want offten release patch updates. But we are of the opinion that branch master should always contains stable code for old features, as far as posible.

### Technology stack
NoiseEngine uses .NET 7 and new versions of Rust. For rendering we currently use a Vulkan, but support of other graphics APIs can be added in the future.

## Contribution
### How can I contribute?
We welcome contributions! Without this, we are not fulfilling our golden goal.
- [Contributing](https://github.com/NoiseStudio/docs/blob/master/Contributing.md) explains what kinds of contributions we welcome.
- [Building and testing](#building) instructions.

In addition, do not forget to format the Rust code and detect errors and fix them with the compiler and clippy before you post a pull request.

Formatting ([installation guide](https://github.com/rust-lang/rustfmt#on-the-stable-toolchain)):
```
cargo fmt
```

Clippy ([installation guide](https://github.com/rust-lang/rust-clippy#step-2-install-clippy)):
```
cargo clippy
```

### Building and testing
To compile this project you need to have [.NET 7.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) and [Rust 1.70 or newest](https://www.rust-lang.org/learn/get-started).

After when you have this tools you can build project with running command in repository root:
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
