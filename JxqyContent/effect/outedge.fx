float4 EdgeColor = float4(0.6,0,0,0.6);
Texture coloredTexture;
float AplhaThreshold = 0.784;
float radiusx;
float radiusy;

sampler coloredTextureSampler = sampler_state
{
	texture = <coloredTexture>;
};

float4 OutEdgePixelShaderFunction(float2 textureCoordinate : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(coloredTextureSampler, textureCoordinate);
	if (color.a < AplhaThreshold)
	{
		if (tex2D(coloredTextureSampler, textureCoordinate - float2(radiusx, radiusy)).a > AplhaThreshold ||
			tex2D(coloredTextureSampler, textureCoordinate - float2(-radiusx, radiusy)).a > AplhaThreshold ||
			tex2D(coloredTextureSampler, textureCoordinate - float2(radiusx, -radiusy)).a > AplhaThreshold ||
			tex2D(coloredTextureSampler, textureCoordinate - float2(-radiusx,-radiusy)).a > AplhaThreshold ||
			tex2D(coloredTextureSampler, textureCoordinate - float2(0, radiusy)).a > AplhaThreshold ||
			tex2D(coloredTextureSampler, textureCoordinate - float2(radiusx, 0)).a > AplhaThreshold ||
			tex2D(coloredTextureSampler, textureCoordinate - float2(-radiusx, 0)).a > AplhaThreshold ||
			tex2D(coloredTextureSampler, textureCoordinate - float2(0, -radiusy)).a > AplhaThreshold)
		{
			return EdgeColor;
		}
	}
	return float4(0,0,0,0);
}

technique OutEdge
{
	pass OutEdgePass
	{
		PixelShader = compile ps_2_0 OutEdgePixelShaderFunction();
	}
}