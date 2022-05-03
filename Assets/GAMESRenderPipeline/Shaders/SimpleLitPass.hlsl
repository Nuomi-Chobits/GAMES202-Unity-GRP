#ifndef GRP_SIMPLELITPASS_INCLUDED
#define GRP_SIMPLELITPASS_INCLUDED

#include "Assets/GAMESRenderPipeline/ShaderLibrary/Common.hlsl"
#include "Assets/GAMESRenderPipeline/ShaderLibrary/Shadows.hlsl"
#include "Assets/GAMESRenderPipeline/ShaderLibrary/Lighting.hlsl"

struct Attributes
{
    float4 positionOS    : POSITION;
    float2 texcoord      : TEXCOORD0;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
};

struct Varyings
{
    float4 positionCS     : SV_POSITION;
    float2 uv             : TEXCOORD0;
    float3 normal         : TEXCOORD1;       
    float3 positionWS     : TEXCOORD2;  
};

Varyings SimpleLitVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    output.positionCS   = TransformObjectToHClip(input.positionOS.xyz);
    output.positionWS   = TransformObjectToWorld(input.positionOS.xyz);
    output.normal       = TransformObjectToWorldNormal(input.normalOS);
    output.uv           = TRANSFORM_TEX(input.texcoord, _BaseMap);
    return output;
}

half4 SimpleLitFragment(Varyings input): SV_Target
{
    half4 final_color = (half4) 0;

    half4 base_color_gamma = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
    half4 base_color       = pow(abs(base_color_gamma),2.2);
    half3 viewDirWS = GetWorldSpaceViewDir(input.positionWS);

    final_color = GRPFragmentBlinnPhong(base_color,input.normal,viewDirWS, input.positionWS,_SpecularStrength);
    final_color = pow(abs(final_color),1.0/2.2);

    return final_color;
}

#endif 