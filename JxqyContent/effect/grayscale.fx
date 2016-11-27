Texture coloredTexture;

sampler coloredTextureSampler = sampler_state
{
	texture = <coloredTexture>;
};

float4 GreyscalePixelShaderFunction(float2 textureCoordinate : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(coloredTextureSampler, textureCoordinate);
	float3 colrgb = color.rgb;
	float greycolor = dot(colrgb, float3(0.3, 0.59, 0.11));

	return float4(greycolor, greycolor, greycolor, color.a);
}

technique Grayscale
{
	pass GreyscalePass
	{
		PixelShader = compile ps_2_0 GreyscalePixelShaderFunction();
	}
}