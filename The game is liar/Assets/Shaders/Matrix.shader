Shader "Unlit/Matrix"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _white_noise("White noise", 2D) = "white" {}
        _font_texture("Font texture", 2D) = "white" {}
        
        _screen_width("Width", Int) = 256
        _screen_height("Height", Int) = 256
        _letter_size("Pixelate amount", Int) = 16

        _speed_base_x("Speed x", Float) = 1.
        _speed_base_y("Speed y", Float) = 1.
        _color_speed("Color change speed", Float) = 1.

        _mode("Mode (Move in x, y, xy)", Int) = 0
        _flip("Flip (None, X, Y, XY)", Int) = 0

        _from_color("From Color", Color) = (.1, 1., .35, 1)
        _to_color("To Color", Color) = (.1, 1., .35, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        // https://shahriyarshahrabi.medium.com/shader-studies-matrix-effect-3d2ead3a84c5
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

            uint      _screen_width;
            uint      _screen_height;
            uint      _letter_size;

            float     _speed_base_x;
            float     _speed_base_y;
            float     _color_speed;

            int       _mode;
            int       _flip;

            sampler2D _white_noise;
            sampler2D _font_texture;

            float4    _white_noise_TexelSize;
            float4    _from_color;
            float4    _to_color;
            float4    _bg_color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // Color conversion: https://www.chilliant.com/rgb2hsv.html
            float3 HUEtoRGB(in float H)
            {
                float R = abs(H * 6 - 3) - 1;
                float G = 2 - abs(H * 6 - 2);
                float B = 2 - abs(H * 6 - 4);
                return saturate(float3(R, G, B));
            }

            float3 HSVtoRGB(in float3 HSV)
            {
                float3 RGB = HUEtoRGB(HSV.x);
                return ((RGB - 1) * HSV.y + 1) * HSV.z;
            }

            float Epsilon = 1e-10;

            float3 RGBtoHCV(in float3 RGB)
            {
                // Based on work by Sam Hocevar and Emil Persson
                float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
                float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
                float C = Q.x - min(Q.w, Q.y);
                float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
                return float3(H, C, Q.x);
            }

            float3 RGBtoHSV(in float3 RGB)
            {
                float3 HCV = RGBtoHCV(RGB);
                float S = HCV.y / (HCV.z + Epsilon);
                return float3(HCV.x, S, HCV.z);
            }
            //-------------------------------------------------------------------------------

            float text(float2 coord)
            {
                float2 uv = frac(coord.xy / _letter_size);                // Geting the fract part of the block, this is the uv map for the block
                float2 block = floor(coord.xy / _letter_size);                // Getting the id for the block. The first blocl is (0,0) to its right (1,0), and above it (0,1) 
                uv = uv * 0.7 + .1;                       // Zooming a bit in each block to have larger letters

                uv += floor(tex2D(_white_noise, block / _white_noise_TexelSize.zw + _Time.y * .002).xy * _letter_size);

                uv /= _letter_size;                              // So far the uv value is between 0-16. To sample the font texture we need to normalize this to 0-1. hence a divid by 16
                uv.x = 1 - uv.x;
                return tex2D(_font_texture, uv).r;
            }
            //---------------------------------------------------------

            float3 rain(float2 fragCoord, float3 color)
            {
                int2 grid = float2(floor(fragCoord.x / _letter_size), floor(fragCoord.y / _letter_size));
                float2 speed = float2(cos(grid.y * 3.) * .15 + .35, cos(grid.x * 3.) * .15 + .35) * float2(_speed_base_x, _speed_base_y);
                float2 offset = float2(sin(grid.y * 15.), sin(grid.x * 15.));

                switch (_flip)
                {
                    case 1:
                    {
                        fragCoord.x *= -1;
                    } break;
                    case 2:
                    {
                        fragCoord.y *= -1;
                    } break;
                    case 3:
                    {
                        fragCoord *= -1;
                    } break;
                }

                float y = frac((fragCoord.y / _screen_height) + _Time.y * speed.y + offset.y);                
                float x = frac((fragCoord.x / _screen_width) + _Time.y * speed.x + offset.x);

                if (_mode == 0)
                    y = 1;
                else if (_mode == 1)
                    x = 1;

                return color / ((x * y) * 20.);
            }

            //---------------------------------------------------------
#define scale 1
            fixed4 frag(v2f i) : SV_Target
            {
                float t = (sin(_Time.y * _color_speed) + 1) / 2;
                float3 fromHSV = RGBtoHSV(_from_color);
                float3 toHSV = RGBtoHSV(_to_color);
                float3 hsv = (1 - t) * fromHSV + t * toHSV;
                float3 color = HSVtoRGB(hsv);

                fixed4 col = float4(0., 0., 0., _from_color.w);
                col.xyz = text(i.uv * float2(_screen_width, _screen_height) * scale) * rain(i.uv * float2(_screen_width, _screen_height) * scale, color);
                return col;
            }
            ENDCG
        }
    }
}
