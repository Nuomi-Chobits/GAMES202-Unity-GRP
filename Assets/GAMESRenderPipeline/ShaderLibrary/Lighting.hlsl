#ifndef GRP_LIGHTING_INCLUDED
#define GRP_LIGHTING_INCLUDED

#define MAX_DIRECTIONAL_LIGHT_COUNT 4

CBUFFER_START(_GRPLight)
	int _DirectionalLightCount;
	float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
    float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

struct Light
{
    float3 dir;
    half3  color;
    float  attenuation;
};

int GetDirectionalLightCount ()
{
    return _DirectionalLightCount;
}

DirectionalShadowData GetDirectionalShadowData(int lightIndex) {
    DirectionalShadowData data;
    data.strength = _DirectionalLightShadowData[lightIndex].x;
    data.tileIndex = _DirectionalLightShadowData[lightIndex].y;
    return data;
}

Light GetLight(int index, float3 positionWS)
{
    Light light;
    light.dir = _DirectionalLightDirections[index].rgb;
    light.color = _DirectionalLightColors[index].rgb;
    DirectionalShadowData shadowData = GetDirectionalShadowData(index);
    light.attenuation = GetDirectionalShadowAttenuation(shadowData, positionWS);
    return light;
}

Light GetMainLight(float3 positionWS)
{
    return GetLight((int)0, positionWS);
}

half4 GRPFragmentBlinnPhong(half4 base_color,float3 normal,float3 viewDir,float3 positionWS,float SpecularStrength)
{
    normal = normalize(normal);

    //ambient
    half3 ambient = (0.05 * base_color).xyz;
    
    //light
    Light light = GetMainLight(positionWS);

    //diffuse
    float diff = saturate(max(dot(light.dir, normal),0.0) * light.attenuation);
    half3 diffuse = diff * base_color.xyz * light.color;

    //specular
    viewDir = SafeNormalize(viewDir);
    float3 reflectDir = reflect(-light.dir,normal);
    float  spec = pow(max(dot(viewDir , reflectDir),0.0),60.0);
    half3  specular = SpecularStrength * spec * light.color;

    half3 final_color = ambient + diffuse + specular;
    half alpha = 1.0;

    return half4(final_color,alpha);
}

#endif