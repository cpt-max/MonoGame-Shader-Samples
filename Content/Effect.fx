struct Particle
{
    float2 pos;
    float2 vel;
};

//=============================================================================
// Compute Shader
//=============================================================================
#define GroupSize 256

RWStructuredBuffer<Particle> Particles;

float DeltaTime;
float Force;
float2 ForceCenter;

[numthreads(GroupSize, 1, 1)]
void CS(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
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
    float2 TexCoord : TEXCOORD0;
    uint VertexID : SV_VertexID;
};

struct VertexOut
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

VertexOut VS(in VertexIn input)
{
    VertexOut output;
    
    uint particleID = input.VertexID / 4;
    Particle p = ParticlesReadOnly[particleID];
    
    float2 size = float2(1, 16.0 / 9.0) * 0.001;
    float2 pos = p.pos + input.Position.xy * size;

    output.Position = float4(pos, 0, 1);
    output.TexCoord = input.TexCoord;
	
    return output;
}

//==============================================================================
// Pixel shader 
//==============================================================================
float4 PS(VertexOut input) : SV_TARGET
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
        ComputeShader = compile cs_5_0 CS();
    }
}