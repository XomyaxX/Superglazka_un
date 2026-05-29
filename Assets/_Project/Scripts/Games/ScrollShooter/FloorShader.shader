Shader "Superglazka/NeonFloor"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.06, 0, 0.13, 1)
        _LineColor ("Line Color", Color) = (0.2, 0, 0.4, 1)
        _GridScale ("Grid Scale", Float) = 2
        _ScrollSpeed ("Scroll Speed", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogCoord : TEXCOORD1;
            };

            float4 _BaseColor;
            float4 _LineColor;
            float _GridScale;
            float _ScrollSpeed;

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.uv * _GridScale;
                float scroll = _Time.y * _ScrollSpeed;
                uv.y += scroll;
                float2 grid = abs(frac(uv - 0.5) - 0.5) / fwidth(uv);
                float line = min(grid.x, grid.y);
                float val = 1.0 - min(line, 1.0);
                float4 col = lerp(_BaseColor, _LineColor, val * 0.5);
                col.rgb = MixFog(col.rgb, input.fogCoord);
                return col;
            }
            ENDHLSL
        }
    }
}
