#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED
// # include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void CalculateMainLight_float(float3 WorldPos, out float3 Direction, out float3 Color)
{
    #if SHADERGRAPH_PREVIEW
    Direction = float3(0.5, 0.5, 0);
    Color = 1;
    #else
    Light mainLight = GetMainLight(0);
    Direction = mainLight.direction;
    Color = mainLight.color;   
    #endif
}

void AddAdditionalLights_float(
    float Smoothness,
    float3 WorldPos,
    float3 WorldNormal,
    float3 WorldView,
    float MainDiffuse,
    float MainSpecular,
    float3 MainColor,
    out float Diffuse, out float Specular, out float3 Color)
{
    Diffuse = MainDiffuse;
    Specular = MainSpecular;
    Color = MainColor;
    
    #ifndef SHADERGRAPH_PREVIEW
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; i++)
    {
        Light light = GetAdditionalLight(i, WorldPos);
        half NdotL = saturate(dot(WorldNormal, light.direction));
        half atten = light.distanceAttenuation * light.shadowAttenuation;
        half thisDiffuse = atten * NdotL;
        half thisSpecular = step(0.5, LightingSpecular(thisDiffuse, light.direction, WorldNormal, WorldView, 1,Smoothness));
        Diffuse += thisDiffuse;
        Specular += thisSpecular;
        Color += light.color * (thisDiffuse + thisSpecular);
    }
    #endif

    half total = Diffuse + Specular;
    Color = total <= 0 ? MainColor : Color / total;
}

#endif