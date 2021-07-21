[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Collision Test Compute Shader for MonoGame

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/PixelSort.jpg?raw=true)

This sample uses a compute shader to sort pixels in a texture horizontally by hue.<br>
For each pair of pixels a compute thread is launched, that swaps the pixels (if neccessary) like a bubble sort.<br>
While this could also be done using Render-to-texture, it becomes easier to program, and probably faster, in a compute shader. This is because each compute thread can write to multiple pixels, which simplifies the code, and reduces the number of total threads and texture reads required.  

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








