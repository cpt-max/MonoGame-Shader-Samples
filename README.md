# MonoGame Shader Samples Overview

Each sample is in a separate branch of this repository.<br>
The samples are based on a [custom MonoGame fork](https://github.com/cpt-max/MonoGame/tree/compute_shader), that adds tesselation, geometry and compute shaders.<br>
Lazy people can [download the prebuilt executables for Windows](https://www.dropbox.com/s/c5h81mtgw5pnctu/Monogame%20Shader%20Samples.zip?dl=1).
<br><br>

## [Simple Tesselation & Geometry Shader](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/tesselation_geometry)
[<img align="left" width="400" src="Screenshots/TesselationGeometry.jpg">](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/tesselation_geometry)
This sample uses a very simple hull and domain shader to tessellate a single input triangle into many sub triangles. Each sub triangle is then passed into a geometry shader to generate even more triangles along it's edges, which creates a wireframe-like effect.

<br clear="left"/><br><br>

## [Edge-rounding Tessellation Shader](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/edgerounding)
[<img align="left" width="400" src="Screenshots/EdgeRounding.jpg">](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/edgerounding)
This sample uses a hull and domain shader to round off the edges of a mesh. The mesh is created out of quad patches. The rounding radius and tesselltation factor can be changed dynamically.

<br clear="left"/><br><br>

## [Particle Compute Shader](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/compute_gpu_particles)
[<img align="left" width="400" src="Screenshots/ComputeParticles.jpg">](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/compute_gpu_particles)

This sample uses a compute shader to update particles on the GPU. The particle buffer is used directly by the vertex shader that draws the particles. Since no data needs to be downloaded to the CPU, this method is very fast.

<br clear="left"/><br><br>

## [Collision Test Compute Shader](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/compute_cpu)
[<img align="left" width="400" src="Screenshots/ComputeCircles.jpg">](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/compute_cpu)
This sample uses a compute shader to do brute-force collision checks between circles. The buffer containing the collision results is then downloaded to the CPU, in order to color the circles according to how many collisions they are involved in.

<br clear="left"/><br><br>









