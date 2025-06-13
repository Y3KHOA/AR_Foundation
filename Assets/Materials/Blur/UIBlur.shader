Shader "Custom/UIBlurSimple"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BlurSize ("Blur Size", Float) = 2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        GrabPass { "_GrabTex" }

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _GrabTex;
            float4 _GrabTex_TexelSize;
            float _BlurSize;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = ComputeGrabScreenPos(o.pos).xy;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = fixed4(0,0,0,0);
                float2 offset = _GrabTex_TexelSize.xy * _BlurSize;

                col += tex2D(_GrabTex, i.uv + offset);
                col += tex2D(_GrabTex, i.uv - offset);
                col += tex2D(_GrabTex, i.uv + float2(offset.x, -offset.y));
                col += tex2D(_GrabTex, i.uv + float2(-offset.x, offset.y));
                col += tex2D(_GrabTex, i.uv);

                col /= 5.0;
                col.a = 0.5; // độ mờ
                return col;
            }
            ENDCG
        }
    }
}
