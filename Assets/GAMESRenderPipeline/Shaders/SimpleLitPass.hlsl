#ifndef GRP_SIMPLELITPASS_INCLUDED
#define GRP_SIMPLELITPASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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
    float3 viewDir        : TEXCOORD2;  
};

Varyings SimpleLitVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS,input.tangentOS);

    half3 viewDirWS     = GetWorldSpaceViewDir(vertexInput.positionWS);
    output.positionCS   = vertexInput.positionCS;
    output.normal       = normalInput.normalWS;
    output.viewDir      = viewDirWS;
    output.uv           = input.texcoord;
    return output;
}

half4 SimpleLitFragment(Varyings input): SV_Target
{
    half4 base_color_gamma = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
    half4 base_color       = pow(abs(base_color_gamma),2.2);

    //ambient
    half3 ambient = (0.05 * base_color).xyz;

    //lighting
    Light light = GetMainLight();
    half3 lightDir = light.direction;
    half  lightAtten = light.distanceAttenuation;
    half3 lightColor = light.color;

    //diffuse
    float diff = max(dot(lightDir, input.normal),0.0);
    half3 diffuse = diff * base_color.xyz * lightAtten * light.color;

    //specular
    float3 viewDir = SafeNormalize(input.viewDir);
    float3 reflectDir = reflect(-lightDir,input.normal);
    float  spec = pow(max(dot(viewDir , reflectDir),0.0),60.0);
    half3  specular = _SpecularStrength * lightAtten * spec * light.color;

    half4 final_color = half4(ambient + diffuse + specular,1.0);
    final_color = pow(abs(final_color),1.0/2.2);

    return final_color;
}

#endif 