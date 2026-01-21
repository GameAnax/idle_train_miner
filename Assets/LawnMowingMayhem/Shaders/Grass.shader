Shader "Custom/Grass"
{
    Properties
    {
        _TintColor1("Tint Color 1", Color) = (1,1,1,1)
        _TintColor2("Tint Color 2", Color) = (1,1,1,1)
        _ColorHeight("Color Height",  Range(0, 1)) = .3
        _CutoutThresh("Cutout Threshold", Range(0.0,1.0)) = 0.2
        _Speed("Speed", Range(0.0,5.0)) = 0.5
        _Amount("Amount", Range(-1.0,1.0)) = 1
        _NoiseScale("Noise Scale",  Range(0, .5)) = .1
        _Height("Grass Height",  Range(0, 2)) = 1
        _MainTex("Main Texture", 2D) = "white" {}
        _CollisionBending("Collision Bending (instanced)", Color) = (0,0,0,0)
    }

    SubShader
    {
        LOD 100
        ZWrite On
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // Global uniforms (Non-instanced)
            float _CutoutThresh;
            // float _Speed;
            float _Amount;
            float _NoiseScale;
            // float _Height;
            float _ColorHeight;

            struct Input
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Required for VS input
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD1; 
                float localPos : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Required for FS input
                SHADOW_COORDS(2)
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _CollisionBending)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _TintColor1)
                UNITY_DEFINE_INSTANCED_PROP(fixed4, _TintColor2)
                UNITY_DEFINE_INSTANCED_PROP(float,  _Speed)
                UNITY_DEFINE_INSTANCED_PROP(float,  _Height)
            UNITY_INSTANCING_BUFFER_END(Props)

            float hash(float n) { return frac(sin(n)*43758.5453); }

            float noise(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f*f*(3.0 - 2.0*f);
                float n = p.x + p.y*57.0 + 113.0*p.z;
                return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
                    lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
                    lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
                        lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
            }

            v2f vert(Input input)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o); // CRITICAL FOR METAL

                o.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                fixed4 collisionBend = UNITY_ACCESS_INSTANCED_PROP(Props, _CollisionBending);
                float tempSpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _Speed);
                float tempHeight = UNITY_ACCESS_INSTANCED_PROP(Props, _Height);
                
                // Apply height scale
                input.vertex.y *= (collisionBend.y > 0 ? collisionBend.y : 1.0); 
                o.localPos = input.vertex.y;

                float3 worldPos = mul(unity_ObjectToWorld, input.vertex).xyz;
                float3 worldPosxz = float3(worldPos.x, 0, worldPos.z);
                
                // Wind and Displacement
                float heightNoise = (noise(worldPosxz * _NoiseScale) + 1.0);
                float worldNoise = noise(worldPosxz * _NoiseScale + float3(1, 0, 0) * _Time.y * tempSpeed) - 0.5;
                float worldNoise2 = noise(worldPosxz * _NoiseScale + float3(1, 0, 0) * _Time.y * tempSpeed + 100.0) - 0.5;
                
                input.vertex.y = input.vertex.y * heightNoise * tempHeight;
                float yy = input.vertex.y * input.vertex.y * _Amount;
                
                input.vertex.x += (worldNoise + collisionBend.x * 2.0) * yy;
                input.vertex.z += (worldNoise2 + collisionBend.z * 2.0) * yy;
                input.vertex.y -= input.vertex.y * (abs(collisionBend.x) + abs(collisionBend.z)) * 0.7;

                o.vertex = UnityObjectToClipPos(input.vertex);
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); // CRITICAL FOR METAL

                fixed4 t1 = UNITY_ACCESS_INSTANCED_PROP(Props, _TintColor1);
                fixed4 t2 = UNITY_ACCESS_INSTANCED_PROP(Props, _TintColor2);

                fixed4 tex = tex2D(_MainTex, i.uv);
                clip(tex.a - _CutoutThresh); // Handle cutout transparency

                fixed4 colr = lerp(t1, t2, saturate(i.localPos * _ColorHeight));
                return tex * colr;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}