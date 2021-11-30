[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Object Culling Compute Shader with Indirect Draw

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/ObjectCulling.jpg?raw=true)

This sample uses a compute shader to determine the visibility of objects directly on the GPU.<br>
A structured buffer is filled up with all the visible objects, which are then draw using indirect draw. This has the advantage that no data has to be downloaded from the GPU to the CPU.<br>

The visibility check is a simple distance check here. While such a simple calculation could easily be done on the CPU, in a real world scenario you may have a much more complex algorithm, like some type of occlusion culling for example.<br>

There is a [converted version of this sample using an append buffer](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/object_culling_indirect_draw_append), which slightly simplifies the code.

### Build for OpenGL
- Open ShaderTestGL.csproj.
- Make sure MonoGame.Framework.DesktopGL from this [MonoGame fork](https://github.com/MonoGame/MonoGame/pull/7533) is referenced.
- Rebuild the content in ShaderTestGL.mgcb using the MGCB Editor from that fork.

### Build for DirectX
- Open ShaderTestDX.csproj.
- Make sure MonoGame.Framework.WindowsDX from this [MonoGame fork](https://github.com/MonoGame/MonoGame/pull/7533) is referenced. 
- Rebuild the content in ShaderTestDX.mgcb using the MGCB Editor from that fork. 

[Download the prebuilt executables for Windows](https://www.dropbox.com/s/c5h81mtgw5pnctu/Monogame%20Shader%20Samples.zip?dl=1)
<br><br>
[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)




