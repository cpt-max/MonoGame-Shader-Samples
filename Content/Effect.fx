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
};

VertexOut VS(in VertexIn input)
{
	VertexOut output;
	
    output.Position = float4(input.Position, 1);
	
	return output;
}

//==============================================================================
// Pixel shader 
//==============================================================================
float4 PS(VertexOut input) : SV_TARGET
{
	return float4(1,1,1,1);
}

//==============================================================================
// Techniques
//==============================================================================
technique Tech0
{
	pass P0
	{
		VertexShader = compile vs_4_0 VS();
		PixelShader = compile ps_4_0 PS();
	}
};