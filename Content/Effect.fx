struct Monkey
{
    float3 pos;
    float pad1; // pad to float4
    float3 vel;
    float pad2; // pad to float4
};

//=============================================================================
// Compute Shader
//=============================================================================
#define GroupSize 64

RWStructuredBuffer<Monkey> AllMonkeys;
AppendStructuredBuffer<Monkey> VisibleMonkeys;

float WorldSize;
float DeltaTime;
float CullRadius;

[numthreads(GroupSize, 1, 1)]
void CS(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint  localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{   
    Monkey monkey = AllMonkeys[globalID.x];
    
    // move monkey
    monkey.pos += monkey.vel * DeltaTime;
    monkey.pos -= (monkey.pos >  WorldSize) * WorldSize * 2; // wrap on world border
    monkey.pos += (monkey.pos < -WorldSize) * WorldSize * 2; // wrap on world border
    
    // store updated monkey
    AllMonkeys[globalID.x] = monkey;

    // check visibility
    bool isVisible = dot(monkey.pos, monkey.pos) < CullRadius * CullRadius;
    if (isVisible)
    {
        // add monkey to visible monkey buffer
        VisibleMonkeys.Append(monkey);
    }
}

//==============================================================================
// Vertex shader
//==============================================================================
float4x4 ViewProjection;

StructuredBuffer<Monkey> VisibleMonkeysReadonly;

struct VertexIn
{
    float3 Position : POSITION0;
    float3 Normal : NORMAL0;
    uint InstanceID : SV_InstanceID;
};

struct VertexOut
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL0;
};

VertexOut VS(in VertexIn input)
{
    VertexOut output = (VertexOut)0;
    
    Monkey monkey = VisibleMonkeysReadonly[input.InstanceID];
    
    float size = 2;
    float3 pos = monkey.pos + input.Position * size;
    
    output.Position = mul(float4(pos, 1), ViewProjection);
    output.Normal = input.Normal;
	
    return output;
}

//==============================================================================
// Pixel shader 
//==============================================================================
float4 PS(VertexOut input) : SV_TARGET
{
    return float4(input.Normal, 1);
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