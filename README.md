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

## Build Instructions
The custom MonoGame fork used in this sample is available as a NuGet package, no need to build it yourself.<br>
As long as .Net 6 is installed, you can just open the csproj files in Visual Studio 2022, or launch directly from the command line:
```
dotnet run --project ShaderSampleGL.csproj
```
On Windows you can use ShaderSampleGL.csproj (OpenGL), or ShaderSampleDX.csproj (DirectX).<br>
On Linux you have to use ShaderSampleGL.csproj.<br>
Mac, Android and iOS are not yet available.

Here are more details about [NuGet packages, platform support and build requirements](https://github.com/cpt-max/Docs/blob/master/Build%20Requirements.md).
<br><br>

[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)




