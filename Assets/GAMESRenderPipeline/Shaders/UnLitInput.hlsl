#ifndef GRP_UNLITINPUT_INCLUDED
#define GRP_UNLITINPUT_INCLUDED

#include "Assets/GAMESRenderPipeline/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerMaterial)
    half4 _BaseColor;
    float4 _BaseMap_ST;
    float _Cutoff;
CBUFFER_END

TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);

#endif