[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Object Culling Compute Shader with Indirect Draw

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/ObjectCulling.jpg?raw=true)

This is the same sample as [here](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/object_culling_indirect_draw), but it uses an append buffer instead of a regular structured buffer. This slightly simplifies the code.<br>
Have a look at the [diff between the branches](https://github.com/cpt-max/MonoGame-Shader-Samples/compare/object_culling_indirect_draw...object_culling_indirect_draw_append). The indirect draw buffer, including the atomic counter operation on it, got completely removed from the shader, replaced by the Append() call.<br>
In order to still update the counter value in the indirect draw buffer, the CopyCounterValue() function is used.

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




