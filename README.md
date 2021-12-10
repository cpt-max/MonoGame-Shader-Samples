[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Edge-rounding Tessellation Shader for MonoGame

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/EdgeRounding.jpg?raw=true)

This sample uses a hull and domain shader to round off the edges of a mesh. The mesh is created out of quad patches. The rounding radius and tesselltation factor can be changed dynamically.  

### Limitations
- This method works for corners defined by 3 edges. It's fine to have more edges, if the extra edges are in one of the 3 planes defined by the first 3 edges. In this case the shape of the corner is not influenced by the extra edges, so they can be ignored when calculating the vertex normal.
- This method has only been tested for convex corners. It should also work for concave corners (like beeing on the inside of a cube that surrounds you), but the generated normals probably need to be flipped. Corners that mix convex and concave edges (like the inside corners of a rectangular frame) will not work without modifications.
- The texture mapping doesn't work properly when the shear angles are not zero.

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








