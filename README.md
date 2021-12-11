[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Pixel-Sort Compute Shader for MonoGame

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/PixelSort.jpg?raw=true)

This sample uses a compute shader to sort pixels in a texture horizontally by hue.<br>
For each pair of pixels a compute thread is launched, that swaps the pixels (if neccessary) like a bubble sort.<br><br>
While this could also be done using Render-to-texture, it becomes easier to program, and probably faster, with a compute shader. This is because each compute thread can write to multiple pixels, which simplifies the code, and reduces the number of threads and texture reads required.  

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




