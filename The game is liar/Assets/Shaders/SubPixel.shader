Shader "Unlit/SubPixel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _mode("Mode", Int) = 0
        _sampler_type("Sampler Type (point, linear)", Int) = 0
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

            float4 Sampling(float2 uv)
            {
                if (_mode == 0)
                {
                    // uv - your texcoord
                    // tex - your texture sampler with bilinear filtering
                    uv *= _MainTex_TexelSize.zw;
                    float2 duv = fwidth(uv);
                    uv = floor(uv) + .5 + clamp((frac(uv) - .5 + duv) / duv, 0, 1);
                    uv *= _MainTex_TexelSize.xy;
                }
                else if (_mode == 1)
                {
                    float2 pixels = uv * _MainTex_TexelSize.zw;

                    // Updated to the final article
                    float2 alpha = 0.7 * fwidth(pixels);
                    float2 pixels_fract = frac(pixels);
                    float2 pixels_diff = clamp(.5 / alpha * pixels_fract, 0, .5) + clamp(.5 / alpha * (pixels_fract - 1) + .5, 0, .5);
                    pixels = floor(pixels) + pixels_diff;
                    uv = pixels * _MainTex_TexelSize.xy;
                }
                else if (_mode == 2)
                {
                    float2 pixels = uv * _MainTex_TexelSize.zw + 0.5;

                    // tweak fractional value of the texture coordinate
                    float2 fl = floor(pixels);
                    float2 fr = frac(pixels);
                    float2 aa = fwidth(pixels) * 0.75;

                    fr = smoothstep(.5 - aa, .5 + aa, fr);

                    uv = (fl + fr - 0.5) * _MainTex_TexelSize.xy;
                }
                else if (_mode == 3)
                {
                    float2 pixel = uv * _MainTex_TexelSize.zw;
                    
                    float2 seam = floor(pixel + 0.5);
                    float2 dudv = fwidth(pixel);
                    pixel = seam + clamp((pixel - seam) / dudv, -0.5, 0.5);

                    uv = pixel * _MainTex_TexelSize.xy;
                }

                if (_sampler_type == 0)
                    return _MainTex.Sample(point_clamp_sampler, uv);
                else
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
