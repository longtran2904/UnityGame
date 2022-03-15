Shader "Unlit/SubPixel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _mode("Mode", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
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

            Texture2D _MainTex;
            float4 _MainTex_TexelSize;
            SamplerState linear_clamp_sampler;
            SamplerState point_clamp_sampler;
            int _mode;
            int _sampler_type;

            float2 CSantos(float2 uv, float2 texelSize)
            {
                float2 pixels = uv * texelSize;
                float2 alpha = 0.7 * fwidth(pixels);
                float2 pixels_fract = frac(pixels);
                float2 pixels_diff = clamp(.5 / alpha * pixels_fract, 0, .5) + clamp(.5 / alpha * (pixels_fract - 1) + .5, 0, .5);
                pixels = floor(pixels) + pixels_diff;
                uv = pixels / texelSize;

                return uv;
            }

            float2 Klems(float2 uv, float2 texelSize)
            {
                float2 pixels = uv * texelSize + 0.5;
                float2 fl = floor(pixels);
                float2 fr = frac(pixels);
                float2 aa = fwidth(pixels) * 0.75;

                fr = smoothstep(.5 - aa, .5 + aa, fr);

                uv = (fl + fr - 0.5) / texelSize;

                return uv;
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

            // This is the Casey's way
            float2 FatPixel(float2 uv, float2 texelSize)
            {
                float2 pixel = uv * texelSize;

                float2 fat_pixel = floor(pixel) + 0.5;
                fat_pixel += 1 - clamp((1.0 - frac(pixel)) * fwidth(pixel + .5), 0, 1);

                uv = fat_pixel / texelSize;

                return uv;
            }

            // https://jorenjoestar.github.io/post/pixel_art_filtering/
            float4 Sampling(float2 uv)
            {
                if (_mode == 0)
                    uv = CSantos(uv, _MainTex_TexelSize.zw);
                else if (_mode == 1)
                    uv = Klems(uv, _MainTex_TexelSize.zw);
                else if (_mode == 2)
                    uv = InigoQuilez(uv, _MainTex_TexelSize.zw);
                else if (_mode == 3)
                    uv = FatPixel(uv, _MainTex_TexelSize.zw);

                return _MainTex.Sample(linear_clamp_sampler, uv);
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
                // sample the texture
                fixed4 col = Sampling(i.uv);
                return col;
            }
            ENDCG
        }
    }
}
