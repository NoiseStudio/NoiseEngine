# Project structure
NoiseEngine (core) have few another code projects in their repository:

- [NoiseEngine](/NoiseEngine/) - main code project with public API.
    - [BuiltInResources](/NoiseEngine/BuiltInResources/) - resources which are shipped with engine like default shader etc.
    - [Jobs](/NoiseEngine/Jobs/) - Entity Component System and Jobs (task) system.
    - [Nesl](/NoiseEngine/Nesl/) - NoiseEngine Shader Language - their compiler and things related to it.
    - [Rendering](/NoiseEngine/Rendering/) - All things related to rendering.
    - Add more which you can find in file tree.

- [NoiseEngine.Native](/NoiseEngine.Native/) - native part of the engine, where the more low-level and non-C# things are written (Rust part).
- [NoiseEngine.Tests](/NoiseEngine.Tests/) - tests of public API and things which written managed/C#.
- [NoiseEngine.Tests.Native](/NoiseEngine.Tests.Native/) - tests of native part which cannot exposed to managed part.
- [NoiseEngine.Generator](/NoiseEngine.Generator/) - C# source generator which improve creating games or apps by generated things.

- [NoiseEngine.Benchmarks](/NoiseEngine.Benchmarks/) - benchmarks of the engine.
- [NoiseEngine.Builder](/NoiseEngine.Builder/) - internal core builder (not CLI) which compiles built in resources in the engine.
- [NoiseEngine.InternalGenerator](/NoiseEngine.InternalGenerator/) - internal C# source generator which is only used in main code project to create engine source code.
- [NoiseEngine.InternalGenerator.Shared](/NoiseEngine.InternalGenerator.Shared/) - internal C# source generator which is only used in this repository to create engine source code.