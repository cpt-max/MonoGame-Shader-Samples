[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Object Culling Compute Shader with Indirect Draw

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/ObjectCulling.jpg?raw=true)

This sample uses a compute shader to determine the visibility of objects directly on the GPU.<br>
A structured buffer is filled up with all the visible objects, which are then draw using indirect draw. This has the advantage that no data has to be downloaded from the GPU to the CPU.<br>

The visibility check is a simple distance check here. While such a simple calculation could easily be done on the CPU, in a real world scenario you may have a much more complex algorithm, like some type of occlusion culling for example.<br>

There is a [converted version of this sample using an append buffer](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/object_culling_indirect_draw_append), which slightly simplifies the code.

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




