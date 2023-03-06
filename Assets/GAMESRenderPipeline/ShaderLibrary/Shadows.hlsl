#ifndef GRP_SHADOWS_INCLUDE
#define GRP_SHADOWS_INCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

#if defined(_DIRECTIONAL_PCF3)
	#define DIRECTIONAL_FILTER_SAMPLES 4
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_DIRECTIONAL_PCF5)
	#define DIRECTIONAL_FILTER_SAMPLES 9
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_DIRECTIONAL_PCF7)
	#define DIRECTIONAL_FILTER_SAMPLES 16
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4

//https://docs.unity3d.com/Manual/SL-SamplerStates.html
//sampler_linear_clamp_compare 
//“Point”, “Linear” or “Trilinear” (required) set up texture filtering mode
//“Clamp”, “Repeat”, “Mirror” or “MirrorOnce” (required) set up texture wrap mode.
// Wrap modes can be specified per - axis(UVW), e.g.“ClampU_RepeatV”.
//“Compare” (optional) set up sampler for depth comparison; use with HLSL SamplerComparisonState type and SampleCmp / SampleCmpLevelZero functions.
//sampler_linear_clamp_compare sampler_point_clamp_compare
#define SHADOW_SAMPLER sampler_linear_clamp_compare

//D3D11.hlsl
//#define TEXTURE2D_SHADOW(textureName)         TEXTURE2D(textureName)
//#define SAMPLER_CMP(samplerName)              SamplerComparisonState samplerName

TEXTURE2D_SHADOW(_DirectionalShadowAtlas); SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_GRPShadows)
	float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
	float4 _ShadowAtlasSize;
CBUFFER_END


struct DirectionalShadowData
{
	float strength;
	int  tileIndex;
};

float SampleDirectionalShadowAtlas(float3 positionSTS) {
	return SAMPLE_TEXTURE2D_SHADOW(
		_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS
	);
}

float FilterDirectionalShadow(float3 positionSTS){
	#if defined(DIRECTIONAL_FILTER_SETUP)
		float weights[DIRECTIONAL_FILTER_SAMPLES];
		float2 positions[DIRECTIONAL_FILTER_SAMPLES];
		float4 size = _ShadowAtlasSize.yyxx;
		DIRECTIONAL_FILTER_SETUP(size,positionSTS.xy,weights,positions);
		float shadow = 0;
		for (int i = 0; i < DIRECTIONAL_FILTER_SAMPLES; i++) {
			shadow += weights[i] * SampleDirectionalShadowAtlas(float3(positions[i].xy, positionSTS.z));
		}
		return shadow;
	#else
		return SampleDirectionalShadowAtlas(positionSTS);
	#endif
}

float GetDirectionalShadowAttenuation(DirectionalShadowData data, float3 positionWS) {
	if (data.strength <= 0.0) {
		return 1.0;
	}

	float3 positionSTS = mul(
		_DirectionalShadowMatrices[data.tileIndex],
		float4(positionWS, 1.0)
	).xyz;
	float shadow = FilterDirectionalShadow(positionSTS);
	return lerp(1.0, shadow, data.strength);
}

#endif