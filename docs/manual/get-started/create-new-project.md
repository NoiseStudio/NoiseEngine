# Create new project

## With CLI
Future and simpler version of project creation, currently not available.

## Without CLI
### Project creation
Raw and advanced way to create new NoiseEngine project is creating a necessary files handly. First thing which we must do will be create a new C#'s console project, we can do this with command:
```
dotnet new console --name HelloNE
```

After that file named `HelloNE/HelloNE.csproj` should be created and should looks similar to this:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```
Then we must attach dependencies to source code of NoiseEngine:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <Import Project="{{ PATH TO NOISEENGINE REPOSITORY }}/NoiseEngine.Imports.xml" />

</Project>
```

### Opening window
Now we can open window with your next game, we can do this by modifinig `Program.cs` file in project directory:
```csharp
using NoiseEngine;
using NoiseEngine.DeveloperTools.Systems;

// Initialize NoiseEngine's Application
Application.Initialize(new());

// Create empty scene and camera with default window.
var scene = new ApplicationScene();
var camera = new Camera(scene) {
    RenderTarget = new Window(),
    RenderLoop = new PerformanceRenderLoop()
};

// Initialize debug movement system to camera which allows us to move.
DebugMovementSystem.InitializeTo(camera);

// Prevert closing application when we do not want to do that.
Application.WaitToEnd();
```
And after command:
```
dotnet run
```
your's app's window will be open and everything should looks like:
![Opened window](/images/docs/manual/get-started/create-new-project/opening-window.webp)

### Show anything
Developers fastly want to see something on the screen and to do that we can use Primitives, simple objects which are built-in into engine.

To add a primitive cube we must add to your game's `Program.cs` file just a few lines:
```csharp
...keep all previous lines

scene.Primitive.CreateCube();

// Prevert closing application when we do not want to do that.
Application.WaitToEnd();
```
And now, after running application we should see:
![Simple cube](/images/docs/manual/get-started/create-new-project/primitives.webp)
If you do not see it, probably you are rotated wrong, move your mouse fast and find a cube.

### Source code
You can find source code of this and another examples [here](/examples/manual/get-started/create-new-project/README.md).
