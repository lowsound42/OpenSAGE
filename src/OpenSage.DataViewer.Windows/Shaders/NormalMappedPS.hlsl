#include "MeshCommonPS.hlsli"
#include "NormalMapped.hlsli"

//#define SPECULAR_ENABLED
#define LIGHTING_TYPE Object
#include "Lighting.hlsli"

cbuffer MaterialConstants : register(b2)
{
    float BumpScale;
    float SpecularExponent;
    bool AlphaTestEnable;
    float4 AmbientColor;
    float4 DiffuseColor;
    float4 SpecularColor;
};

Texture2D<float4> DiffuseTexture : register(t0);
Texture2D<float4> NormalMap : register(t1);

SamplerState Sampler : register(s0);

float4 main(VSOutputSimple input) : SV_Target
{
    float2 uv = input.TransferCommon.UV0;

    // TODO: Should do this in vertex shader?
    float3x3 tangentToWorldSpace = float3x3(
        input.WorldTangent,
        input.WorldBinormal,
        input.TransferCommon.WorldNormal);

    float3 tangentSpaceNormal = (NormalMap.Sample(Sampler, uv).rgb * 2) - float3(1, 1, 1);
    tangentSpaceNormal.xy *= BumpScale;
    tangentSpaceNormal = normalize(tangentSpaceNormal);

    float3 worldSpaceNormal = mul(tangentSpaceNormal, tangentToWorldSpace);

    LightingParameters lightingParams;
    lightingParams.WorldPosition = input.TransferCommon.WorldPosition;
    lightingParams.WorldNormal = worldSpaceNormal;
    lightingParams.MaterialAmbient = AmbientColor.rgb;
    lightingParams.MaterialDiffuse = DiffuseColor.rgb;
    lightingParams.MaterialSpecular = SpecularColor.rgb;
    lightingParams.MaterialShininess = SpecularExponent;

    float3 diffuseColor;
    float3 specularColor;
    DoLighting(lightingParams, diffuseColor, specularColor);

    float4 diffuseTextureColor = DiffuseTexture.Sample(Sampler, uv);

    if (AlphaTestEnable)
    {
        if (diffuseTextureColor.a < AlphaTestThreshold)
        {
            discard;
        }
    }

    float3 objectColor = diffuseTextureColor.rgb * diffuseColor;

    objectColor += specularColor;

    return float4(
        objectColor,
        DiffuseColor.a * diffuseTextureColor.a);
}