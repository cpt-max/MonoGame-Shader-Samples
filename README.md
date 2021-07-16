[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)
# Tessellation and Geometry Shader for MonoGame

![Screenshot](/Screenshot.jpg?raw=true)

This sample uses a very simple hull and domain shader to tessellate a single input triangle into many sub triangles. Each sub triangle is then passed into a geometry shader to generate even more triangles along it's edges, which creates a wireframe-like effect.

You can switch between different techniques, which effectively lets you disable tessellation and/or the geometry shader.

### Build for OpenGL
- Open ShaderTestGL.csproj.
- Make sure MonoGame.Framework.DesktopGL from this [MonoGame fork](https://github.com/cpt-max/MonoGame/tree/compute_shader) is referenced.
- Rebuild the content in ShaderTestGL.mgcb using the MGCB Editor from that fork.

### Build for DirectX
- Open ShaderTestDX.csproj.
- Make sure MonoGame.Framework.WindowsDX from this [MonoGame fork](https://github.com/cpt-max/MonoGame/tree/compute_shader) is referenced. 
- Rebuild the content in ShaderTestDX.mgcb using the MGCB Editor from that fork. 

[Download the prebuilt executables for Windows](https://www.dropbox.com/s/c5h81mtgw5pnctu/Monogame%20Shader%20Samples.zip?dl=1)

If you are only interested in the DirectX version, you can also use this [DX only branch](https://github.com/cpt-max/MonoGame/tree/shader) instead. This branch doesn't contain the switch from MojoShader to ShaderConductor, so it's a lot lighter.

Thanks to JSandusky as the Hull, Domain and Geometry shader support for DirectX is based on [his MonoGame fork](https://github.com/JSandusky/MonoGame).

<br><br>
[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)








