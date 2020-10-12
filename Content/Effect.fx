
float4x4 WorldViewProjection;
float Tesselation;
float Radius;

struct VS_INPUT
{
    float4 pos : POSITION;
    float3 norm : NORMAL;
};

struct VS_OUTPUT
{
    float4 worldPos : POSITION;
    float3 norm : NORMAL;
};

struct DS_OUTPUT
{
    float4 pos : SV_POSITION;
    float3 norm : NORMAL;
};

//================================================================================================
// Vertex Shader
//================================================================================================
VS_OUTPUT VS(VS_INPUT input)
{
    VS_OUTPUT output;

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
};

PatchConstantOut PatchConstantFunc(
	InputPatch<VS_OUTPUT, 4> cp,
	uint patchID : SV_PrimitiveID)
{
	PatchConstantOut output;

	output.edges[0] = Tesselation;
	output.edges[1] = Tesselation;
	output.edges[2] = Tesselation;
	output.edges[3] = Tesselation;
			  
	output.inside[0] = Tesselation;
	output.inside[1] = Tesselation;
    
    output.patchNorm = normalize(cross(cp[0].worldPos.xyz - cp[2].worldPos.xyz,
		                     cp[3].worldPos.xyz - cp[1].worldPos.xyz));

	return output;
}

[domain("quad")]  // tri  quad  isoline
[partitioning("fractional_odd")]  // fractional_even  fractional_odd  pow2
[outputtopology("triangle_cw")]  // triangle_cw  triangle_ccw  line
[outputcontrolpoints(4)]
[patchconstantfunc("PatchConstantFunc")]
[maxtessfactor(30.0)]
VS_OUTPUT HS(InputPatch<VS_OUTPUT, 4> cp, uint i : SV_OutputControlPointID, uint patchID : SV_PrimitiveID)
{
    VS_OUTPUT output;

    output.worldPos = cp[i].worldPos;
    output.norm = cp[i].norm;
    //output.pos = mul(float4(output.worldPos, 1), WorldViewProjection);
 
	return output;
}

//================================================================================================
// Domain Shader
//================================================================================================
float3 interpol(float3 corner0, float3 corner1, float3 corner2, float3 corner3, float2 edgeDist, float2 uvSign)
{
    float3 leftEdge  = normalize(corner3 - corner0);
    float3 rightEdge = normalize(corner2 - corner1);
    
    float3 startLeft  = uvSign.x > 0 ? corner0 : corner3;
    float3 startRight = uvSign.x > 0 ? corner1 : corner2;
    
    float3 midLeft  = startLeft  + leftEdge  * edgeDist.x;
    float3 midRight = startRight + rightEdge * edgeDist.x;
    
    float3 startUp = uvSign.y > 0 ? midLeft : midRight;
    float3 upEdge = normalize(midRight - midLeft);
    
    return startUp + upEdge * edgeDist.y;
}

[domain("quad")]
DS_OUTPUT DS(const OutputPatch<VS_OUTPUT, 4> cp, float2 uv : SV_DomainLocation, PatchConstantOut patchConst)
{
    DS_OUTPUT output;
    
    //float step = 1 / Tesselation;
    //float nr = (Tesselation - 1) / 2;
      
	float2 uvSigned = uv * 2 - 1; 
    float2 uvSign = sign(uvSigned);
    float2 uvAbs = abs(uvSigned);
    //float2 uvAbsPow = pow(uvAbs, Distribution);
    float2 edgeDist = (1 - uvAbs) * uvSign * Radius;
    
    //float2 uvPow = (uvSign * uvAbsPow + 1) / 2;
    
    //float2 uvStart = 1 / Tesselation;
    //float2 edgeClose = uvAbs - uvStart;
    
    // calculate position
    /*
    float3 leftEdge  = normalize(cp[3].worldPos - cp[0].worldPos);
    float3 rightEdge = normalize(cp[2].worldPos - cp[1].worldPos);
    
    float3 startLeft  = uvSign.x > 0 ? cp[0].worldPos : cp[3].worldPos;
    float3 startRight = uvSign.x > 0 ? cp[1].worldPos : cp[2].worldPos;
    
    float3 midLeft  = startLeft  + leftEdge  * edgeDist.x;
    float3 midRight = startRight + rightEdge * edgeDist.x;
    
    float3 startUp = uvSign.y > 0 ? midLeft : midRight;
    float3 upEdge = normalize(midRight - midLeft);
    
    float3 worldPos = startUp + upEdge * edgeDist.y;
    */
    // calculate normal

 
    float3 worldPos = interpol(cp[0].worldPos.xyz, cp[1].worldPos.xyz, cp[2].worldPos.xyz, cp[3].worldPos.xyz, edgeDist, uvSign);
    float3 norm     = interpol(cp[0].norm,     cp[1].norm,     cp[2].norm,     cp[3].norm,     edgeDist*8, uvSign);
    
    float t = abs(edgeDist.x * edgeDist.y) * 100;
    norm = norm * (1 - t) + patchConst.patchNorm * t;
    norm = normalize(norm);
    
    output.pos = mul(float4(worldPos, 1), WorldViewProjection);
    output.norm = abs(norm - patchConst.patchNorm); // ; // patchNorm; //  patch[0].worldPos.xyz;

    return output;
}

//================================================================================================
// Pixel Shader
//================================================================================================
float4 PS(DS_OUTPUT input) : SV_TARGET
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
