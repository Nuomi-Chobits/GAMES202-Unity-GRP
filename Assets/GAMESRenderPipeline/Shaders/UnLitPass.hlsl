#ifndef GRP_UNLITPASS_INCLUDE
#define GRP_UNLITPASS_INCLUDE

#include "Assets/GAMESRenderPipeline/ShaderLibrary/Common.hlsl"

struct Attributes
{
   float4 positionOS : POSITION;
   float2 uv : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;      
};

Varyings UnLitVertex(Attributes input)
{
    Varyings output = (Varyings) 0;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    return output;
}

half4 UnLitFragment(Varyings input): SV_TARGET
{
    half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
    color.xyz = color.xyz * _BaseColor.xyz;
    color.a = color.a * _BaseColor.a;
    #if defined(_CLIPPING)
        clip(color.a - _Cutoff);
    #endif
    return color;
}

#endif