
float4x4 WorldViewProjection;
float Tesselation;
float Radius;
float Test;

struct VS_IN
{
    float3 pos : POSITION;
    float3 norm : NORMAL;
};

struct VS_OUT
{
    float3 pos : POSITION;
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

    // pass the corner points through to the hull shader unaltered
    output.pos = input.pos;
    output.norm = input.norm;

    return output;
}

//================================================================================================
// Hull Shader
//================================================================================================
struct PatchConstantOut
{
	float tessEdge[4] : SV_TessFactor;
	float tessInside[2]: SV_InsideTessFactor;
    float3 edge[4] : TEXCOORD0;
    float3 patchNorm : NORMAL;
    float3 edgeNorm[4] : NORMAL1;
};

PatchConstantOut PatchConstantFunc(InputPatch<VS_OUT, 4> cp, uint patchID : SV_PrimitiveID)
{
	PatchConstantOut output;

    // set tesselation density
    output.tessEdge[0] = Tesselation;
    output.tessEdge[1] = Tesselation;
    output.tessEdge[2] = Tesselation;
    output.tessEdge[3] = Tesselation;
    output.tessInside[0] = Tesselation;
    output.tessInside[1] = Tesselation;
    
    // calculate edge vectors between the corner points
    output.edge[0] = normalize(cp[1].pos - cp[0].pos);
    output.edge[1] = normalize(cp[2].pos - cp[1].pos);
    output.edge[2] = normalize(cp[3].pos - cp[2].pos);
    output.edge[3] = normalize(cp[0].pos - cp[3].pos);
    
    // calculate the normal vectors in the center of the edge
    output.edgeNorm[0] = normalize(cp[0].norm + cp[1].norm);
    output.edgeNorm[1] = normalize(cp[1].norm + cp[2].norm);
    output.edgeNorm[2] = normalize(cp[2].norm + cp[3].norm);
    output.edgeNorm[3] = normalize(cp[3].norm + cp[0].norm);
    
    // calculate the normal at the center of this patch
    output.patchNorm = normalize(cross(
        cp[0].pos - cp[2].pos,
        cp[3].pos - cp[1].pos));

	return output;
}

[domain("quad")]  // tri  quad  isoline
[partitioning("fractional_odd")] // fractional_even  fractional_odd  pow2
[outputtopology("triangle_cw")]  // triangle_cw  triangle_ccw  line
[outputcontrolpoints(4)]
[patchconstantfunc("PatchConstantFunc")]
[maxtessfactor(30.0)]
VS_OUT HS(InputPatch<VS_OUT, 4> cp, uint i : SV_OutputControlPointID, uint patchID : SV_PrimitiveID)
{
    VS_OUT output;

    // pass the corner points through to the tesselator and domain shader unaltered
    output.pos = cp[i].pos;
    output.norm = cp[i].norm;
 
	return output;
}

//================================================================================================
// Domain Shader
//================================================================================================
[domain("quad")]
DS_OUT DS(const OutputPatch<VS_OUT, 4> cp, float2 uv : SV_DomainLocation, PatchConstantOut patchConst)
{
    DS_OUT output;
    
    // calculate distance from the corner 
    float2 uvCornerDist = min(uv, 1 - uv) * 2;
    float2 uvSign = sign(uv - 0.5);
    float2 cornerDist = uvCornerDist * uvSign * Radius;

    // determine indices into vector arrays depending on which corner we are in
    int3 inds = uvSign.x > 0 ?
                uvSign.y > 0 ? int3(1, -1,  2) : int3(2, 3,  2) :
                uvSign.y > 0 ? int3(0, -1, -4) : int3(3, 3, -4);
    
    int cornerInd = inds.x; // index for cp array
    int2 adjacentEdgeInd = abs(inds.yz) - 1; // index for patchConst.edge array
    int2 adjacentEdgeDir = sign(inds.yz); // -1 to reverse the edge direction, otherwise +1
    
    // calculate position
    float3 edgeX = patchConst.edge[adjacentEdgeInd.x] * adjacentEdgeDir.x;
    float3 edgeY = patchConst.edge[adjacentEdgeInd.y] * adjacentEdgeDir.y;
    
    float3 cornerPos = cp[cornerInd].pos;
    float3 pos = cornerPos + edgeX * cornerDist.x + edgeY * cornerDist.y;
    
    // calculate normal
    float2 lerpDist = uvCornerDist * uvCornerDist;
    float3 cornerNorm = cp[cornerInd].norm;
    
    float3 xNormStart = lerp(cornerNorm, patchConst.patchNorm, lerpDist.y);
    float3 yNormStart = lerp(cornerNorm, patchConst.patchNorm, lerpDist.x);
    
    float3 xNormEnd = lerp(patchConst.edgeNorm[adjacentEdgeInd.x], patchConst.patchNorm, lerpDist.y);
    float3 yNormEnd = lerp(patchConst.edgeNorm[adjacentEdgeInd.y], patchConst.patchNorm, lerpDist.x);

    float3 xNorm = lerp(xNormStart, xNormEnd, lerpDist.x);
    float3 yNorm = lerp(yNormStart, yNormEnd, lerpDist.y);
    
    float3 norm = normalize(xNorm * lerpDist.x + yNorm * lerpDist.y);
    
    if (all(lerpDist == 0)) // use the original corner normal when we are exactly at the corner
        norm = cornerNorm;
    
    // shift position inwards in the corners
    float x = Radius - abs(cornerDist.x);
    float y = Radius - abs(cornerDist.y);
    float normShift = sqrt(Radius * Radius + x * x + y * y) - Radius;
    pos -= norm * normShift * Test;

    // output
    output.pos = mul(float4(pos, 1), WorldViewProjection);
    output.norm = norm; 

    return output;
}

//================================================================================================
// Pixel Shader
//================================================================================================
float4 PS(DS_OUT input) : SV_TARGET
{
    float3 col = input.norm;
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
