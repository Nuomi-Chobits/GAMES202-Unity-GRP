#ifndef GRP_SIMPLELITINPUT_INCLUDED
#define GRP_SIMPLELITINPUT_INCLUDED

#include "Assets/GAMESRenderPipeline/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    float _SpecularStrength;
CBUFFER_END

TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);

#endif 