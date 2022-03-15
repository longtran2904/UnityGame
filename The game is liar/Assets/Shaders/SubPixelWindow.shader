Shader "Unlit/SubPixelWindow"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _PixelateAmount ("PixelateAmount", Float) = 256
        _NoiseScale ("NoiseScale", Float) = 500
        _MinBaseValue ("MinBaseValue", Float) = .65
        _MaxBaseValue ("MaxBaseValue", Float) = .8
        _MinTimeNoiseScale ("MinTimeNoiseScale", Float) = 180
        _MaxTimeNoiseScale ("MaxTimeNoiseScale", Float) = 200
        _Speed ("Speed", Float) = 1
        [HDR]
        _Color ("Color", Color) = (0.1411765, 0.7490196, 0.7490196, 1)
        _BackgroundColor ("BackgroundColor", Color) = (0, 0.09803922, 0.1647059, 1)
        _Brightness ("Brightness", Float) = 1
        _Ratio ("Ratio", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Zwrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            inline float unity_noise_randomValue(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            inline float unity_noise_interpolate(float a, float b, float t)
            {
                return (1.0 - t) * a + (t * b);
            }

            inline float unity_valueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);

                uv = abs(frac(uv) - 0.5);
                float2 c0 = i + float2(0.0, 0.0);
                float2 c1 = i + float2(1.0, 0.0);
                float2 c2 = i + float2(0.0, 1.0);
                float2 c3 = i + float2(1.0, 1.0);
                float r0 = unity_noise_randomValue(c0);
                float r1 = unity_noise_randomValue(c1);
                float r2 = unity_noise_randomValue(c2);
                float r3 = unity_noise_randomValue(c3);

                float bottomOfGrid = unity_noise_interpolate(r0, r1, f.x);
                float topOfGrid = unity_noise_interpolate(r2, r3, f.x);
                float t = unity_noise_interpolate(bottomOfGrid, topOfGrid, f.y);
                return t;
            }

            void Unity_SimpleNoise_float(float2 UV, float Scale, out float Out)
            {
                float t = 0.0;

                float freq = pow(2.0, float(0));
                float amp = pow(0.5, float(3 - 0));
                t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                freq = pow(2.0, float(1));
                amp = pow(0.5, float(3 - 1));
                t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                freq = pow(2.0, float(2));
                amp = pow(0.5, float(3 - 2));
                t += unity_valueNoise(float2(UV.x * Scale / freq, UV.y * Scale / freq)) * amp;

                Out = t;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            float _PixelateAmount;
            float _NoiseScale;
            float _MinBaseValue;
            float _MaxBaseValue;
            float _MinTimeNoiseScale;
            float _MaxTimeNoiseScale;
            float _Speed;
            float4 _Color;
            float4 _BackgroundColor;
            float _Brightness;
            float _Ratio;

            float GetNoise(float2 uv, float noiseScale)
            {
                float result = 0;
                Unity_SimpleNoise_float(uv, noiseScale, result);
                return result;
            }

            float4 Sampling(float2 uv)
            {
                uv.x *= _Ratio;

                // Pixelate the texture
                uv *= _PixelateAmount;
                uv = floor(uv);
                uv /= _PixelateAmount;

                // Star generation
                float star = GetNoise(uv, _NoiseScale);
                star = smoothstep(_MinBaseValue, _MaxBaseValue, star);
                star *= GetNoise(uv, lerp(_MinTimeNoiseScale, _MaxTimeNoiseScale, _SinTime.w * _Speed)); // Change the star value over time

                float4 col = star * _Color * _Brightness;
                col += _BackgroundColor; // This will make black pixels have background color (it also make normal stars a little brighter but I don't care)

                return col;
            }

            float2 InigoQuilez(float2 uv, float2 texelSize)
            {
                float2 pixel = uv * texelSize;

                float2 seam = floor(pixel + 0.5);
                float2 dudv = fwidth(pixel);
                pixel = seam + clamp((pixel - seam) / dudv, -0.5, 0.5);

                uv = pixel / texelSize;

                return uv;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = Sampling(i.uv);
                return col;
            }
            ENDCG
        }
    }
}
