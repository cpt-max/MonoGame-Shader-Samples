[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Texture3D Compute Shader for MonoGame

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/Texture3D.jpg?raw=true)

This sample uses a compute shader to update a 3D texture on the GPU.<br>
The texture is initialized with a bunch of randomly colored pixels. The pixel's color represents a velocity, so pixels move through the volume.<br><br>

The visualisation of the 3D texture is accomplished by rendering each z-slice of the texture as a separate quad. As a result a slice will become invisible, when viewd exactly from the side.

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








