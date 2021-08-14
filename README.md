[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Particles with Indirect Draw

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/ParticlesIndirectDraw.jpg?raw=true)

This sample uses a compute shader to spawn, destroy and update particles.<br>
Since the spawn and destroy logic is done on the GPU, the CPU doesn't know how many particles to draw.<br>
Using indirect draw makes it possible to draw and update the correct number of particles, without the need to download that data to the CPU.

Holding the left mouse button will make particles in range spawn child particles and turn red. For a particle to spawn another child you have to wait until the red has turned blue again. The right mouse button will erase particles in range.

In contrast to the [simpler indirect draw sample here](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/object_culling_indirect_draw), this sample also demonstrates indirect dispatch, to update the particles. The indirect draw buffer contains both, the draw arguments, as well as the group counts for the dispath call, plus some extra variables.

There's two particle buffers, and two indirect draw buffers, which are used in a ping-pong fashion. That means in the first frame buffer 1 will be the input, and buffer 2 will be the output, which will be filled with the surviving and newely spawned particles. In the next frame the buffers switch roles. The output buffer becomes the new input buffer, and vice versa. 

Since instanced drawing is not very efficient for low vertex count objects, like the particle quads here, each instance will draw multiple particles at once. This complicates the shaders a bit, but improves performance substatially. 
For the FPS counter to make sense, you have to outcomment 2 lines
```C#
graphics.SynchronizeWithVerticalRetrace = false;
IsFixedTimeStep = false;
```

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




