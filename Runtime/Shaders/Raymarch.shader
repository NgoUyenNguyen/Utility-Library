Shader "Unlit/Raymarch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define MAX_STEPS 100
            #define SURF_DIST 0.001
            #define MAX_DIST 100

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolator
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 camPos : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            Interpolator vert (MeshData i)
            {
                Interpolator o;
                
                o.vertex = TransformObjectToHClip(i.vertex);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                o.camPos = TransformWorldToObject(GetCameraPositionWS());
                o.hitPos = (i.vertex);
                
                return o;
            }

            float GetDist(float3 p)
            {
                float d = length(p) - 0.5;
                d = length(float2(length(p.xz) - 0.5, p.y)) - 0.1;

                return d;
            }

            float RayMarch(float3 ro, float3 rd)
            {
                float dO = 0;
                float dS;
                [loop]
                for (int i = 0; i < MAX_STEPS; i++)
                {
                    float3 p = ro + dO * rd;
                    dS = GetDist(p);
                    dO += dS;
                    if (dS < SURF_DIST || dO > MAX_DIST) break;
                }

                return dO;
            }

            float3 GetNormal(float3 p)
            {
                float2 e = float2(1e-2, 0);
                float3 n = GetDist(p) - float3(
                    GetDist(p - e.xyy),
                    GetDist(p - e.yxy),
                    GetDist(p - e.yyx)
                );

                return normalize(n);
            }

            float4 frag (Interpolator i) : SV_Target
            {
                float3 ro = i.camPos;
                float3 rd = normalize(i.hitPos - i.camPos);

                float d = RayMarch(ro, rd);
                float4 col = 0;

                if (d < MAX_DIST)
                {
                    float3 p = ro + rd * d;
                    float3 n = GetNormal(p);
                    col.rgb = n;
                }
                else
                {
                    discard;
                }
                
                return col;
            }
            ENDHLSL
        }
    }
}
