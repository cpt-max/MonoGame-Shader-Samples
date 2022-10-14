[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Edit Mesh Compute Shader for MonoGame

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/EditMesh.jpg?raw=true)

This sample uses a compute shader to modify a vertex and an index buffer directly on the GPU.<br>
The mouse can be used to distort vertex positions.<br>
The mesh normals are visualized, but they don't update in response to mesh modifications. This would complicate the compute shader quite a bit.<br>
Pressing the tab key will reverse the triangle winding order in the index buffer. The same effect could of course be created much simpler, without index buffer modification, but that's not the point of this sample.


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




