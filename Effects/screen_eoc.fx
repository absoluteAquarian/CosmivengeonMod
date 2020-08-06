sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 ScreenDarken(float2 coords : TEXCOORD0) : COLOR0
{
    //Mask colour will always be black
    float4 colorMask = float4(0, 0, 0, 1);
    float4 colour = tex2D(uImage0, coords);
    
    //Progress will range from 0 (start) to 1 (end)
    //Just use it as the component in lerp (max lerp is only 33%)
    float lerpAmount = uProgress * 0.3333f;
    float4 lerp = colour * (1.0f - lerpAmount) + colorMask * lerpAmount;
    
    //Make sure to use the alpha component from the original pixel
    return lerp * colour.a;
}

technique Technique1
{
    pass ScreenDarken
    {
        PixelShader = compile ps_2_0 ScreenDarken();
    }
}