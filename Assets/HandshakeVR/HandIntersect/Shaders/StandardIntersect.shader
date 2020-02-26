Shader "Custom/StandardIntersect" 
{
    Properties
    {
		_MainTex("Albedo Texture", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0, 1)) = 0.5
		_Metallic("Metallic", Range(0, 1)) = 0.0
		[MaterialToggle] _isLeftHand("Is Left Hand?", Int) = 0

		// intersect variables
		[HDR]
		_Color("Base Color", Color) = (1, 1, 1, 0.25)
		
		[HDR]
		_HighlightColor("Intersection Color", Color) = (1, 1, 1, .5)		
		_IntersectionThreshold("Intersection Threshold ", Range(0, 1)) = 0.25
		_SilhouetteEnhancement("Silhouette Enhancement", Range(-2, 2)) = 1

		_XSpeed("X Speed", Range(-10, 10)) = 1
		_YSpeed("Y Speed", Range(-10, 10)) = 1
	}
    SubShader
    {
		Tags
		{ 
			"Queue" = "Transparent" 
			"IgnoreProjector" = "True" 
			"RenderType" = "Transparent" 
		}

		ZWrite On

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#include "Assets/LeapMotion/Core/Resources/LeapCG.cginc"
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		int _isLeftHand;
		void vert(inout appdata_full v) {
			v.vertex = LeapGetLateVertexPos(v.vertex, _isLeftHand);
		}

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) /** _Color*/;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG

        Pass
        {
			//Blend SrcAlpha OneMinusSrcAlpha
			Blend One One
            ZWrite Off
 
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
			#include "Assets/LeapMotion/Core/Resources/LeapCG.cginc"

            uniform sampler2D _CameraDepthTexture;
					
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;

            float4 _Color;
            float4 _HighlightColor;

            float _IntersectionThreshold;
			float _SilhouetteEnhancement;
			
			float _XSpeed;
			float _YSpeed;

			float4 _MainTex_ST;

            struct v2f
            {
                float4 pos : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
				float3 viewDir : TEXCOORD2;
				float4 projPos : TEXCOORD4;
            };
 
			int _isLeftHand;
            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(LeapGetLateVertexPos(v.vertex, _isLeftHand));
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				o.normal = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
				o.viewDir = normalize(_WorldSpaceCameraPos - mul(unity_ObjectToWorld, v.vertex).xyz);

				o.projPos = ComputeScreenPos(o.pos);
				UNITY_TRANSFER_FOG(o, o.pos);

                return o;
            }
 
            half4 frag(v2f i) : COLOR
            {
				float2 texCoord = i.texcoord + float2(_XSpeed * _Time.x, _YSpeed * _Time.x);

				float heightSampleCenter = tex2D(_MainTex, texCoord).r;
				float heightSampleRight = tex2D(_MainTex, texCoord + float2(_MainTex_TexelSize.x, 0)).r;
				float heightSampleUp = tex2D(_MainTex, texCoord + float2(0, _MainTex_TexelSize.y)).r;

				float sampleDeltaRight = heightSampleRight - heightSampleCenter;
				float sampleDeltaUp = heightSampleUp - heightSampleCenter;

				float3 normal = i.normal;

				//half4 col = tex2D(_MainTex, texCoord) * _Color;
				half4 col = _Color;

				float alpha = col.a;

				float3 normalDirection = normalize(i.normal);
				float3 viewDirection = normalize(i.viewDir);

				float opacity = min(1.0, _Color.a / pow(abs(dot(viewDirection, normalDirection)), _SilhouetteEnhancement));
				col.a = opacity;

				float world_z = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, 
				UNITY_PROJ_COORD(i.projPos)
				));

				float projPos = i.projPos.w;
				float distance = world_z - projPos;
				float multiplier = pow(1 - saturate(distance / _IntersectionThreshold), 3);

				return lerp(col, _HighlightColor, multiplier);
            }
 
            ENDCG
        }
    }
    FallBack "Diffuse"
}
