[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Collision Test Compute Shader for MonoGame

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/ComputeCircles.jpg?raw=true)

This sample uses a compute shader to do brute-force collision checks between circles. The buffer containing the collision results is then downloaded to the CPU, in order to color the circles according to how many collisions they are involved in.  

## Build Instructions
The custom MonoGame fork used in this sample is available as a NuGet package, no need to build it yourself.<br>
As long as .Net 6 is installed, you can just open the csproj files in Visual Studio 2022, or launch directly from the command line:
```
dotnet run --project ShaderSampleGL.csproj
```
On Windows you can use ShaderSampleGL.csproj (OpenGL), or ShaderSampleDX.csproj (DirectX).<br>
On Linux you have to use ShaderSampleGL.csproj.<br>
Mac, Android and iOS are not yet available.

Here are more details about [NuGet packages, platform support and build requirements](https://github.com/cpt-max/Docs/blob/master/Build%20Requirements.md).
<br><br>

[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)




