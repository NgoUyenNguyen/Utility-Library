Shader "PostProcessing/Blur"
{
    Properties
    {
		_Spread("Standard Deviation (Spread)", Float) = 0
		_GridSize("Grid Size", Integer) = 1
    }
    SubShader
    {
        Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
		}

		HLSLINCLUDE

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
		

		CBUFFER_START(UnityPerMaterial)
			uint _GridSize;
			float _Spread;
		CBUFFER_END

		float gaussian(int x)
		{
			float sigma = max(_Spread, 0.0001);
            float sigmaSqu = sigma * sigma;
            return exp(-(x * x) / (2 * sigmaSqu));
		}

		ENDHLSL

        Pass
        {
			Name "Horizontal"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_horizontal

            float4 frag_horizontal (Varyings i) : SV_Target
			{
				float3 col = float3(0.0f, 0.0f, 0.0f);
				float gridSum = 0.0f;

				int upper = ((_GridSize - 1) / 2);
				int lower = -upper;

				for (int x = lower; x <= upper; ++x)
				{
					float gauss = gaussian(x);
					gridSum += gauss;
					float2 uv = i.texcoord + float2(_BlitTexture_TexelSize.x * x, 0.0f);
					col += gauss * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).xyz;
				}

				col /= gridSum;

				return float4(col, 1.0f);
			}
            ENDHLSL
        }

		Pass
        {
			Name "Vertical"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_vertical

            float4 frag_vertical (Varyings i) : SV_Target
			{
				float3 col = float3(0.0f, 0.0f, 0.0f);
				float gridSum = 0.0f;

				int upper = ((_GridSize - 1) / 2);
				int lower = -upper;

				for (int y = lower; y <= upper; ++y)
				{
					float gauss = gaussian(y);
					gridSum += gauss;
					float2 uv = i.texcoord + float2(0.0f, _BlitTexture_TexelSize.y * y);
					col += gauss * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).xyz;
				}

				col /= gridSum;
				return float4(col, 1.0f);
			}
            ENDHLSL
        }
    }
}