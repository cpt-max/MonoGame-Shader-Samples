struct Particle
{
    float2 pos;
    float2 vel;
};

//=============================================================================
// Compute Shader
//=============================================================================
#define ComputeGroupSize 256

RWStructuredBuffer<Particle> Particles;

float DeltaTime;
float Force;
float2 ForceCenter;

[numthreads(ComputeGroupSize, 1, 1)]
void CS(uint3 localID : SV_GroupThreadID, uint3 dispatchID : SV_GroupID,
	    uint  localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    Particle p = Particles[globalID.x];
    
    float2 toCenter = ForceCenter - p.pos;
    float distSqr = dot(toCenter, toCenter);
    
    p.vel += (Force * DeltaTime / distSqr) * normalize(toCenter); // apply force
    p.vel *= max(0.1, 1 - DeltaTime * dot(p.vel, p.vel) * 100); // velocity damping
    
    p.pos += p.vel * DeltaTime; // move
    p.pos -= (p.pos >  1) * 2; // wrap on border
    p.pos += (p.pos < -1) * 2; // wrap on border
    
    Particles[globalID.x] = p;
}

//==============================================================================
// Vertex shader
//==============================================================================
StructuredBuffer<Particle> ParticlesReadOnly;

struct VertexIn
{
    float3 Position : POSITION0;
    uint VertexID : SV_VertexID;
};

struct VertexOut
{
    float4 Position : SV_POSITION;
    float2 ParticlePos : TexCoord0;
};

VertexOut VS(in VertexIn input)
{
    VertexOut output;
    
    Particle p = ParticlesReadOnly[input.VertexID];
    output.Position = float4(p.pos, 0, 1);
    output.ParticlePos = p.pos;
	
    return output;
}


//==============================================================================
// Geometry shader 
//==============================================================================
struct GeomOut
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TexCoord0;
};

[maxvertexcount(4)]
void GS(point in VertexOut input[1], inout TriangleStream<GeomOut> output)
{ 
    GeomOut v0, v1, v2, v3;
    
    float2 pos = input[0].ParticlePos;
    float2 size = float2(1, 16.0 / 9.0) * 0.001;
    
    v0.Position = float4(pos + float2(-size.x, -size.y), 0, 1);
    v1.Position = float4(pos + float2(-size.x, +size.y), 0, 1);
    v2.Position = float4(pos + float2(+size.x, +size.y), 0, 1);
    v3.Position = float4(pos + float2(+size.x, -size.y), 0, 1);
    
    v0.TexCoord = float2(0, 1);
    v1.TexCoord = float2(0, 0);
    v2.TexCoord = float2(1, 0);
    v3.TexCoord = float2(1, 1);
    
    output.Append(v0);
    output.Append(v1);
    output.Append(v2);
    output.Append(v3);
}

//==============================================================================
// Pixel shader 
//==============================================================================
float4 PS(GeomOut input) : SV_TARGET
{
    return float4(0.5, 0.8, 1, 1);
}


//===============================================================================
// Techniques
//===============================================================================
technique Tech0
{
    pass Pass0
    {
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_4_0 PS();
        GeometryShader = compile gs_4_0 GS();
        ComputeShader = compile cs_5_0 CS();
    }
}