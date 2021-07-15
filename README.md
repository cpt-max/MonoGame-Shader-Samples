# Collision Test Compute Shader for MonoGame

![Screenshots](/Screenshot.jpg?raw=true)

This sample uses a compute shader to do brute-force collision checks between circles. The buffer containing the collision results is then downloaded to the CPU, in order to color the circles according to how many collisions they are involved in.  

### Build for OpenGL
- Open ShaderTestGL.csproj.
- Make sure MonoGame.Framework.DesktopGL from this [MonoGame fork](https://github.com/cpt-max/MonoGame/tree/compute_shader) is referenced.
- Rebuild the content in ShaderTestGL.mgcb using the MGCB Editor from that fork.

### Build for DirectX
- Open ShaderTestDX.csproj.
- Make sure MonoGame.Framework.WindowsDX from this [MonoGame fork](https://github.com/cpt-max/MonoGame/tree/compute_shader) is referenced. 
- Rebuild the content in ShaderTestDX.mgcb using the MGCB Editor from that fork. 








