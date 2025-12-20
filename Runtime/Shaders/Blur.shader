Shader "PostProcessing/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Spread ("Standard Deviation (Spread)", Float) = 0
        _GridSize ("Grid Size", Integer) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        sampler2D _MainTex;

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_TexelSize;
            float _Spread;
            uint _GridSize;
        CBUFFER_END

        float Gaussian(float x)
        {
            float sigmaSqu = _Spread * _Spread;
            return (1 / sqrt(TWO_PI * sigmaSqu)) * exp(-(x * x / (2 * sigmaSqu)));
        }

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        Varyings Vert(Attributes IN)
        {
            Varyings OUT;
            OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
            OUT.uv = IN.uv;
            return OUT;
        }
        ENDHLSL

        Pass
        {
            Name "HorizontalBlur"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragHorizontal

            float4 FragHorizontal(Varyings IN) : SV_Target
            {
                float3 col = float3(0, 0, 0);
                float gridSum = 0;

                int upper = (_GridSize - 1) / 2;
                int lower = -upper;
                for (int i = lower; i <= upper; i++)
                {
                    float gauss = Gaussian(i);
                    gridSum += gauss;
                    float2 uv = IN.uv + float2(i * _MainTex_TexelSize.x, 0);
                    col += tex2D(_MainTex, uv).rgb * gauss;
                }

                col /= gridSum;
                return float4(col, 1);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "VerticalBlur"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragVertical

            float4 FragVertical(Varyings IN) : SV_Target
            {
                float3 col = float3(0, 0, 0);
                float gridSum = 0;

                int upper = (_GridSize - 1) / 2;
                int lower = -upper;
                for (int i = lower; i <= upper; i++)
                {
                    float gauss = Gaussian(i);
                    gridSum += gauss;
                    float2 uv = IN.uv + float2(0, i * _MainTex_TexelSize.x);
                    col += tex2D(_MainTex, uv).rgb * gauss;
                }

                col /= gridSum;
                return float4(col, 1);
            }
            ENDHLSL
        }
    }
}