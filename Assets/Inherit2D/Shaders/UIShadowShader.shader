Shader "Unlit/UIShadowShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GradientStartColor ("Gradient Start Color", Color) = (1, 0, 0, 1)
        _GradientEndColor ("Gradient End Color", Color) = (0, 0, 1, 1)
        _BlurSize ("Blur Size", Range(0, 10)) = 1.0
        _GradientDirection ("Gradient Direction", Range(0, 1)) = 0.0 // 0: Horizontal, 1: Vertical
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _GradientStartColor;
            float4 _GradientEndColor;
            float _BlurSize;
            float _GradientDirection; // Tham số điều chỉnh hướng gradient

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 color = fixed4(0, 0, 0, 0);

                // Số lượng sample cho Gaussian Blur
                int samples = 5;
                float totalWeight = 0.0;

                for (int x = -samples; x <= samples; x++)
                {
                    for (int y = -samples; y <= samples; y++)
                    {
                        float weight = exp(-float(x * x + y * y) / (2.0 * samples * samples));
                        float2 offset = float2(x, y) * _BlurSize * 0.01;
                        color += tex2D(_MainTex, uv + offset) * weight;
                        totalWeight += weight;
                    }
                }

                color /= totalWeight;

                // Tính toán màu gradient theo tọa độ UV
                // Điều chỉnh hướng gradient dựa trên _GradientDirection
                float gradientFactor = (_GradientDirection == 0.0) ? uv.x : uv.y; // X cho horizontal, Y cho vertical
                fixed4 gradientColor = lerp(_GradientStartColor, _GradientEndColor, gradientFactor);

                // Áp dụng màu gradient vào màu của texture
                return color * gradientColor;
            }
            ENDCG
        }
    }

    FallBack "Transparent"
}
