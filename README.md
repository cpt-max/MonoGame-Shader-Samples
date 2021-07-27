[< Back to overview](https://github.com/cpt-max/MonoGame-Shader-Samples/tree/overview)

# Edit Mesh Compute Shader for MonoGame

![Screenshots](https://github.com/cpt-max/MonoGame-Shader-Samples/blob/overview/Screenshots/EditMesh.jpg?raw=true)

This sample uses a compute shader to modify a vertex and an index buffer directly on the GPU.<br>
The mouse can be used to distort vertex positions.<br>
The mesh normals are visualized, but they don't update in response to mesh modifications. This would complicate the compute shader quite a bit.<br>
Pressing the tab key will reverse the triangle winding order in the index buffer. The same effect could of course be created much simpler, without index buffer modification, but that's not the point of this sample.

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








