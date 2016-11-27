float alpha = 0.0f;
int useGrayScale = 0;
sampler colorTexture : register(s0);

float4 PixelShaderFunction(float4 drawColor : COLOR0, float2 texCord : TEXCOORD0) : COLOR0
{
	float4 OUT = (float4)0;
	if (useGrayScale == 1)
	{
		float4 color = tex2D(colorTexture, texCord);
		float3 colrgb = color.rgb;
		float greycolor = dot(colrgb, float3(0.3, 0.59, 0.11));

		OUT.rgb = greycolor;
		OUT.a = color.a;
	}
	else
	{
		OUT = tex2D(colorTexture, texCord);
	}

    return OUT * drawColor * alpha;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
