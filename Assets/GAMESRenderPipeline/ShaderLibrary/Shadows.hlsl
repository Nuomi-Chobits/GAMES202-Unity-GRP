#ifndef GRP_SHADOWS_INCLUDE
#define GRP_SHADOWS_INCLUDE

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4

//https://docs.unity3d.com/Manual/SL-SamplerStates.html
//sampler_linear_clamp_compare 
//¡°Point¡±, ¡°Linear¡± or ¡°Trilinear¡± (required) set up texture filtering mode
//¡°Clamp¡±, ¡°Repeat¡±, ¡°Mirror¡± or ¡°MirrorOnce¡± (required) set up texture wrap mode.
// Wrap modes can be specified per - axis(UVW), e.g.¡°ClampU_RepeatV¡±.
//¡°Compare¡± (optional) set up sampler for depth comparison; use with HLSL SamplerComparisonState type and SampleCmp / SampleCmpLevelZero functions.
#define SHADOW_SAMPLER sampler_point_clamp_compare

//D3D11.hlsl
//#define TEXTURE2D_SHADOW(textureName)         TEXTURE2D(textureName)
//#define SAMPLER_CMP(samplerName)              SamplerComparisonState samplerName

TEXTURE2D_SHADOW(_DirectionalShadowAtlas); SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_GRPShadows)
	float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
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

float GetDirectionalShadowAttenuation(DirectionalShadowData data, float3 positionWS) {
	if (data.strength <= 0.0) {
		return 1.0;
	}

	float3 positionSTS = mul(
		_DirectionalShadowMatrices[data.tileIndex],
		float4(positionWS, 1.0)
	).xyz;
	float shadow = SampleDirectionalShadowAtlas(positionSTS);
	return lerp(1.0, shadow, data.strength);
}

#endif