
float4x4 WorldViewProjection;
float Tesselation;
float Radius;

struct VS_IN
{
    float3 pos : POSITION;
    float3 norm : NORMAL;
};

struct VS_OUT
{
    float3 worldPos : POSITION;
    float3 norm : NORMAL;
};

struct DS_OUT
{
    float4 pos : SV_POSITION;
    float3 norm : NORMAL;
};

//================================================================================================
// Vertex Shader
//================================================================================================
VS_OUT VS(VS_IN input)
{
    VS_OUT output;

    output.worldPos = input.pos;
    output.norm = input.norm;
    //output.pos = mul(float4(output.worldPos, 1), WorldViewProjection);

    return output;
}

//================================================================================================
// Hull Shader
//================================================================================================
struct PatchConstantOut
{
	float edges[4] : SV_TessFactor;
	float inside[2]: SV_InsideTessFactor;
    float3 patchNorm : NORMAL;
    float3 dir[4] : TEXCOORD0;
    float3 midNorm[4] : NORMAL1;
};

PatchConstantOut PatchConstantFunc(InputPatch<VS_OUT, 4> cp, uint patchID : SV_PrimitiveID)
{
	PatchConstantOut output;

	output.edges[0] = Tesselation;
	output.edges[1] = Tesselation;
	output.edges[2] = Tesselation;
	output.edges[3] = Tesselation;
			  
	output.inside[0] = Tesselation;
	output.inside[1] = Tesselation;
    
    output.dir[0] = normalize(cp[1].worldPos - cp[0].worldPos);
    output.dir[1] = normalize(cp[2].worldPos - cp[1].worldPos);
    output.dir[2] = normalize(cp[3].worldPos - cp[2].worldPos);
    output.dir[3] = normalize(cp[0].worldPos - cp[3].worldPos);
    
    output.midNorm[0] = normalize(cp[0].norm + cp[1].norm);
    output.midNorm[1] = normalize(cp[1].norm + cp[2].norm);
    output.midNorm[2] = normalize(cp[2].norm + cp[3].norm);
    output.midNorm[3] = normalize(cp[3].norm + cp[0].norm);
    
    output.patchNorm = normalize(cross(
        cp[0].worldPos - cp[2].worldPos,
        cp[3].worldPos - cp[1].worldPos));

	return output;
}

[domain("quad")]  // tri  quad  isoline
[partitioning("fractional_odd")]  // fractional_even  fractional_odd  pow2
[outputtopology("triangle_cw")]  // triangle_cw  triangle_ccw  line
[outputcontrolpoints(4)]
[patchconstantfunc("PatchConstantFunc")]
[maxtessfactor(30.0)]
VS_OUT HS(InputPatch<VS_OUT, 4> cp, uint i : SV_OutputControlPointID, uint patchID : SV_PrimitiveID)
{
    VS_OUT output;

    output.worldPos = cp[i].worldPos;
    output.norm = cp[i].norm;
    //output.pos = mul(float4(output.worldPos, 1), WorldViewProjection);
 
	return output;
}

//================================================================================================
// Domain Shader
//================================================================================================
[domain("quad")]
DS_OUT DS(const OutputPatch<VS_OUT, 4> cp, float2 uv : SV_DomainLocation, PatchConstantOut patchConst)
{
    DS_OUT output;
    
	float2 uvSigned = uv * 2 - 1; 
    float2 uvSign = sign(uvSigned);
    float2 uvAbs = abs(uvSigned);
    float2 relCornerDist = 1 - uvAbs;
    float2 edgeDist = relCornerDist * uvSign * Radius;

    int3 inds = uvSign.x > 0 ?
                uvSign.y > 0 ? int3(1, -1,  2) : int3(2, 3,  2) :
                uvSign.y > 0 ? int3(0, -1, -4) : int3(3, 3, -4);
    
    int cornerInd = inds.x;
    int2 adjacentEdge = abs(inds.yz) - 1;
    int2 adjacentEdgeDir = sign(inds.yz);
    
    float3 cornerPos = cp[cornerInd].worldPos;
    float3 dirX = patchConst.dir[adjacentEdge.x] * adjacentEdgeDir.x;
    float3 dirY = patchConst.dir[adjacentEdge.y] * adjacentEdgeDir.y;
    
    float3 worldPos = cornerPos + dirX * edgeDist.x + dirY * edgeDist.y;
    
    // calculate normal
    float3 cornerNorm = cp[cornerInd].norm;
    float3 xNorm = lerp(cornerNorm, patchConst.midNorm[adjacentEdge.x], relCornerDist.x);
    float3 yNorm = lerp(cornerNorm, patchConst.midNorm[adjacentEdge.y], relCornerDist.y);
    float3 norm = lerp(xNorm + yNorm, patchConst.patchNorm, relCornerDist.x * relCornerDist.y);

    worldPos -= cornerNorm * dot(1 - relCornerDist, 1 - relCornerDist) * Radius * 0.2; //0.4142135;

    output.pos = mul(float4(worldPos, 1), WorldViewProjection);
    output.norm = normalize(norm); 

    return output;
}

//================================================================================================
// Pixel Shader
//================================================================================================
float4 PS(DS_OUT input) : SV_TARGET
{
    float3 col = input.norm; // float3((input.norm.xy + 1) / 2, input.norm.z);
    return float4(col, 1);
}

//================================================================================================
// Techniques
//================================================================================================
technique Tech0
{
    pass Pass0
    {
        ZEnable = true;
        ZWriteEnable = true;
        CullMode = none;
        FillMode = wireframe;
		
        BlendOp = Add;
        SrcBlend = One;
        DestBlend = Zero;
	  
        VertexShader = compile vs_4_0 VS();
		HullShader   = compile hs_5_0 HS();
		DomainShader = compile ds_5_0 DS();
        PixelShader  = compile ps_4_0 PS();
    }
}
