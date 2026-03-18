Shader "Custom/LicensePlate_Improved"
{
    Properties
    {
        _CharAtlas ("Character Atlas", 2DArray) = "" {}

        _Char0 ("Char 0", Int) = -1
        _Char1 ("Char 1", Int) = -1
        _Char2 ("Char 2", Int) = -1
        _Char3 ("Char 3", Int) = -1
        _Char4 ("Char 4", Int) = -1
        _Char5 ("Char 5", Int) = -1
        _Char6 ("Char 6", Int) = -1

        _PlateColor ("Plate Color", Color) = (1,1,1,1)
        _CharColor ("Char Color", Color) = (0,0,0,1)

        _ContentWidth ("Content Width", Range(0.1, 1.0)) = 0.7
        _CharAspect ("Char Aspect", Range(0.5, 10.0)) = 1.0
        _Spacing ("Character Spacing", Range(0.0, 0.1)) = 0.02
        _VerticalOffset ("Vertical Offset", Range(-0.5, 0.5)) = 0.0
        _VerticalPadding ("Vertical Padding", Range(0.0, 0.4)) = 0.1
        _EdgePadding ("Edge Padding", Range(0.0, 0.2)) = 0.05
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require 2darray

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_ARRAY(_CharAtlas);
            SAMPLER(sampler_CharAtlas);

            CBUFFER_START(UnityPerMaterial)
                int _Char0, _Char1, _Char2, _Char3, _Char4, _Char5, _Char6;

                float4 _PlateColor;
                float4 _CharColor;

                float _ContentWidth;
                float _CharAspect;
                float _Spacing;
                float _VerticalOffset;
                float _VerticalPadding;
                float _EdgePadding;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            int GetActiveCharCount(int chars[7])
            {
                int count = 0;
                for (int i = 0; i < 7; i++)
                {
                    if (chars[i] >= 0)
                        count++;
                }
                return max(count, 1);
            }

            float4 SampleChar(float2 uv, int charIndex, int activeIndex, int activeCount)
            {
                if (charIndex < 0)
                    return float4(0,0,0,0);

                // Centered content region
                float contentWidth = _ContentWidth;
                float contentMin = 0.5 - contentWidth * 0.5;
                float contentMax = 0.5 + contentWidth * 0.5;

                if (uv.x < contentMin || uv.x > contentMax)
                    return float4(0,0,0,0);

                // Normalize inside content region
                float u = (uv.x - contentMin) / (contentMax - contentMin);

                // Proper slot distribution
                float slotWidth = 1.0 / activeCount;
                float paddedWidth = slotWidth - _Spacing;

                float slotMin = activeIndex * slotWidth + (_Spacing * 0.5);
                float slotMax = slotMin + paddedWidth;

                if (u < slotMin || u > slotMax)
                    return float4(0,0,0,0);

                float localU = (u - slotMin) / paddedWidth;

                // Edge padding
                localU = saturate(localU * (1.0 - _EdgePadding * 2.0) + _EdgePadding);

                // Vertical alignment
                // Apply aspect + offset
                float v = 0.5 + (uv.y - 0.5 + _VerticalOffset) * _CharAspect;

                // Apply vertical padding (shrink usable range)
                float vMin = _VerticalPadding;
                float vMax = 1.0 - _VerticalPadding;

                // Remap into padded region
                v = lerp(vMin, vMax, v);

                // Clamp to prevent sampling outside atlas
                v = saturate(v);

                float2 atlasUV = float2(localU, v);

                return SAMPLE_TEXTURE2D_ARRAY(_CharAtlas, sampler_CharAtlas, atlasUV, charIndex);
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float4 result = _PlateColor;

                int chars[7] = { _Char0, _Char1, _Char2, _Char3, _Char4, _Char5, _Char6 };

                int activeCount = GetActiveCharCount(chars);

                int activeIndex = 0;

                for (int i = 0; i < 7; i++)
                {
                    if (chars[i] < 0)
                        continue;

                    float4 charSample = SampleChar(IN.uv, chars[i], i, activeCount);

                    result = lerp(result, _CharColor, charSample.a);

                    activeIndex++;
                }

                return result;
            }

            ENDHLSL
        }
    }
}