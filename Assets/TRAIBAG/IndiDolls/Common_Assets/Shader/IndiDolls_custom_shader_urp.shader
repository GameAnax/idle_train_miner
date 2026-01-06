Shader "TRAIBAG/IndiDolls_Custom_urp"
{
    Properties
    {
        _RenderMode ("Render Mode (0 Opaque, 1 Transparent)", Float) = 0
        _SrcBlend ("SrcBlend", Float) = 1
        _DstBlend ("DstBlend", Float) = 0
        _ZWrite ("ZWrite", Float) = 1

        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _MainUV_ST ("Main Tiling(XY) Offset(ZW)", Vector) = (1,1,0,0)
        _Metallic ("Metallic", Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5

        _MaskTex ("Mask (R=Emiss, G=Refl, B=Spec)", 2D) = "white" {}
        _MaskUV_ST ("Mask Tiling(XY) Offset(ZW)", Vector) = (1,1,0,0)
        _HasMaskTex ("Has MaskTex", Float) = 0

        _Opacity ("Opacity", Range(0,1)) = 1
        _AlphaMode ("Alpha Combine (0 OpacityOnly, 1 Multiply, 2 Min, 3 Max)", Float) = 0

        _Rim1Toggle ("Enable Rim 1", Float) = 0
        _Rim1Color ("Rim 1 Color", Color) = (1,1,1,1)
        _Rim1Power ("Rim 1 Power", Range(0.5,8)) = 2
        _Rim1Intensity ("Rim 1 Intensity", Range(0,4)) = 1.2
        _Rim1AmbientInfluence ("Rim 1 Ambient Influence", Range(0,1)) = 1

        _Rim2Toggle ("Enable Rim 2", Float) = 0
        _Rim2Color ("Rim 2 Color", Color) = (1,1,1,1)
        _Rim2Power ("Rim 2 Power", Range(0.5,8)) = 3
        _Rim2Intensity ("Rim 2 Intensity", Range(0,4)) = 0.8
        _Rim2AmbientInfluence ("Rim 2 Ambient Influence", Range(0,1)) = 1

        _ReflCube ("Reflection Cube (User)", CUBE) = "" {}
        _ReflColor ("Reflection Color", Color) = (1,1,1,1)
        _ReflIntensity ("Reflection Intensity", Range(0,4)) = 1
        _ReflFresnel ("Reflection Fresnel Power", Range(0,8)) = 4
        _ReflRoughness ("Reflection Roughness", Range(0,1)) = 0.3
        _HasReflCube ("Has User ReflCube", Float) = 0

        _EmissiveTex ("Emissive Tex (Optional)", 2D) = "black" {}
        _HasEmissiveTex ("Has EmissiveTex", Float) = 0
        _EmissiveColor ("Emissive Color", Color) = (1,1,1,1)
        _EmissiveIntensity ("Emissive Intensity", Range(0,10)) = 0
        _EmissUVScale ("Emiss LocalUV Scale (XY)", Vector) = (1,1,0,0)
        _EmissScrollDir ("Emiss Scroll Dir (XY)", Vector) = (1,0,0,0)
        _EmissScrollSpeed ("Emiss Scroll Speed", Float) = 0
        _BlinkSpeed ("Blink Speed", Float) = 0
        _BlinkPhase ("Blink Phase", Float) = 0
        _BlinkMin ("Blink Min", Range(0,1)) = 1
        _BlinkMax ("Blink Max", Range(0,5)) = 1

        _SpecExtraColor ("Spec Extra Color", Color) = (1,1,1,1)
        _SpecIntensity ("Spec Extra Intensity", Range(0,4)) = 1
        _SpecMinPower ("Spec Min Power", Range(1,128)) = 8
        _SpecMaxPower ("Spec Max Power", Range(1,256)) = 64

        _VtxTintRColor ("Vertex Tint R Color", Color) = (1,1,1,1)
        _VtxTintRStrength ("Vertex Tint R Strength", Range(0,1)) = 0
        _VtxTintGColor ("Vertex Tint G Color", Color) = (1,1,1,1)
        _VtxTintGStrength ("Vertex Tint G Strength", Range(0,1)) = 0
        _VtxTintBColor ("Vertex Tint B Color", Color) = (1,1,1,1)
        _VtxTintBStrength ("Vertex Tint B Strength", Range(0,1)) = 0
        _VtxTintAColor ("Vertex Tint A Color", Color) = (1,1,1,1)
        _VtxTintAStrength ("Vertex Tint A Strength", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS

            #pragma shader_feature _RIM_ON
            #pragma shader_feature _REFL_ON
            #pragma shader_feature _EMISS_ON
            #pragma shader_feature _EMISS_LOCALUV_ON
            #pragma shader_feature _EMISS_SCROLL_ON
            #pragma shader_feature _EMISS_BLINK_ON
            #pragma shader_feature _SPEC_EXTRA_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);        SAMPLER(sampler_MaskTex);
            TEXTURE2D(_EmissiveTex);    SAMPLER(sampler_EmissiveTex);
            TEXTURECUBE(_ReflCube);     SAMPLER(sampler_ReflCube);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainUV_ST;
                float _Metallic;
                float _Smoothness;
                float4 _MaskUV_ST;
                float _HasMaskTex;
                float _RenderMode; float _SrcBlend; float _DstBlend; float _ZWrite;
                float _Opacity; float _AlphaMode;
                float _Rim1Toggle; float4 _Rim1Color; float _Rim1Power; float _Rim1Intensity; float _Rim1AmbientInfluence;
                float _Rim2Toggle; float4 _Rim2Color; float _Rim2Power; float _Rim2Intensity; float _Rim2AmbientInfluence;
                float4 _ReflColor; float _ReflIntensity; float _ReflFresnel; float _ReflRoughness; float _HasReflCube;
                float _HasEmissiveTex; float4 _EmissiveColor; float _EmissiveIntensity; float4 _EmissUVScale; float4 _EmissScrollDir; float _EmissScrollSpeed;
                float _BlinkSpeed; float _BlinkPhase; float _BlinkMin; float _BlinkMax;
                float4 _SpecExtraColor; float _SpecIntensity; float _SpecMinPower; float _SpecMaxPower;
                float4 _VtxTintRColor; float _VtxTintRStrength;
                float4 _VtxTintGColor; float _VtxTintGStrength;
                float4 _VtxTintBColor; float _VtxTintBStrength;
                float4 _VtxTintAColor; float _VtxTintAStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                float2 uv2        : TEXCOORD1;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uvMain     : TEXCOORD0;
                float2 uvMask     : TEXCOORD1;
                float2 uvEmiss    : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                float3 normalWS   : TEXCOORD4;
                float4 shadowCoord: TEXCOORD5;
                float4 color      : COLOR0;
                float  fogFactor  : TEXCOORD6;
            };

            inline float3 ApplyVtxTint(float3 baseCol, float4 vcol)
            {
                float mR = saturate(vcol.r) * _VtxTintRStrength;
                float mG = saturate(vcol.g) * _VtxTintGStrength;
                float mB = saturate(vcol.b) * _VtxTintBStrength;
                float mA = saturate(vcol.a) * _VtxTintAStrength;
                if (mR > 0.0) baseCol = lerp(baseCol, baseCol * saturate(_VtxTintRColor.rgb), mR);
                if (mG > 0.0) baseCol = lerp(baseCol, baseCol * saturate(_VtxTintGColor.rgb), mG);
                if (mB > 0.0) baseCol = lerp(baseCol, baseCol * saturate(_VtxTintBColor.rgb), mB);
                if (mA > 0.0) baseCol = lerp(baseCol, baseCol * saturate(_VtxTintAColor.rgb), mA);
                return baseCol;
            }

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 positionWS = TransformObjectToWorld(v.positionOS.xyz);
                float3 normalWS   = TransformObjectToWorldNormal(v.normalOS);
                o.positionCS = TransformWorldToHClip(positionWS);
                o.positionWS = positionWS;
                o.normalWS   = normalWS;
                o.uvMain     = v.uv * _MainUV_ST.xy + _MainUV_ST.zw;
                o.uvMask     = v.uv * _MaskUV_ST.xy + _MaskUV_ST.zw;
                o.uvEmiss    = o.uvMain;
                o.shadowCoord= TransformWorldToShadowCoord(positionWS);
                o.color      = v.color;
                o.fogFactor  = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            float4 frag (Varyings i) : SV_Target
            {
                float4 albedoTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uvMain) * _Color;
                albedoTex.rgb = ApplyVtxTint(albedoTex.rgb, i.color);

                float3 N = normalize(i.normalWS);
                float3 V = normalize(GetCameraPositionWS() - i.positionWS);
                float3 ambient = SampleSH(N);

                Light mainLight = GetMainLight(i.shadowCoord);
                float NdotL = saturate(dot(N, mainLight.direction));
                float shadow = mainLight.shadowAttenuation;
                float3 direct = mainLight.color * (NdotL * shadow);
                float3 litColor = albedoTex.rgb * (direct + ambient);

                float3 maskRGB = lerp(float3(1,1,1), SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uvMask).rgb, saturate(_HasMaskTex));
                float ambLuma = dot(ambient, float3(0.299,0.587,0.114));

                #ifdef _RIM_ON
                float rim1=0, rim2=0;
                if (_Rim1Toggle>0.5) { rim1 = pow(1.0-saturate(dot(N,V)),_Rim1Power); litColor += _Rim1Color.rgb*(_Rim1Intensity*rim1*lerp(1.0, ambLuma,_Rim1AmbientInfluence)); }
                if (_Rim2Toggle>0.5) { rim2 = pow(1.0-saturate(dot(N,V)),_Rim2Power); litColor += _Rim2Color.rgb*(_Rim2Intensity*rim2*lerp(1.0, ambLuma,_Rim2AmbientInfluence)); }
                #endif

                #ifdef _REFL_ON
                float3 R = reflect(-V,N);
                float3 rcUser = SAMPLE_TEXTURECUBE(_ReflCube, sampler_ReflCube,R).rgb;
                float3 rcSky = float3(0,0,0); // URP 환경 큐브 대체
                float3 rc = lerp(rcSky, rcUser, saturate(_HasReflCube));
                float fres = (_ReflFresnel>0)?pow(saturate(1.0-abs(dot(N,V))),_ReflFresnel):1.0;
                float roughAtten = saturate(1.0-_ReflRoughness*0.6);
                float reflMask = lerp(1.0, maskRGB.g, saturate(_HasMaskTex));
                float reflAmbient = lerp(1.0, ambLuma,0.9);
                litColor += rc*_ReflColor.rgb*(_ReflIntensity*roughAtten*reflMask*fres*reflAmbient);
                #endif

                #ifdef _EMISS_ON
                float2 emissUV = i.uvEmiss;
                #ifdef _EMISS_LOCALUV_ON
                    float3 objPos = TransformWorldToObject(i.positionWS).xyz;
                    emissUV = objPos.xy*_EmissUVScale.xy;
                #endif
                #ifdef _EMISS_SCROLL_ON
                    emissUV += _EmissScrollDir.xy*(_EmissScrollSpeed*_Time.y);
                #endif
                float3 emissTexRGB = lerp(float3(1,1,1), SAMPLE_TEXTURE2D(_EmissiveTex,sampler_EmissiveTex,emissUV).rgb,saturate(_HasEmissiveTex));
                float blink=1;
                #ifdef _EMISS_BLINK_ON
                    float sBlink = sin(_Time.y*_BlinkSpeed+_BlinkPhase)*0.5+0.5;
                    blink = lerp(_BlinkMin,_BlinkMax,sBlink);
                #endif
                float emissMask = lerp(1.0,maskRGB.r,saturate(_HasMaskTex));
                litColor += emissTexRGB*_EmissiveColor.rgb*(_EmissiveIntensity*emissMask*blink);
                #endif

                // Spec base
                {
                    float3 L = mainLight.direction;
                    float3 H = normalize(L+V);
                    float nh = saturate(dot(N,H));
                    float specBase = pow(nh, lerp(1.0,128.0,saturate(_Smoothness)));
                    float specMask = lerp(1.0, maskRGB.b, saturate(_HasMaskTex));
                    litColor += mainLight.color*specBase*0.3*specMask;
                }

                #ifdef _SPEC_EXTRA_ON
                {
                    float3 L = mainLight.direction;
                    float3 H = normalize(L+V);
                    float nh = saturate(dot(N,H));
                    float t = saturate(_Smoothness);
                    float powExpX = max(1.0, lerp(_SpecMinPower,_SpecMaxPower,pow(t,1.5)));
                    float specN = pow(nh,powExpX);
                    float normX = (powExpX+8.0)*0.02;
                    float specMask = lerp(1.0, maskRGB.b, saturate(_HasMaskTex));
                    litColor += _SpecExtraColor.rgb*(_SpecIntensity*specMask*(specN*(0.8+normX)))*mainLight.color;
                }
                #endif

                float baseA = saturate(_Opacity);
                float texA = albedoTex.a;
                float a = (_AlphaMode<0.5)?baseA:(_AlphaMode<1.5)?saturate(baseA*texA):(_AlphaMode<2.5)?min(baseA,texA):max(baseA,texA);
                float4 col = float4(litColor,a);
                col.rgb = MixFog(col.rgb,i.fogFactor);
                return col;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ZWrite On
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; };

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings o;
                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(posWS);
                return o;
            }

            float4 ShadowPassFragment(Varyings i):SV_Target { return 0; }
            ENDHLSL
        }
    }

    CustomEditor "TRAIBAG.IndiDolls_custom_shader_UI"
    Fallback Off
}
