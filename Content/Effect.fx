//==============================================================================
// Compute Shader
//==============================================================================
#define GroupSizeXYZ 4

RWTexture3D<unorm float4> Texture;
int TextureSize;

[numthreads(GroupSizeXYZ, GroupSizeXYZ, GroupSizeXYZ)]
void CS(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint  localIndex : SV_GroupIndex, uint3 pixelID : SV_DispatchThreadID)
{ 
    float4 pixel = Texture[pixelID];
    
    // dead pixels do nothing
    if (!any(pixel.xyz))
        return;   
    
    // pixels with alpha == 1 will move through the texture volume
    if (pixel.a == 1)
    {
        int3 vel = round(pixel.xyz * 4) - 2; // the pixel color represents a velocity
        int3 targetID = pixelID + vel; // destination coordinates for this pixel

        // when a pixel reaches the edge of the cube, we change it's color, to make it turn around
        bool3 boundaryReached = targetID < 0 || targetID >= TextureSize;
        
        if (any(boundaryReached))
        {
            float3 reverseColor = boundaryReached ? (1 - pixel.xyz) : pixel.xyz;
            Texture[pixelID] = float4(reverseColor, 1);
            return;
        }
            
        // move current pixel to new destination
        Texture[targetID] = pixel; 
    }
   
    // fade pixel alpha out to create a streak effect
    Texture[pixelID] = pixel.a > 0.1 ?  
        float4(pixel.rgb, pixel.a * 0.9) : 
        float4(0,0,0,1); // dead
}

//==============================================================================
// Vertex shader
//==============================================================================
float4x4 WorldViewProjection;

struct VertexIn
{
    float3 Position : POSITION0;
};

struct VertexOut
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD0;
};

VertexOut VS(in VertexIn input)
{
    VertexOut output;
    
    output.Position = mul(float4(input.Position, 1), WorldViewProjection);
    output.TexCoord = input.Position.xyz + 0.5;
	
    return output;
}

//==============================================================================
// Pixel shader 
//==============================================================================
Texture3D TextureReadOnly;
SamplerState TextureSampler;

float4 PS(VertexOut input) : SV_TARGET
{
    float4 texCol = TextureReadOnly.Sample(TextureSampler, input.TexCoord);
    
    // brighten the corners of the cube
    float3 pos = input.TexCoord * 2 - 1;
    float cornerBright = pow(dot(pos, pos), 5) * 0.0015;
    
    return texCol + cornerBright;
}


//==============================================================================
// Techniques
//==============================================================================
technique Tech0
{
    pass Pass0
    {
        ComputeShader = compile cs_5_0 CS();
        VertexShader = compile vs_4_0 VS();
        PixelShader = compile ps_4_0 PS();
    }
}