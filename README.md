[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Particle Compute & Geometry Shader for MonoGame

![Screenshots](/Screenshot.jpg?raw=true)

This sample uses a compute shader to update particles on the GPU. 
The particle buffer is used directly by the vertex shader that draws the particles. Since no data needs to be downloaded to the CPU, this method is very fast.

A geometry shader is used to generate the quads for rendering the particles. This version performs a bit better on my RX 5700XT (170 FPS for 10 Mio. particles) than the [version without geometry shader](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/compute_gpu_particles) (145 FPS for 10 Mio. particles).

For performance tests you have to outcomment 2 lines, otherwise the FPS counter doesn't make sense, and probably up the MaxParticleCount:
```
graphics.SynchronizeWithVerticalRetrace = false;
IsFixedTimeStep = false;
```

### Build for OpenGL
- Open ShaderTestGL.csproj.
- Make sure MonoGame.Framework.DesktopGL from this [MonoGame fork](https://github.com/cpt-max/MonoGame/tree/compute_shader) is referenced.
- Rebuild the content in ShaderTestGL.mgcb using the MGCB Editor from that fork.

### Build for DirectX
- Open ShaderTestDX.csproj.
- Make sure MonoGame.Framework.WindowsDX from this [MonoGame fork](https://github.com/cpt-max/MonoGame/tree/compute_shader) is referenced. 
- Rebuild the content in ShaderTestDX.mgcb using the MGCB Editor from that fork. 

[Download the prebuilt executables for Windows](https://www.dropbox.com/s/c5h81mtgw5pnctu/Monogame%20Shader%20Samples.zip?dl=1)
<br><br>
[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)




