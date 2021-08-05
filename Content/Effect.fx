struct Particle
{
    float2 pos;
    float2 vel;
    float age;
    float padding;
};

//=============================================================================
// Compute Shader
//=============================================================================
#define GroupSize 256

StructuredBuffer<Particle> ParticlesIn;
RWStructuredBuffer<Particle> ParticlesOut;

RWBuffer<uint> IndirectDrawIn;
RWBuffer<uint> IndirectDrawOut;

int MaxParticleCount;
int RandInt;

float2 MousePos;
float Spawn;
float SpawnRadius;
float DeltaTime;

[numthreads(GroupSize, 1, 1)]
void CS(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint  localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    uint particleCount = IndirectDrawIn[4];
    if (globalID.x >= particleCount) 
        return;
    
    Particle p = ParticlesIn[globalID.x];
    
    // move and age particle
    p.pos += p.vel * DeltaTime;
    p.pos -= (p.pos > 1) * 2; // wrap on border
    p.pos += (p.pos < -1) * 2; // wrap on border
    p.age += DeltaTime * length(p.vel * 10);

    // spawn and erase particles
    if (Spawn != 0)
    {
        float2 toCenter = MousePos - p.pos;
        float distSqr = dot(toCenter, toCenter);
        if (distSqr < SpawnRadius * SpawnRadius)
        { 
            if (Spawn < 0)
            {
                 // erase by returning early, before the particle gets added to the output buffer
                return;
            }  
            else if (p.age > 0.5) // only particles of a certain age can spawn child particles
            {
                p.age = 0; // reset the particles age, so it can't immidiately spawn another child particle
                
                // spawn new particles
                uint particleOutID;
                InterlockedAdd(IndirectDrawOut[4], 1, particleOutID); // increment the particle count in the indirect draw buffer
                
                Particle pNew;
                pNew.pos = p.pos;
                pNew.vel = ParticlesIn[(particleOutID + (uint)RandInt) % ((uint)MaxParticleCount)].vel; // grab the velocity from another random particle in the buffer
                pNew.age = 0;
                
                ParticlesOut[particleOutID] = pNew;
            }
        }
    }

    // output particle 
    uint particleOutID;
    InterlockedAdd(IndirectDrawOut[4], 1, particleOutID); // increment the particle count in the indirect draw buffer
    InterlockedMin(IndirectDrawOut[4], (uint) MaxParticleCount); // limit to MaxParticleCount
    
    particleOutID = min(particleOutID, MaxParticleCount - 1);
    ParticlesOut[particleOutID] = p;
    
    // set group count in indirect draw buffer, which will be used in the next DispatchCompute call
    uint particleOutCount = particleOutID + 1;
    uint groupCount = particleOutCount / GroupSize + 1; 
    InterlockedMax(IndirectDrawOut[0], groupCount);
}

//==============================================================================
// Vertex shader
//==============================================================================
StructuredBuffer<Particle> ParticlesDraw;

struct VertexIn
{
    float3 Position : POSITION0;
    uint InstanceId : SV_InstanceID;
};

struct VertexOut
{
    float4 Position : SV_POSITION;
    float2 ParticlePos : TexCoord0;
    float ParticleAge : TexCoord1;
};

VertexOut VS(in VertexIn input)
{
    VertexOut output;
    
    Particle p = ParticlesDraw[input.InstanceId];
    output.Position = float4(p.pos, 0, 1);
    output.ParticlePos = p.pos;
    output.ParticleAge = p.age;
	
    return output;
}

//==============================================================================
// Geometry shader 
//==============================================================================
struct GeomOut
{
    float4 Position : SV_POSITION;
    float ParticleAge : TexCoord0;
};

[maxvertexcount(6)]
void GS(point in VertexOut input[1], inout PointStream<GeomOut> output)
{ 
    GeomOut v0, v1, v2, v3;
    
    float2 pos = input[0].ParticlePos;
    float2 size = float2(1, 16.0 / 9.0) * 0.001;
    
    v0.Position = float4(pos + float2(-size.x, -size.y), 0, 1);
    v1.Position = float4(pos + float2(-size.x, +size.y), 0, 1);
    v2.Position = float4(pos + float2(+size.x, +size.y), 0, 1);
    v3.Position = float4(pos + float2(+size.x, -size.y), 0, 1);
    
    v0.ParticleAge = input[0].ParticleAge;
    v1.ParticleAge = input[0].ParticleAge;
    v2.ParticleAge = input[0].ParticleAge;
    v3.ParticleAge = input[0].ParticleAge;
    
    output.Append(v0);
    output.Append(v1);
    output.Append(v2);
    output.RestartStrip();
    
    output.Append(v0);
    output.Append(v2);
    output.Append(v3);
}

//==============================================================================
// Pixel shader 
//==============================================================================
float4 PS(VertexOut input) : SV_TARGET
{
    float ripe = input.ParticleAge / 0.5;
    return ripe > 1 ?
        float4(0.5, 0.8, 1, 1) : 
        float4(1, ripe, 0, 1);
}

//===============================================================================
// Techniques
//===============================================================================
technique Tech0
{
    pass Pass0
    {
        ComputeShader = compile cs_5_0 CS();
        VertexShader = compile vs_5_0 VS();
        PixelShader = compile ps_4_0 PS();
        //GeometryShader = compile gs_4_0 GS();
    }
}