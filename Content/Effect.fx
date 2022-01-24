
float4x4 WorldViewProjection;
Texture2D Texture;
float TextureDisplacement;

SamplerState TextureSampler;

//==============================================================================
// Vertex shader
//==============================================================================
struct VertexIn
{
	float3 Position : POSITION0;
};

struct VertexOut
{
	float4 Position : SV_POSITION;
	float4 LocalPosition : TEXCOORD0;
};

VertexOut VS(in VertexIn input)
{
	VertexOut output;

	output.LocalPosition = float4(input.Position.xyz, 1);
	output.Position = mul(output.LocalPosition, WorldViewProjection);
	
	return output;
}

//==============================================================================
// Pixel shader 
//==============================================================================
float4 PS(VertexOut input) : SV_TARGET
{
	return float4(
		input.LocalPosition.x * 4,
		input.LocalPosition.y * 2,
		input.LocalPosition.x * -4,
		1);
}

//==============================================================================
// Hull shader 
//==============================================================================
float Bend;
float Tesselation;

struct HullOut
{
	float4 Position : BEZIERPOS;
};

struct PatchConstantOut
{
	float Edges[3] : SV_TessFactor;
	float Inside   : SV_InsideTessFactor;
};

PatchConstantOut PatchConstantFunc(
	InputPatch<VertexOut, 3> ip,
	uint patchID : SV_PrimitiveID)
{
	PatchConstantOut output;

	output.Edges[0] = Tesselation;
	output.Edges[1] = Tesselation;
	output.Edges[2] = Tesselation;

	output.Inside = Tesselation;

	return output;
}

[domain("tri")]  // tri  quad  isoline
[partitioning("fractional_even")]  // fractional_even  fractional_odd  pow2
[outputtopology("triangle_cw")]  // triangle_cw  triangle_ccw  line
[outputcontrolpoints(3)]
[patchconstantfunc("PatchConstantFunc")]
[maxtessfactor(30.0)]
HullOut HS(InputPatch<VertexOut, 3> ip, uint i : SV_OutputControlPointID, uint patchID : SV_PrimitiveID)
{
	HullOut output;

	output.Position = ip[i].LocalPosition;

	return output;
}

//==============================================================================
// Domain shader
//==============================================================================
[domain("tri")]
VertexOut DS(const OutputPatch<HullOut, 3> patch, float3 barycentric : SV_DomainLocation, PatchConstantOut patchConst)
{
	VertexOut output;

	float4 pos =
		patch[0].Position * barycentric.x +
		patch[1].Position * barycentric.y +
		patch[2].Position * barycentric.z;

	float dist = length(pos.xyz);
	pos.z = -Bend * dist * dist;
	pos.z += TextureDisplacement * Texture.SampleLevel(TextureSampler, pos.xy * 2, 0).x;

	output.Position = mul(pos, WorldViewProjection);
	output.LocalPosition = pos;

	return output;
}

//==============================================================================
// Geometry shader 
//==============================================================================
float GeometryGeneration;

[maxvertexcount(100)]
void GS(triangle in VertexOut vertex[3], inout TriangleStream<VertexOut> triStream)
{
	float3 v0 = vertex[0].LocalPosition.xyz;
	float3 v1 = vertex[1].LocalPosition.xyz;
	float3 v2 = vertex[2].LocalPosition.xyz;

	float size = 1 / GeometryGeneration;

	for (float s = 0; s < 3; s += size)
	{
		float t = frac(s);
		float3 origin = s < 1 ? lerp(v0, v1, t) :
						s < 2 ? lerp(v1, v2, t) :
								lerp(v2, v0, t);

		origin.z += TextureDisplacement * Texture.SampleLevel(TextureSampler, origin.xy, 0).x;

		vertex[0].Position = mul(float4(origin + v0 * size, 1), WorldViewProjection);
		vertex[1].Position = mul(float4(origin + v1 * size, 1), WorldViewProjection);
		vertex[2].Position = mul(float4(origin + v2 * size, 1), WorldViewProjection);
		
		triStream.Append(vertex[0]);
		triStream.Append(vertex[1]);
		triStream.Append(vertex[2]);

		triStream.RestartStrip();
	}
}

//==============================================================================
// Techniques
//==============================================================================
technique Basic_Vertex_Pixel
{
	pass P0
	{
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
	}
};

technique Hull_Domain
{
	pass P0
	{
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
		HullShader = compile hs_5_0 HS();
		DomainShader = compile ds_5_0 DS();
	}
};

technique Geometry
{
	pass P0
	{
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
		GeometryShader = compile gs_4_0 GS();
	}
};

technique Hull_Domain_Geometry
{
	pass P0
	{
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
		HullShader = compile hs_5_0 HS();
		DomainShader = compile ds_5_0 DS();
		GeometryShader = compile gs_4_0 GS();
	}
};

