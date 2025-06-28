Shader "Unlit/GymWoodFloor"
{
    Properties
    {
        _BoardColor ("Base Wood Color", Color) = (0.8, 0.65, 0.4, 1)
        _StripeStrength ("Stripe Strength", Range(0, 0.3)) = 0.1
        _BoardCount ("Board Count", Float) = 20
        _GlossStrength ("Gloss Strength", Range(0, 1)) = 0.15
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

            float4 _BoardColor;
            float _StripeStrength;
            float _BoardCount;
            float _GlossStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                o.uv = v.uv;
                return o;
            };

            fixed4 frag(v2f i) : SV_Target
            {
                float boardCount = _BoardCount;
                float2 uv = i.uv;

                float boardIndex = floor(uv.x * boardCount);

                float stripe = sin(boardIndex * 10.0 + uv.y * 30.0) * 0.5 + 0.5;
                stripe += sin(uv.y * 100.0) * 0.05;

                float3 baseColor = _BoardColor.rgb * (1.0 - _StripeStrength * stripe);

                float seam = step(frac(uv.x * boardCount), 0.015);
                baseColor *= 1.0 - seam * 0.4;

                float gloss = pow(1.0 - abs(i.viewDir.y), 6.0) * _GlossStrength;

                return float4(baseColor + gloss, 1.0);
            }
            ENDCG
        }
    }
}
