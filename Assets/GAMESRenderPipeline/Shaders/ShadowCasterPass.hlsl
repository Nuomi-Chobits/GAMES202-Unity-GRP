#ifndef GRP_SHADOWCASTERPASS_INCLUDE
#define GRP_SHADOWCASTERPASS_INCLUDE

#include "Assets/GAMESRenderPipeline/ShaderLibrary/Common.hlsl"

struct Attributes {
	float4 positionOS    : POSITION;
	float2 texcoord      : TEXCOORD0;
};

struct Varyings {
	float4 positionCS     : SV_POSITION;
	float2 uv             : TEXCOORD0;
};

Varyings ShadowCasterVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uv = input.texcoord;
    return output;
}

half4 ShadowCasterFragment(Varyings input) : SV_Target
{
    half4 final_color = (half4) 0;

    return final_color;
}
#endif