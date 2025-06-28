Shader "Unlit/LEDGrid"
{
    Properties
    {
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 1)
        _GridSize ("Grid Size", Vector) = (10, 10, 0, 0)
        _LineWidth ("Line Width", Range(0.001, 0.1)) = 0.02
        _ScrollSpeed ("Scroll Speed", Vector) = (0.1, 0.0, 0, 0)
        _EmissionStrength ("Emission", Range(0, 5)) = 1.0
        _HueSpeed ("Hue Rotation Speed", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _BackgroundColor;
            float4 _GridSize;
            float _LineWidth;
            float4 _ScrollSpeed;
            float _EmissionStrength;
            float _HueSpeed;

            float3 HSVtoRGB(float h, float s, float v)
            {
                float3 rgb = clamp(abs(frac(h + float3(0.0, 2.0/3.0, 1.0/3.0)) * 6.0 - 3.0) - 1.0, 0.0, 1.0);
                return v * lerp(float3(1.0, 1.0, 1.0), rgb, s);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv + _ScrollSpeed.xy * _Time.y;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 gridUV = frac(i.uv * _GridSize.xy);

                float lineX = step(gridUV.x, _LineWidth) + step(1.0 - gridUV.x, _LineWidth);
                float lineY = step(gridUV.y, _LineWidth) + step(1.0 - gridUV.y, _LineWidth);
                float lineValue = saturate(lineX + lineY);

                float hue = frac(_Time.y * _HueSpeed);
                float3 dynamicColor = HSVtoRGB(hue, 1.0, 1.0);

                float4 color = lerp(_BackgroundColor, float4(dynamicColor, 1.0), lineValue);
                return color * _EmissionStrength;
            }
            ENDCG
        }
    }
}
