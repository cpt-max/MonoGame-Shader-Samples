[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Texture3D Compute Shader for MonoGame

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/Texture3D.jpg?raw=true)

This sample uses a compute shader to update a 3D texture on the GPU.<br>
The texture is initialized with a bunch of randomly colored pixels. The pixel's color represents a velocity, so pixels move through the volume.<br><br>

The visualisation of the 3D texture is accomplished by rendering each z-slice of the texture as a separate quad. As a result a slice will become invisible, when viewd exactly from the side.

## Build Instructions
The custom MonoGame fork used in this sample is available as a NuGet package, no need to build it yourself.<br>
As long as .Net 5 or 6 is installed, you can just open the csproj files in Visual Studio 2019/2022, or launch directly from the command line:
```
dotnet run --project ShaderSampleGL.csproj
```
On Windows you can use ShaderSampleGL.csproj (OpenGL), or ShaderSampleDX.csproj (DirectX).<br>
On Linux you have to use ShaderSampleGL.csproj.<br>
Mac, Android and iOS are not yet available.

Here are more details about [NuGet packages, platform support and build requirements](https://github.com/cpt-max/Docs/blob/master/Build%20Requirements).
<br><br>

[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)








