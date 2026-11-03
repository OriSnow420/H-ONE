Shader "HOne/Map Reveal Fog"
{
    Properties
    {
        _RevealMask ("Reveal Mask", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 positionWS : TEXCOORD0;
            };

            TEXTURE2D(_RevealMask);
            SAMPLER(sampler_RevealMask);
            float4 _MapMinimum;
            float4 _MapSize;
            float4 _PlayerPosition;
            float _PlayerRadius;
            float _EdgeFeather;
            int _PulseCount;
            float4 _PulseData[32];

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(world);
                output.positionWS = world.xy;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = (input.positionWS - _MapMinimum.xy) / _MapSize.xy;
                float reveal = SAMPLE_TEXTURE2D(_RevealMask, sampler_RevealMask, uv).r;
                float playerDistance = distance(input.positionWS, _PlayerPosition.xy);
                reveal = max(reveal, 1.0 - smoothstep(_PlayerRadius - _EdgeFeather,
                    _PlayerRadius + _EdgeFeather, playerDistance));

                float ring = 0.0;
                for (int i = 0; i < _PulseCount; i++)
                {
                    float distanceToPulse = distance(input.positionWS, _PulseData[i].xy);
                    float radius = _PulseData[i].z;
                    reveal = max(reveal, 1.0 - smoothstep(radius - _EdgeFeather,
                        radius + _EdgeFeather, distanceToPulse));
                    ring = max(ring, (1.0 - smoothstep(0.0, 0.18,
                        abs(distanceToPulse - radius))) * _PulseData[i].w);
                }

                float fogAlpha = 1.0 - reveal;
                float ringAlpha = ring * 0.55;
                float3 fogColor = float3(0.006, 0.008, 0.012);
                float3 ringColor = float3(0.53, 0.74, 0.70);
                return half4(lerp(fogColor, ringColor, ring), max(fogAlpha, ringAlpha));
            }
            ENDHLSL
        }
    }
}
