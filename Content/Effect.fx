
float3 CamPos;

float4x4 World;
float3x3 WorldRot;
float4x4 ViewProjection;

float Tesselation;
float Radius;
float Test;

TextureCube CubeMap;
SamplerState CubeSampler;

bool EnableTexture;
Texture2D Texture;
SamplerState TextureSampler
{
    AddressU = mirror;
    AddressV = mirror;
};

//================================================================================================
// Vertex Shader
//================================================================================================
struct VS_IN
{
    float3 pos : POSITION;
    float3 norm : NORMAL;
    float2 texCoord : TEXCOORD;
};

struct VS_OUT
{
    float3 pos : POSITION;
    float3 norm : NORMAL;
    float2 texCoord : TEXCOORD;
};

VS_OUT VS(VS_IN input)
{
    VS_OUT output;

    // pass the corner points through to the hull shader unaltered
    output.pos = input.pos;
    output.norm = input.norm;
    output.texCoord = input.texCoord;

    return output;
}

//================================================================================================
// Hull Shader
//================================================================================================
struct HS_OUT
{
    float3 pos : POSITION;
    float3 norm : NORMAL;
    float3 sphereCenter : TEXCOORD0;
    float3 roundingEdge[2] : TEXCOORD1;
    float2 texCoord : TEXCOORD3;
    float2 roundingTexCoord[2] : TEXCOORD4;
    
};

struct PatchConstOut
{
	float tessEdge[4] : SV_TessFactor;
	float tessInside[2]: SV_InsideTessFactor;
};

PatchConstOut PatchConstantFunc(InputPatch<VS_OUT, 4> cp, uint patchID : SV_PrimitiveID)
{
	PatchConstOut output;

    output.tessEdge[0] = Tesselation;
    output.tessEdge[1] = Tesselation;
    output.tessEdge[2] = Tesselation;
    output.tessEdge[3] = Tesselation;
    
    output.tessInside[0] = Tesselation;
    output.tessInside[1] = Tesselation;
    
	return output;
}

[domain("quad")]  // tri  quad  isoline
[partitioning("fractional_odd")] // fractional_even  fractional_odd  pow2
[outputtopology("triangle_cw")]  // triangle_cw  triangle_ccw  line
[outputcontrolpoints(4)]
[patchconstantfunc("PatchConstantFunc")]
[maxtessfactor(30.0)]
HS_OUT HS(InputPatch<VS_OUT, 4> cp, uint i : SV_OutputControlPointID, uint patchID : SV_PrimitiveID)
{
    HS_OUT output;

    float3 pos = cp[i].pos;
    float3 norm = cp[i].norm;
    
    // adjacent edges
    int indNext = (i + 1) % 4;
    int indPrev = (i - 1) % 4;
    
    float3 posNext = cp[indNext].pos;
    float3 posPrev = cp[indPrev].pos;
    
    float3 dirToNext = posNext - pos;
    float3 dirToPrev = posPrev - pos;
    
    float lengthNext = length(dirToNext);
    float lengthPrev = length(dirToPrev);
    
    dirToNext /= lengthNext;
    dirToPrev /= lengthPrev;
    
    // rounding sphere
    float3 patchNorm = normalize(cross(dirToPrev, dirToNext));  
    float sphereShift = Radius / dot(norm, patchNorm);
    float3 sphereCenter = pos - sphereShift * norm;
    
    float3 sphereTouchPatch = sphereCenter + patchNorm * Radius;
    float3 posToTouch = sphereTouchPatch - pos;
    
    float roundingLengthNext = dot(posToTouch, dirToNext);
    float roundingLengthPrev = dot(posToTouch, dirToPrev);
    
    // tex coords   
    float2 texCoord = cp[i].texCoord;
    float2 texChangeNext = cp[indNext].texCoord - texCoord;
    float2 texChangePrev = cp[indPrev].texCoord - texCoord;
    
    float roundingEdgeFractionNext = roundingLengthNext / lengthNext;
    float roundingEdgeFractionPrev = roundingLengthPrev / lengthPrev;
    
    float2 roundingTexChangeNext = texChangeNext * roundingEdgeFractionNext;
    float2 roundingTexChangePrev = texChangePrev * roundingEdgeFractionPrev;

    // output
    output.pos = pos;
    output.norm = norm;
    output.sphereCenter = sphereCenter;
    output.roundingEdge[0] = dirToNext * roundingLengthNext;
    output.roundingEdge[1] = dirToPrev * roundingLengthPrev;
    output.texCoord = cp[i].texCoord;
    output.roundingTexCoord[0] = roundingTexChangeNext;
    output.roundingTexCoord[1] = roundingTexChangePrev;
    
	return output;
}

//================================================================================================
// Domain Shader
//================================================================================================
struct DS_OUT
{
    float4 pos : SV_POSITION;
    float3 worldPos : POSITION;
    float3 norm : NORMAL;
    float3 color : COLOR;
    float2 texCoord : TEXCOORD;
};

[domain("quad")]
DS_OUT DS(const OutputPatch<HS_OUT, 4> cp, float2 uv : SV_DomainLocation, PatchConstOut patchConst)
{
    DS_OUT output;
    
    // calculate distance from the corner 
    float2 uvCornerDist = min(uv, 1 - uv) * 2;
    float2 uvSign = sign(uv - 0.5);

    // determine indices for vector arrays depending on which corner we are in
    int2 inds = uvSign.x > 0 ?
                uvSign.y > 0 ? int2(1, 1) : int2(2, 0) :
                uvSign.y > 0 ? int2(0, 0) : int2(3, 1);
    
    int cornerInd = inds.x;
    int edgeIndX = inds.y;
    int edgeIndY = 1 - inds.y;

    // calculate position & normal
    float3 cornerPos    = cp[cornerInd].pos;
    float3 sphereCenter = cp[cornerInd].sphereCenter;
    
    float3 flatPos = cornerPos + 
        cp[cornerInd].roundingEdge[edgeIndX] * uvCornerDist.x +
        cp[cornerInd].roundingEdge[edgeIndY] * uvCornerDist.y;
    
    float3 norm = normalize(flatPos - sphereCenter);
    float3 pos = sphereCenter + norm * Radius;
    
    // calculate texcoord
    float2 cornerTexCoord = cp[cornerInd].texCoord;

    float2 texCoord = cornerTexCoord +
        cp[cornerInd].roundingTexCoord[edgeIndX] * uvCornerDist.x +
        cp[cornerInd].roundingTexCoord[edgeIndY] * uvCornerDist.y;
    
    // output
    output.worldPos = mul(float4(pos, 1), World).xyz;
    output.pos = mul(float4(output.worldPos, 1), ViewProjection);
    output.norm = mul(norm, WorldRot);
    output.color = norm;
    output.texCoord = texCoord;

    return output;
}

//================================================================================================
// Pixel Shader
//================================================================================================
float4 PS(DS_OUT input) : SV_TARGET
{
    float3 col = input.color;
    
    // multiply texture
    float3 tex = EnableTexture ? Texture.Sample(TextureSampler, input.texCoord).xyz : 1;
    col *= tex;
     
    // add cubemap reflection
    float3 eyeVec = normalize(CamPos - input.worldPos);
    float3 reflectVec = normalize(reflect(eyeVec, input.norm));  
    float reflectStrength = 0.2f + tex.x * 0.8f;
    col += CubeMap.Sample(CubeSampler, reflectVec) * reflectStrength;

    return float4(col, 1);
}

//================================================================================================
// Techniques
//================================================================================================
technique Tech0
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 VS();
		HullShader   = compile hs_5_0 HS();
		DomainShader = compile ds_5_0 DS();
        PixelShader  = compile ps_4_0 PS();
    }
}
