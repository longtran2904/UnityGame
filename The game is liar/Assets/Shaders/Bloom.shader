Shader "Hidden/Bloom"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            Texture2D _MainTex;
            float4 _MainTex_TexelSize;
            SamplerState sampler_MainTex;

#define TEXTURE2D_ARGS(textureName, samplerName) Texture2D textureName, SamplerState samplerName
#define TEXTURE2D_PARAM(textureName, samplerName) textureName, samplerName
#define SAMPLE_TEXTURE2D(textureName, samplerName, coord2) textureName.Sample(samplerName, coord2)

            half4 DownsampleBox13Tap(TEXTURE2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize)
            {
                half4 A = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(-1.0, -1.0));
                half4 B = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(0.0, -1.0));
                half4 C = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(1.0, -1.0));
                half4 D = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(-0.5, -0.5));
                half4 E = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(0.5, -0.5));
                half4 F = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(-1.0, 0.0));
                half4 G = SAMPLE_TEXTURE2D(tex, samplerTex, uv);
                half4 H = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(1.0, 0.0));
                half4 I = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(-0.5, 0.5));
                half4 J = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(0.5, 0.5));
                half4 K = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(-1.0, 1.0));
                half4 L = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(0.0, 1.0));
                half4 M = SAMPLE_TEXTURE2D(tex, samplerTex, uv + texelSize * float2(1.0, 1.0));

                half2 div = (1.0 / 4.0) * half2(0.5, 0.125);

                half4 o = (D + E + I + J) * div.x;
                o += (A + B + G + F) * div.y;
                o += (B + C + H + G) * div.y;
                o += (F + G + L + K) * div.y;
                o += (G + H + M + L) * div.y;

                return o;
            }

            half4 UpsampleTent(TEXTURE2D_ARGS(tex, samplerTex), float2 uv, float2 texelSize, float4 sampleScale)
            {
                float4 d = texelSize.xyxy * float4(1.0, 1.0, -1.0, 0.0) * sampleScale;

                half4 s;
                s = SAMPLE_TEXTURE2D(tex, samplerTex, uv - d.xy);
                s += SAMPLE_TEXTURE2D(tex, samplerTex, uv - d.wy) * 2.0;
                s += SAMPLE_TEXTURE2D(tex, samplerTex, uv - d.zy);

                s += SAMPLE_TEXTURE2D(tex, samplerTex, uv + d.zw) * 2.0;
                s += SAMPLE_TEXTURE2D(tex, samplerTex, uv) * 4.0;
                s += SAMPLE_TEXTURE2D(tex, samplerTex, uv + d.xw) * 2.0;

                s += SAMPLE_TEXTURE2D(tex, samplerTex, uv + d.zy);
                s += SAMPLE_TEXTURE2D(tex, samplerTex, uv + d.wy) * 2.0;
                s += SAMPLE_TEXTURE2D(tex, samplerTex, uv + d.xy);

                return s * (1.0 / 16.0);
            }

            /*half4 Prefilter(half4 color, float2 uv)
            {
                half autoExposure = SAMPLE_TEXTURE2D(_AutoExposureTex, sampler_AutoExposureTex, uv).r;
                color *= autoExposure;
                color = min(_Params.x, color); // clamp to max
                color = QuadraticThreshold(color, _Threshold.x, _Threshold.yzw);
                return color;
            }*/

            half4 FragPrefilter13(float2 uv)
            {
                half4 color = DownsampleBox13Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), uv, _MainTex_TexelSize.xy);
                //return Prefilter(SafeHDR(color), i.texcoord);
                return color;
            }

            half4 FragDownsample13(float2 uv)
            {
                half4 color = DownsampleBox13Tap(TEXTURE2D_PARAM(_MainTex, sampler_MainTex), uv, _MainTex_TexelSize.xy);
                return color;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
                return col;
            }
            ENDCG
        }
    }
}
