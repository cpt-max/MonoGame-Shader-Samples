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

ByteAddressBuffer IndirectDrawIn;
RWByteAddressBuffer IndirectDrawOut;

int MaxParticleCount;
int RandInt;

float2 MousePos;
float Spawn;
float SpawnRadius;
float DeltaTime;

// define fields inside indirect draw buffer
static const uint indGroupCount = 0;
static const uint indInstanceCount = 4 * 4;
static const uint indParticleCount = 7 * 4;
static const uint indDummyParticle = 8 * 4;

[numthreads(GroupSize, 1, 1)]
void CS(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint  localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{   
    uint particleID = globalID.x;
    uint particleCount = IndirectDrawIn.Load(indParticleCount);
    uint outID, _;
    
    // when particleCount is not an exact multiple of GroupSize we have some dummy particles at the end
    if (particleID >= particleCount)
        return;
    
    Particle p = ParticlesIn[particleID];
    
    // move and age particle
    p.pos += p.vel * DeltaTime;
    p.pos -= (p.pos >  1) * 2; // wrap on border
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
                p.age = 0; // reset the particle age, so it can't immidiately spawn another child particle
                
                // spawn new particles
                IndirectDrawOut.InterlockedAdd(indParticleCount, 1, outID); // increment the particle count in the indirect draw buffer
                
                Particle pNew;
                pNew.pos = p.pos;
                pNew.vel = ParticlesIn[(outID + (uint) RandInt) % ((uint) MaxParticleCount)].vel; // grab the velocity from another random particle in the buffer
                pNew.age = 0;
                pNew.padding = 0;
                
                ParticlesOut[outID] = pNew;
            }
        }
    }

    // output particle 
    IndirectDrawOut.InterlockedAdd(indParticleCount, 1, outID); // increment particle count in the indirect draw buffer  
    outID = min(outID, MaxParticleCount - 1);
    ParticlesOut[outID] = p;
    
    // set groupCountX in indirect draw buffer, which will be used in the next DispatchComputeIndirect call.
    // set the same group count also to the instanceCount for the next indirect draw call.
    // each instance will draw an entire group of particles, this is more efficient than drawing a single particle per instance.
    uint particleOutCount = outID + 1;
    uint groupCount = particleOutCount / GroupSize + (particleOutCount % GroupSize > 0);
    groupCount = min(groupCount, (uint)MaxParticleCount / GroupSize); // limit to MaxParticleCount
    
    IndirectDrawOut.InterlockedMax(indGroupCount, groupCount, _); // for indirect dispatch 
    IndirectDrawOut.InterlockedMax(indInstanceCount, groupCount, _); // for indirect draw
}

//==============================================================================
// Vertex shader
//==============================================================================
StructuredBuffer<Particle> ParticlesDraw;

struct VertexIn
{
    float3 Position : POSITION0;
    uint InstanceID : SV_InstanceID;
    uint VertexID : SV_VertexID;
};

struct VertexOut
{
    float4 Position : SV_POSITION;
    float2 ParticlePos : TexCoord0;
    float ParticleAge : TexCoord1;
};

VertexOut VS(in VertexIn input)
{
    VertexOut output = (VertexOut)0;
    
    uint particleCount = IndirectDrawIn.Load(indParticleCount);
    uint particleID = input.InstanceID * GroupSize + input.VertexID;
    
    if (particleID >= particleCount)
    {
        output.Position.x = 10000000; // move off screen
        return output;
    }
    
    Particle p = ParticlesDraw[particleID];
    
    output.Position = float4(p.pos, 0, 1);
    output.ParticlePos = p.pos;
    output.ParticleAge = p.age;
	
    return output;
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
    }
}