[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Collision Test Compute Shader for MonoGame

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/ComputeCircles.jpg?raw=true)

This sample uses a compute shader to do brute-force collision checks between circles. The buffer containing the collision results is then downloaded to the CPU, in order to color the circles according to how many collisions they are involved in.  

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








