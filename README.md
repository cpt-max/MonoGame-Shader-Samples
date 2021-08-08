[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Particles with Indirect Draw

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/ParticlesIndirectDraw.jpg?raw=true)

This sample uses a compute shader to spawn, destroy and update particles.<br>
Since the spawn and destroy logic is done on the GPU, the CPU doesn't know how many particles to draw.<br>
Using indirect draw makes it possible to draw and update the correct number of particles, without the need to download that data from the GPU to the CPU.

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




