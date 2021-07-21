float HueFromRGB(float3 rgb)
{
    float minimum = min(rgb.r, min(rgb.g, rgb.b));
    float maximum = max(rgb.r, max(rgb.g, rgb.b));  
    float delta = maximum - minimum;
    
    float hue = delta == 0 ? 0 :
        (rgb.r == maximum) ?     (rgb.g - rgb.b) / delta :
        (rgb.g == maximum) ? 2 + (rgb.b - rgb.r) / delta :
                             4 + (rgb.r - rgb.g) / delta;
    
    hue *= 60;
    return hue >= 0 ? hue : hue + 360;
}

//================================================================================================
// Compute Shader
//================================================================================================
#define GroupSizeXY 8

Texture2D<float4> Input;
RWTexture2D<float4> Output;

int Width;
int OffsetX;

[numthreads(GroupSizeXY, GroupSizeXY, 1)]
void CS(uint3 localID : SV_GroupThreadID, uint3 groupID : SV_GroupID,
        uint  localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    // read two adjacent pixels from the texture
    // swap them if they are not already ordered by acending hue
    uint2 idL = uint2(globalID.x * 2 + OffsetX, globalID.y);
    uint2 idR = uint2(idL.x + 1, idL.y);

    float4 colL = Input[idL];
    float4 colR = Input[idR];
    
    float hueL = HueFromRGB(colL.rgb);
    float hueR = HueFromRGB(colR.rgb);
      
    bool exceedBorder = idR.x >= (uint)Width;
    bool swap = hueL > hueR && !exceedBorder;

    Output[idL] = swap ? colR : colL;
    Output[idR] = swap ? colL : colR;
}

//================================================================================================
// Techniques
//================================================================================================
technique Tech0
{
    pass Pass0
    {
        ComputeShader = compile cs_5_0 CS();
    }
}