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

int StartX;
int Width;

[numthreads(GroupSizeXY, GroupSizeXY, 1)]
void CS(uint3 localID : SV_GroupThreadID, uint3 grouphID : SV_GroupID,
        uint  localIndex : SV_GroupIndex, uint3 globalID : SV_DispatchThreadID)
{
    uint2 idL = uint2(globalID.x * 2 + StartX, globalID.y);
    uint2 idR = uint2(idL.x + 1, idL.y);

    float3 colL = Input[idL].xyz;
    float3 colR = Input[idR].xyz;
    
    float hueL = HueFromRGB(colL);
    float hueR = HueFromRGB(colR);
      
    bool exceedBorder = idR.x >= (uint)Width;
    bool swap = hueL > hueR && !exceedBorder;

    Output[idL] = float4(swap ? colR : colL, 1);
    Output[idR] = float4(swap ? colL : colR, 1);
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