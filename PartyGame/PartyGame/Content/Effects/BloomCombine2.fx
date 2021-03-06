﻿#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
};

sampler s0; // from SpriteBatch

texture _baseMap;
sampler _baseMapSampler = sampler_state { Texture = <_baseMap>; };

float _bloomIntensity;
float _baseIntensity;

float _bloomSaturation;
float _baseSaturation;


// helper for modifying the saturation of a color.
float4 adjustSaturation( float4 color, float saturation )
{
    // The constants 0.3, 0.59, and 0.11 are chosen because the human eye is more sensitive to green light, and less to blue.
    float grey = dot( color.rgb, float3( 0.3, 0.59, 0.11 ) );

    return lerp( grey, color, saturation );
}


float4 PixelShaderFunction( float2 texCoord : TEXCOORD0 ) : COLOR0
{
    // Look up the bloom and original base image colors.
    float4 bloom = tex2D( s0, texCoord );
    float4 base = tex2D( _baseMapSampler, texCoord );
    
    // Adjust color saturation and intensity.
    bloom = adjustSaturation( bloom, _bloomSaturation ) * _bloomIntensity;
    base = adjustSaturation( base, _baseSaturation ) * _baseIntensity;
    
    // Darken down the base image in areas where there is a lot of bloom, to prevent things looking excessively burned-out.
    base *= ( 1 - saturate( bloom ) );
    
    // Combine the two images.
    return base + bloom;
}

technique BasicColorDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
};