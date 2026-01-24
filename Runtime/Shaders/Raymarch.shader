Shader "Unlit/Raymarch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Sphere1 ("Sphere 1", Vector) = (0,0,0,0.5)
        _Sphere2 ("Sphere 2", Vector) = (0,0,0,0.5)
        _Blend ("Blend Level", Range(0, 1)) = 0.01
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #define MAX_STEPS 100
            #define SURF_DIST 0.001
            #define MAX_DIST 100

            struct MeshData
            {
                float4 vertex : POSITION;
            };

            struct Interpolator
            {
                float4 vertex : SV_POSITION;
                float3 camPos : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };


            float4 _BaseColor;
            float4 _Sphere1;
            float4 _Sphere2;
            float _Blend;

            Interpolator vert(MeshData i)
            {
                Interpolator o;

                o.vertex = TransformObjectToHClip(i.vertex);
                o.camPos = TransformWorldToObject(GetCameraPositionWS());
                o.hitPos = (i.vertex);

                return o;
            }

            float remap(float x, float inFrom, float inTo, float outFrom, float outTo)
            {
                return outFrom + (x - inFrom) * (outTo - outFrom) / (inTo - inFrom);
            }

            float smin(float a, float b, float k)
            {
                k *= 1.0 / (1.0 - sqrt(0.5));
                float h = max(k - abs(a - b), 0.0) / k;
                return min(a, b) - k * 0.5 * (1.0 + h - sqrt(1.0 - h * (h - 2.0)));
            }

            float GetDist(float3 p)
            {
                float sphere1 = length(p - _Sphere1.xyz) - _Sphere1.w;
                float sphere2 = length(p - _Sphere2.xyz) - _Sphere2.w;
                float torus = length(float2(length(p.xz) - 0.3, p.y)) - 0.1;

                float k = remap(_Blend, 0, 1, 1e-9, 0.05);

                return smin(smin(sphere1, sphere2, k), torus, k);
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

            float4 frag(Interpolator i) : SV_Target
            {
                float3 ro = i.camPos;
                float3 rd = normalize(i.hitPos - i.camPos);
                Light mainLight = GetMainLight();

                float d = RayMarch(ro, rd);
                float4 col = 0;

                if (d < MAX_DIST)
                {
                    float3 p = ro + rd * d;
                    float3 diffuse = dot(GetNormal(p), mainLight.direction);
                    col.rgb = diffuse * _BaseColor;
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