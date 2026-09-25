Shader "Custom/DSLRBlur"
{
    Properties
    {
        _FocusDistance ("Focus Distance", Range(0, 10)) = 0.5
        _BlurAmount ("Blur Amount", Range(0, 10)) = 0.5
        _FocusRange ("Focus Range (sharpness)", Range(0.01, 0.5)) = 0.1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "DSLRBlur"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            TEXTURE2D(_CameraDepthTexture);

            float4 _BlitTexture_TexelSize;
            float _FocusDistance;
            float _BlurAmount;
            float _FocusRange;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;

                float2 uv = float2(
                    (input.vertexID << 1) & 2,
                    input.vertexID & 2
                );

                output.uv = uv;

                #if UNITY_UV_STARTS_AT_TOP
                output.uv.y = 1.0 - output.uv.y;
                #endif

                output.positionCS = float4(
                    uv * 2.0 - 1.0,
                    0.0,
                    1.0
                );

                return output;
            }

            float GaussianWeight(float distance, float sigma)
            {
                float sigma2 = sigma * sigma;
                return exp(-(distance * distance) / (2.0 * sigma2));
            }

            float GetBlurAmount(float depth)
            {
                float depthDiff = abs(depth - _FocusDistance);
                float blur = smoothstep(0, _FocusRange, depthDiff);
                return blur * _BlurAmount;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float centerDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_LinearClamp, input.uv).r;
                centerDepth = Linear01Depth(centerDepth, _ZBufferParams);

                float blurRadius = GetBlurAmount(centerDepth);

                if (blurRadius < 0.01)
                {
                    return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.uv);
                }

                float2 texel = _BlitTexture_TexelSize.xy;
                half4 color = half4(0, 0, 0, 0);
                float totalWeight = 0.0;

                int samples = 16;
                float sigma = blurRadius * 3.0;

                for (int i = 0; i < samples; i++)
                {
                    float angle = 6.28318 * float(i) / float(samples);
                    float r = blurRadius * 8.0;

                    float2 offset = float2(cos(angle), sin(angle)) * r * texel;
                    float weight = GaussianWeight(float(i) - float(samples) * 0.5, sigma);

                    half4 sample = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.uv + offset);
                    color += sample * weight;
                    totalWeight += weight;
                }

                half4 centerSample = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, input.uv);
                color += centerSample * GaussianWeight(0, sigma) * 2.0;
                totalWeight += GaussianWeight(0, sigma) * 2.0;

                return color / totalWeight;
            }

            ENDHLSL
        }
    }
}