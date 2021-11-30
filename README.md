[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Object Culling Compute Shader with Indirect Draw

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/ObjectCulling.jpg?raw=true)

This is the same sample as [here](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/object_culling_indirect_draw), but it uses an append buffer instead of a regular structured buffer. This slightly simplifies the code.<br>
Have a look at the [diff between the branches](https://github.com/cpt-max/MonoGame-Shader-Samples/compare/object_culling_indirect_draw...object_culling_indirect_draw_append). The indirect draw buffer, including the atomic counter operation on it, got completely removed from the shader, replaced by the Append() call.<br>
In order to still update the counter value in the indirect draw buffer, the CopyCounterValue() function is used.

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




