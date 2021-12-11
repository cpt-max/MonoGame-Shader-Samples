[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Particle Compute Shader for MonoGame

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/ComputeParticles.jpg?raw=true)

This sample uses a compute shader to update particles on the GPU. 
The particle buffer is used directly by the vertex shader that draws the particles. Since no data needs to be downloaded to the CPU, this method is very fast.

For the FPS counter to make sense, you have to outcomment 2 lines
```
graphics.SynchronizeWithVerticalRetrace = false;
IsFixedTimeStep = false;
```

## Build Instructions
The custom MonoGame fork used in this sample is available as a NuGet package, no need to build it yourself.<br>
As long as .Net 5 or 6 is installed, you can just open the csproj files in Visual Studio 2019/2022, or launch directly from the command line:
```
dotnet run --project ShaderSampleGL.csproj
```
On Windows you can use ShaderSampleGL.csproj (OpenGL), or ShaderSampleDX.csproj (DirectX).<br>
On Linux you have to use ShaderSampleGL.csproj.<br>
Mac, Android and iOS are not yet available.

Here are more details about [NuGet packages, platform support and build requirements](https://github.com/cpt-max/Docs/blob/master/Build%20Requirements.md).
<br><br>

[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)




