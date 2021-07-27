#define GroupSize 64


//================================================================================================
// Compute Shader Sculpt Vertices
//================================================================================================

RWByteAddressBuffer Vertices;

float3 CamPos;
float3 MouseRay;
float Attract;
float DeltaTime;

[numthreads(GroupSize, 1, 1)]
void CS_SculptVertices(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint  localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    uint vertexID = globalID.x;
    
    uint posFloats  = 3; // position is Vector3
    uint normFloats = 3; // normal is Vector3
    uint texFloats  = 2; // textureCoordinate is Vector2
    uint totalFloats = posFloats + normFloats + texFloats;
    
    uint bytesPerFloat = 4;   
    uint vertexByteInd = vertexID * totalFloats * bytesPerFloat;
    
    uint posByteInd  = vertexByteInd;
    uint normByteInd = posByteInd  + posFloats  * bytesPerFloat;
    uint texByteInd  = normByteInd + normFloats * bytesPerFloat;
    
    float3 pos  = asfloat(Vertices.Load3(posByteInd));
    float3 norm = asfloat(Vertices.Load3(normByteInd));
    float2 tex  = asfloat(Vertices.Load2(texByteInd));
    
    float3 attractPoint = CamPos + MouseRay * dot(pos - CamPos, MouseRay);
    float3 attractDir = normalize(attractPoint - pos);
    float dist = distance(pos, attractPoint) * 10;
    float str = dist > 1 ? pow(dist, -1.2) : dist;
    
    pos += attractDir * Attract * str * DeltaTime;

    Vertices.Store3(posByteInd,  asuint(pos));
    Vertices.Store3(normByteInd, asuint(norm));
    Vertices.Store2(texByteInd,  asuint(tex));
}


//================================================================================================
// Compute Shader Flip Indices
//================================================================================================
RWByteAddressBuffer Indices;

[numthreads(GroupSize, 1, 1)]
void CS_FlipIndices(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    uint indexID = globalID.x * 6; // every thread is responsible for 2 triangle / 6 indices 
    uint bytesPerIndex = 2; // the index buffer uses IndexElementSize.SixteenBits => 2 bytes
    uint byteAddr = indexID * bytesPerIndex;
    
    // load 3 uints of 32 bit each, since our indices are only 16 bit, this will load 6 indices at once.
    uint3 inds = Indices.Load3(byteAddr);
    
    // extract the 6 indices, zero out the bits not belonging to the respective index
    // with IndexElementSize.ThirtytwoBits those bit-operations wouldn't be neccessary
    uint ind1 = inds.x & 0x0000ffff;
    uint ind2 = inds.x & 0xffff0000;
    uint ind3 = inds.y & 0x0000ffff;
    uint ind4 = inds.y & 0xffff0000;
    uint ind5 = inds.z & 0x0000ffff;
    uint ind6 = inds.z & 0xffff0000;
    
    // swap index 1 and 3, swap index 4 and 5 
    // merge 2 16 bit indices back into a single 32bit uints
    inds.x = ind3 | ind2;
    inds.y = ind1 | ind6;
    inds.z = ind5 | ind4;
  
    Indices.Store3(byteAddr, inds);
}

//==============================================================================
// Vertex shader
//==============================================================================
struct VertexIn
{
    float3 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexOut
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

float4x4 WorldViewProjection;

VertexOut VS(in VertexIn input)
{
    VertexOut output;

    output.Position = mul(float4(input.Position.xyz, 1), WorldViewProjection);
    output.Normal = input.Normal;
    output.TexCoord = input.TexCoord;
	
    return output;
}

//==============================================================================
// Pixel shader 
//==============================================================================
float4 PS(VertexOut input) : SV_TARGET
{
    return float4(input.Normal, 1);
}

//================================================================================================
// Techniques
//================================================================================================
technique SculptVertices
{
    pass Pass0
    {
        ComputeShader = compile cs_5_0 CS_SculptVertices();
    }
}

technique FlipIndices
{
    pass Pass0
    {
        ComputeShader = compile cs_5_0 CS_FlipIndices();
    }
}

technique DrawMesh
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 VS();
        PixelShader = compile ps_4_0 PS();
    }
}