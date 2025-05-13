Shader "Custom/WireframeWebGL"
{
    Properties
    {
        _WireThickness ("Wire Thickness", Range(0.001, 0.05)) = 0.01
        _Color ("Object Color", Color) = (1,1,1,1)
        _WireColor ("Wireframe Color", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Wireframe Pass"

            HLSLPROGRAM
            #pragma target 3.0 // Đảm bảo WebGL hỗ trợ
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _WireThickness;
            float4 _Color;
            float4 _WireColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 barycentric : TEXCOORD1; // Tọa độ Barycentric cho Wireframe
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 barycentric : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(Attributes input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Chuyển đổi sang Clip Space
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.barycentric = input.barycentric; // Giữ nguyên tọa độ Barycentric

                return output;
            }

            half4 frag(v2f i) : SV_Target
            {
                // Tính khoảng cách từ cạnh gần nhất
                float minEdge = min(i.barycentric.x, min(i.barycentric.y, i.barycentric.z));
                float wireAlpha = smoothstep(0.01, 0.01 + _WireThickness, minEdge);

                // Trộn màu vật thể và màu wireframe
                return lerp(_WireColor, _Color, wireAlpha);
            }
            ENDHLSL
        }
    }
}
