float MinAlpha = 0.0f;
sampler colorTexture : register(s0);

float4 PixelShaderFunction(float2 texCord : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(colorTexture, texCord);

	clip(color.a > MinAlpha ? 1 : -1);
	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
