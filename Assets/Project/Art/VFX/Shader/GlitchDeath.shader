Shader "Custom/GlitchDeath"
{
    Properties
    {
        _BaseMap      ("Texture", 2D)           = "white" {}
        _BaseColor    ("Base Color", Color)      = (1,1,1,1)

        // ── 글리치 파라미터 (코드에서 DOTween으로 제어) ──────────────────────
        _GlitchIntensity ("Glitch Intensity",  Range(0,1)) = 0.0
        _GlitchSpeed     ("Glitch Speed",      Range(1,60)) = 20.0
        _ChromaShift     ("Chroma Shift",      Range(0,0.1)) = 0.0
        _Dissolve        ("Dissolve",          Range(0,1)) = 0.0
        _GlitchColor     ("Glitch Emit Color", Color) = (0,1,0.8,1)
        _GlitchEmitPower ("Glitch Emit Power", Range(0,5)) = 2.0

        // ── 내부 URP 렌더링 제어 ─────────────────────────────────────────────
        [HideInInspector] _SrcBlend   ("__src",  Float) = 5.0   // SrcAlpha
        [HideInInspector] _DstBlend   ("__dst",  Float) = 10.0  // OneMinusSrcAlpha
        [HideInInspector] _ZWrite     ("__zw",   Float) = 0.0
        [HideInInspector] _Cull       ("__cull", Float) = 2.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        // ── Forward Lit Pass ──────────────────────────────────────────────────
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ── 텍스처 / 샘플러 ────────────────────────────────────────────────
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            // ── 상수 버퍼 (SRP Batcher 호환) ──────────────────────────────────
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float  _GlitchIntensity;
                float  _GlitchSpeed;
                float  _ChromaShift;
                float  _Dissolve;
                float4 _GlitchColor;
                float  _GlitchEmitPower;
            CBUFFER_END

            // ── 난수 함수 (UV 기반 해시) ──────────────────────────────────────
            float Hash(float n)
            {
                return frac(sin(n) * 43758.5453123);
            }

            // ── 글리치 UV 오프셋 계산 ─────────────────────────────────────────
            // y 좌표를 일정 블록으로 나눠 블록마다 다른 X 오프셋을 부여.
            // _GlitchSpeed와 시간으로 빠르게 변화.
            float GlitchOffset(float uvY, float time)
            {
                float blockSize  = 0.08;                          // 글리치 블록 높이
                float block      = floor(uvY / blockSize);        // 현재 블록 번호
                float timeSlice  = floor(time * _GlitchSpeed);   // 시간 단위
                float rnd        = Hash(block + timeSlice * 7.3);
                // 특정 블록만 건너뜀 (전체가 흔들리지 않도록)
                float jump       = step(0.6, rnd);                // 40% 블록만 이동
                return (rnd - 0.5) * _GlitchIntensity * jump;
            }

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float  fogCoord    : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varyings OUT;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.fogCoord    = ComputeFogFactor(OUT.positionHCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv     = IN.uv;
                float  time   = _Time.y;

                // ── 1. 글리치 UV 흔들림 ──────────────────────────────────────
                float xOffset = GlitchOffset(uv.y, time);
                float2 uvG    = uv + float2(xOffset, 0.0);

                // ── 2. 색수차(Chroma Shift): R/G/B UV를 미세하게 분리 ─────────
                float cs      = _ChromaShift;
                float2 uvR    = uvG + float2( cs, 0.0);
                float2 uvB    = uvG + float2(-cs, 0.0);

                float r = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvR).r;
                float g = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvG).g;
                float b = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvB).b;
                float a = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvG).a;

                half3 baseRGB  = half3(r, g, b) * _BaseColor.rgb;
                half  baseAlpha = a * _BaseColor.a;

                // ── 3. 디졸브: 스캔라인 패턴 기반 소멸 ──────────────────────
                // uv.y 기반 노이즈로 픽셀을 랜덤하게 소멸
                float noiseY   = frac(sin(floor(uv.y * 80.0) + floor(time * 12.0)) * 4375.85);
                float dissolve = step(noiseY, _Dissolve);

                // 디졸브 경계 부분에 글리치 발광 추가
                float edgeGlow = smoothstep(_Dissolve - 0.05, _Dissolve, noiseY)
                               * smoothstep(_Dissolve + 0.05, _Dissolve, noiseY);
                half3 glowColor = _GlitchColor.rgb * _GlitchEmitPower * edgeGlow * (1.0 - dissolve);

                // ── 4. 글리치 강도에 따른 전체 발광 깜빡임 ──────────────────
                float blink = Hash(floor(time * _GlitchSpeed * 0.5)) * _GlitchIntensity;
                half3 emit  = _GlitchColor.rgb * blink * 1.5 + glowColor;

                // ── 5. 최종 색상 합성 ─────────────────────────────────────────
                half3 finalRGB   = baseRGB + emit;
                half  finalAlpha = baseAlpha * (1.0 - dissolve);

                // 완전히 용해된 픽셀은 버림 (ALpha Test 보완)
                clip(finalAlpha - 0.01);

                half4 col = half4(finalRGB, finalAlpha);
                col.rgb   = MixFog(col.rgb, IN.fogCoord);
                return col;
            }
            ENDHLSL
        }

        // ── Shadow Caster (그림자 유지) ───────────────────────────────────
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma vertex   ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
