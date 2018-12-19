// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

// Use the non-batching version of this shader if you are using
// local mode, and are seeing the textures move when you zoom in
// due to dynamic batching. (Dynamic batching converts local vertex
// data into world space, making local vertex position data unavailable
// to the shader.)

Shader "MixTerrain/VertexHeightSplat" {
	Properties {
		// Triplanar space, for UI
		[HideInInspector] _TriplanarSpace("Triplanar Space", Float) = 0.0

		_TexPower("Texture Power", Range(0.0, 20.0)) = 10.0
		//_HeightblendFactor("Heightmap Blending Factor", Float) = 0.05

		_Color ("Color", Color) = (1.0,1.0,1.0,1.0)

		// --- ALBEDO
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MainTex1 ("Albedo2 (RGB)", 2D) = "white" {}
		_MainTex2 ("Albedo3 (RGB)", 2D) = "white" {}
		_MainTex3 ("Albedo4 (RGB)", 2D) = "white" {}

		//_MainTex3 ("Albedo4 (RGB)", 2D) = "white" {}
		
		//_VertexColorStrength("Vertex Color Strength", Range(0.0,1.0)) = 1.0
		
		_Glossiness ("Smoothness", Range(0.0,1.0)) = 0.5
		//_Glossiness1 ("Smoothness 2", Range(0.0,1.0)) = 0.5

		[Gamma] _Metallic ("Metallic", Range(0.0,1.0)) = 0.0
		//[Gamma] _Metallic1 ("Metallic 2", Range(0.0,1.0)) = 0.0

		//_MetallicGlossMap("Metallic", 2D) = "black" {}
		//_UsingMetallicGlossMap("Using Metallic Gloss Map", float) = 0.0
		//[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		//[ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

		_ColorGlow ("Color glow", Range(0.0,1.0)) = 0.2
		[Gamma] _ColorMetallic ("Metallic", Range(0.0,1.0)) = 0.0
		_ColorGlossiness ("Smoothness", Range(0.0,1.0)) = 0.5
		_BumpMapColor("Normal Map Color", 2D) = "bump" {}

		// --- BUMP
		_BumpScale("Bump Scale", Float) = 1.0
		_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale1("Bump Scale 2", Float) = 1.0
		_BumpMap1("Normal Map 2", 2D) = "bump" {}
		_BumpScale2("Bump Scale 3", Float) = 1.0
		_BumpMap2("Normal Map 3", 2D) = "bump" {}
		_BumpScale3("Bump Scale 4", Float) = 1.0
		_BumpMap3("Normal Map 4", 2D) = "bump" {}


		// --- PARALLAX
		_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
		_ParallaxTextureHeight ("Height Texture", Range (0.005, 1)) = 0.5
		_ParallaxMap ("Height Map", 2D) = "black" {}

		_Parallax1 ("Height Scale 2", Range (0.005, 0.08)) = 0.02
		_ParallaxTextureHeight1  ("Height Texture 2", Range (0.005, 1)) = 0.5
		_ParallaxMap1 ("Height Map 2", 2D) = "black" {}

		_Parallax2 ("Height Scale 3", Range (0.005, 0.08)) = 0.02
		_ParallaxTextureHeight2  ("Height Texture 3", Range (0.005, 1)) = 0.5
		_ParallaxMap2 ("Height Map 3", 2D) = "black" {}

		_Parallax3 ("Height Scale 4", Range (0.005, 0.08)) = 0.02
		_ParallaxTextureHeight3  ("Height Texture 4", Range (0.005, 1)) = 0.5
		_ParallaxMap3 ("Height Map 4", 2D) = "black" {}

		//_OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
		//_OcclusionMap("Occlusion", 2D) = "white" {}
		//_OcclusionMap1("Occlusion 2", 2D) = "white" {}

		//_EmissionColor("Emission Color", Color) = (0.0,0.0,0.0)
		//_EmissionMap("Emission", 2D) = "white" {}

		//_DetailMask("Detail Mask", 2D) = "white" {}

		_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
		_DetailNormalMapScale("Scale", Float) = 1.0
		_DetailNormalMap("Normal Map", 2D) = "bump" {}

		_DetailAlbedoMap1("Detail Albedo x2 2", 2D) = "grey" {}
		_DetailNormalMapScale1("Scale 2", Float) = 1.0
		_DetailNormalMap1("Normal Map 2", 2D) = "bump" {}

		// UI-only data
		[HideInInspector] _EmissionScaleUI("Scale", Float) = 0.0
		[HideInInspector] _EmissionColorUI("Color", Color) = (1.0,1.0,1.0)

	}
	SubShader {
		Tags {
			"RenderType"="Opaque"
			"PerformanceChecks"="False"
		}
		LOD 300
		
		CGPROGRAM
		 #pragma target 3.5
		//#pragma target 3.0

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert fullforwardshadows nodynlightmap
		//#pragma shader_feature _UVFREE_LOCAL
		//#pragma shader_feature _EMISSION
		//#pragma shader_feature _METALLICGLOSSMAP 
		//#pragma shader_feature _DETAIL
		//#pragma shader_feature _OCCLUSION
		//#pragma shader_feature _PARALLAXMAP
		//#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		//#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#include "UnityCG.cginc"

		// Uncomment to enable experimental feature which flips
		// backward textures. Note: Causes some normals to be flipped.
		// #define _UVFREE_FLIP_BACKWARD_TEXTURES
		
		// Comment out following line to omit vertex colors
		#define _UVFREE_VERTEX_COLOR
		
		// Instanced Properties
		// https://docs.unity3d.com/Manual/GPUInstancing.html
		//UNITY_INSTANCING_BUFFER_START (MyProperties)
		//UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		//#define _Color_arr MyProperties
		//UNITY_INSTANCING_BUFFER_END(MyProperties)

		// Non-instanced properties
		half _TexPower;
		//half _HeightblendFactor;

		//sampler2D _MainTex;
		
		//sampler2D _MainTex1;
		//sampler2D _MainTex2;
				//sampler2D _MainTex3;

		//sampler2D _BumpMap;
		//sampler2D _BumpMap1;
		//sampler2D _BumpMap2;

		//Albedo
		float4 _MainTex_ST;
		UNITY_DECLARE_TEX2D(_MainTex);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_MainTex1);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_MainTex2);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_MainTex3);

		//Bump
		half _BumpScale;
		half _BumpScale1;
		half _BumpScale2;
		half _BumpScale3;
		UNITY_DECLARE_TEX2D(_BumpMap);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap1);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap2);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_BumpMap3);

		//Parallax
		half _Parallax;
		half _Parallax1;
		half _Parallax2;
		half _Parallax3;
		half _ParallaxTextureHeight;
		half _ParallaxTextureHeight1;
		half _ParallaxTextureHeight2;
		half _ParallaxTextureHeight3;
		UNITY_DECLARE_TEX2D(_ParallaxMap);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_ParallaxMap1);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_ParallaxMap2);
		UNITY_DECLARE_TEX2D_NOSAMPLER(_ParallaxMap3);

		//#ifdef _PARALLAXMAP
		//	sampler2D _ParallaxMap;
			

		//	sampler2D _ParallaxMap1;

	
	//#endif

		//sampler2D _BumpMap3;

		sampler2D _BumpMapColor;
		float4 _BumpMapColor_ST;

		


		fixed _Metallic;
		fixed _Glossiness;
		fixed _ColorMetallic;
		fixed _ColorGlossiness;
		fixed _ColorGlow;

		//#ifdef _DETAIL
			sampler2D _DetailAlbedoMap;
			sampler2D _DetailAlbedoMap1;
			float4 _DetailAlbedoMap_ST;

			//sampler2D _DetailMask;
			sampler2D _DetailNormalMap;
			sampler2D _DetailNormalMap1;

			half _DetailNormalMapScale;
		//#endif

		//#ifdef _METALLICGLOSSMAP
		//	sampler2D _MetallicGlossMap;
		//	fixed _UsingMetallicGlossMap;		
		//#endif

		//#ifdef _OCCLUSION
		//	sampler2D _OcclusionMap;
		//	fixed _OcclusionStrength;
			
		//#endif



		//#ifdef _EMISSION
		//	sampler2D _EmissionMap;
		//	half4 _EmissionColor;
		//#endif

		//#ifdef	_UVFREE_VERTEX_COLOR
		//	fixed _VertexColorStrength;
		//#endif

		struct Input {
		
			fixed3 powerNormal;
			
			//#ifdef _UVFREE_FLIP_BACKWARD_TEXTURES
			//	fixed3 normal;
			//#endif
           // float2 texcoord : TEXCOORD0;
            float4 texChoice; // : TEXCOORD0;

			//float2 //uv2_texcoord;
			//float2 texcoord2 : TEXCOORD2;

			float3 worldPos;
			fixed3 viewDirForParallax;

			//#ifdef _UVFREE_VERTEX_COLOR
				fixed4 color:COLOR;
			//#endif
			
			UNITY_VERTEX_INPUT_INSTANCE_ID	
		};


		/*float heightblend(float input1, float height1, float input2, float height2)
		{
			float height_start = max(height1, height2) - _HeightblendFactor;
			float level1 = max(height1 - height_start, 0);
			float level2 = max(height2 - height_start, 0);
			return ((input1 * level1) + (input2 * level2)) / (level1 + level2);
		}*/


		void vert (inout appdata_full v, out Input o) {
		
			UNITY_INITIALIZE_OUTPUT(Input,o);

			/*#ifdef _UVFREE_LOCAL
				#ifdef _UVFREE_FLIP_BACKWARD_TEXTURES
					o.normal = v.normal;
				#endif
				o.powerNormal = pow(abs(v.normal), _TexPower);	
				o.powerNormal = max(o.powerNormal, 0.0001);
				o.powerNormal /= dot(o.powerNormal, 1.0);
				
				v.tangent.xyz = 
					cross(v.normal, fixed3(0.0,sign(v.normal.x),0.0)) * (o.powerNormal.x)
				  + cross(v.normal, fixed3(0.0,0.0,sign(v.normal.y))) * (o.powerNormal.y)
				  + cross(v.normal, fixed3(0.0,sign(v.normal.z),0.0)) * (o.powerNormal.z)
				;
				
				v.tangent.w = 
					(-(v.normal.x) * (o.powerNormal.x))
				  +	(-(v.normal.y) * (o.powerNormal.y))
				  +	(-(v.normal.z) * (o.powerNormal.z))
				;
				
			#else*/
				fixed3 worldNormal = normalize(mul(unity_ObjectToWorld, fixed4(v.normal, 0.0)).xyz);
				
				//#ifdef _UVFREE_FLIP_BACKWARD_TEXTURES
				//	o.normal = worldNormal;
				//#endif
				
				o.powerNormal = pow(abs(worldNormal), _TexPower);
				o.powerNormal = max(o.powerNormal, 0.0001);
				o.powerNormal /= dot(o.powerNormal, 1.0);
								
				v.tangent.xyz = 
					cross(v.normal, mul(unity_WorldToObject,fixed4(0.0,sign(worldNormal.x),0.0,0.0)).xyz * (o.powerNormal.x))
				  + cross(v.normal, mul(unity_WorldToObject,fixed4(0.0,0.0,sign(worldNormal.y),0.0)).xyz * (o.powerNormal.y))
				  + cross(v.normal, mul(unity_WorldToObject,fixed4(0.0,sign(worldNormal.z),0.0,0.0)).xyz * (o.powerNormal.z))
				;
				
				v.tangent.w = 
					(-(worldNormal.x) * (o.powerNormal.x))
				  +	(-(worldNormal.y) * (o.powerNormal.y))
				  +	(-(worldNormal.z) * (o.powerNormal.z))
				;
				
				o.texChoice.x = v.texcoord.x;
				o.texChoice.y = v.texcoord.y;
				o.texChoice.z = v.texcoord1.x;
				o.texChoice.w = v.texcoord1.y;

				//o.texcoord.x = v.texcoord.x;
				//o.texcoord.y = v.texcoord.y;

				//o.texcoord1.x = v.texcoord1.x;
				//o.texcoord1.y = v.texcoord1.y;

				//o.texcoord2.x = v.texcoord2.x;
				//o.texcoord2.y = v.texcoord2.y;

			//#endif

			//#ifdef _PARALLAXMAP
        		TANGENT_SPACE_ROTATION;
        		o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
        	//#endif
		}
		
		void surf (Input IN, inout SurfaceOutputStandard o) {
		
			float3 pos = IN.worldPos;
			
			float2 posX = IN.worldPos.zy;
			float2 posY = IN.worldPos.xz;
			float2 posZ = float2(-IN.worldPos.x, IN.worldPos.y);				
			
			float2 xUV = posX * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 yUV = posY * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 zUV = posZ * _MainTex_ST.xy + _MainTex_ST.zw;

			float2 xUV1 = xUV;
			float2 yUV1 = yUV;
			float2 zUV1 = zUV;

			float2 xUV2 = xUV;
			float2 yUV2 = yUV;
			float2 zUV2 = zUV;

			float2 xUV3 = xUV;
			float2 yUV3 = yUV;
			float2 zUV3 = zUV;

			float2 xUVColor = posX * _BumpMapColor_ST.xy + _BumpMapColor_ST.zw;
			float2 yUVColor = posY * _BumpMapColor_ST.xy + _BumpMapColor_ST.zw;
			float2 zUVColor = posZ * _BumpMapColor_ST.xy + _BumpMapColor_ST.zw;

			// ---------------------------------------------------------------
			// -------------------- PARALLAX ---------------------------------
			// ---------------------------------------------------------------

			//===================== 1
				half parallaxX = UNITY_SAMPLE_TEX2D(_ParallaxMap, xUV).r; //tex2D (_ParallaxMap, xUV).r;
				half parallaxY = UNITY_SAMPLE_TEX2D(_ParallaxMap, yUV).g; //tex2D (_ParallaxMap, yUV).g;
				half parallaxZ = UNITY_SAMPLE_TEX2D(_ParallaxMap, zUV).b; //tex2D (_ParallaxMap, zUV).b;
				
				half parallax = 
					parallaxX * IN.powerNormal.x
				  + parallaxY * IN.powerNormal.y
				  + parallaxZ * IN.powerNormal.z
				  ;
				float2 parallaxOffset = ParallaxOffset (parallax, _Parallax, IN.viewDirForParallax);
				xUV += parallaxOffset;
				yUV += parallaxOffset;
				zUV += parallaxOffset;

			//===================== 2
				half parallaxX1 = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap1, _ParallaxMap, xUV1).r; //tex2D (_ParallaxMap1, xUV1).r;
				half parallaxY1 = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap1, _ParallaxMap, yUV1).g; //tex2D (_ParallaxMap1, yUV1).g;
				half parallaxZ1 = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap1, _ParallaxMap, zUV1).b; //tex2D (_ParallaxMap1, zUV1).b;
				
				half parallax1 = 
					parallaxX1 * IN.powerNormal.x
				  + parallaxY1 * IN.powerNormal.y
				  + parallaxZ1 * IN.powerNormal.z
				  ;
				float2 parallaxOffset1 = ParallaxOffset (parallax1, _Parallax1, IN.viewDirForParallax);
				xUV1 += parallaxOffset1;
				yUV1 += parallaxOffset1;
				zUV1 += parallaxOffset1;

			//===================== 3
				half parallaxX2 = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap2, _ParallaxMap, xUV2).r; //tex2D (_ParallaxMap1, xUV1).r;
				half parallaxY2 = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap2, _ParallaxMap, yUV2).g; //tex2D (_ParallaxMap1, yUV1).g;
				half parallaxZ2 = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap2, _ParallaxMap, zUV2).b; //tex2D (_ParallaxMap1, zUV1).b;
				
				half parallax2 = 
					parallaxX2 * IN.powerNormal.x
				  + parallaxY2 * IN.powerNormal.y
				  + parallaxZ2 * IN.powerNormal.z
				  ;
				float2 parallaxOffset2 = ParallaxOffset (parallax2, _Parallax2, IN.viewDirForParallax);
				xUV2 += parallaxOffset2;
				yUV2 += parallaxOffset2;
				zUV2 += parallaxOffset2;

			//===================== 4
				half parallaxX3 = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap3, _ParallaxMap, xUV3).r; //tex2D (_ParallaxMap1, xUV1).r;
				half parallaxY3 = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap3, _ParallaxMap, yUV3).g; //tex2D (_ParallaxMap1, yUV1).g;
				half parallaxZ3 = UNITY_SAMPLE_TEX2D_SAMPLER(_ParallaxMap3, _ParallaxMap, zUV3).b; //tex2D (_ParallaxMap1, zUV1).b;
				
				half parallax3 = 
					parallaxX3 * IN.powerNormal.x
				  + parallaxY3 * IN.powerNormal.y
				  + parallaxZ3 * IN.powerNormal.z
				  ;
				float2 parallaxOffset3 = ParallaxOffset (parallax3, _Parallax3, IN.viewDirForParallax);
				xUV3 += parallaxOffset3;
				yUV3 += parallaxOffset3;
				zUV3 += parallaxOffset3;

			// ---------------------------------------------------------------
			// -------------------- ALBEDO -----------------------------------
			// ---------------------------------------------------------------
			
			//===================== 1
			fixed4 texX = UNITY_SAMPLE_TEX2D(_MainTex, xUV); //_MainTex.Sample(sampler_MainTex, xUV); //tex2D(_MainTex, xUV);
			fixed4 texY = UNITY_SAMPLE_TEX2D(_MainTex, yUV);//_MainTex.Sample(sampler_MainTex, yUV); //tex2D(_MainTex, yUV);
			fixed4 texZ = UNITY_SAMPLE_TEX2D(_MainTex, zUV);//_MainTex.Sample(sampler_MainTex, zUV); //tex2D(_MainTex, zUV);			
			
			fixed4 tex = 
			    texX * IN.powerNormal.x
			  + texY * IN.powerNormal.y
			  + texZ * IN.powerNormal.z;

			fixed3 albedo = max(fixed3(0.0, 0.0, 0.0), tex.rgb ); //* tint.rgb

			//===================== 2
			fixed4 texX1 = UNITY_SAMPLE_TEX2D_SAMPLER(_MainTex1, _MainTex, xUV1); //_MainTex1.Sample(sampler_MainTex, xUV1); //tex2D(_MainTex1, xUV1);
			fixed4 texY1 = UNITY_SAMPLE_TEX2D_SAMPLER(_MainTex1, _MainTex, yUV1); //_MainTex1.Sample(sampler_MainTex, yUV1); //tex2D(_MainTex1, yUV1);
			fixed4 texZ1 = UNITY_SAMPLE_TEX2D_SAMPLER(_MainTex1, _MainTex, zUV1); //_MainTex1.Sample(sampler_MainTex, zUV1); //tex2D(_MainTex1, zUV1);			
			
			fixed4 tex1 = 
			    texX1 * IN.powerNormal.x
			  + texY1 * IN.powerNormal.y
			  + texZ1 * IN.powerNormal.z;
			
			fixed3 albedo1 = max(fixed3(0.0, 0.0, 0.0), tex1.rgb ); //* tint.rgb

			//===================== 3
			fixed4 texX2 = UNITY_SAMPLE_TEX2D_SAMPLER(_MainTex2, _MainTex, xUV2); //_MainTex2.Sample(sampler_MainTex, xUV2); //tex2D(_MainTex2, xUV2);
			fixed4 texY2 = UNITY_SAMPLE_TEX2D_SAMPLER(_MainTex2, _MainTex, yUV2); //_MainTex2.Sample(sampler_MainTex, xUV2); //tex2D(_MainTex2, yUV2);
			fixed4 texZ2 = UNITY_SAMPLE_TEX2D_SAMPLER(_MainTex2, _MainTex, zUV2); //_MainTex2.Sample(sampler_MainTex, xUV2); //tex2D(_MainTex2, zUV2);			
			
			fixed4 tex2 = 
			    texX2 * IN.powerNormal.x
			  + texY2 * IN.powerNormal.y
			  + texZ2 * IN.powerNormal.z;
			
			fixed3 albedo2 = max(fixed3(0.0, 0.0, 0.0), tex2.rgb ); //* tint.rgb

			//===================== 4
			fixed4 texX3 = UNITY_SAMPLE_TEX2D_SAMPLER(_MainTex3, _MainTex, xUV3); //_MainTex2.Sample(sampler_MainTex, xUV2); //tex2D(_MainTex2, xUV2);
			fixed4 texY3 = UNITY_SAMPLE_TEX2D_SAMPLER(_MainTex3, _MainTex, yUV3); //_MainTex2.Sample(sampler_MainTex, xUV2); //tex2D(_MainTex2, yUV2);
			fixed4 texZ3 = UNITY_SAMPLE_TEX2D_SAMPLER(_MainTex3, _MainTex, zUV3); //_MainTex2.Sample(sampler_MainTex, xUV2); //tex2D(_MainTex2, zUV2);			
			
			fixed4 tex3 = 
			    texX3 * IN.powerNormal.x
			  + texY3 * IN.powerNormal.y
			  + texZ3 * IN.powerNormal.z;
			
			fixed3 albedo3 = max(fixed3(0.0, 0.0, 0.0), tex3.rgb );


			// DIFFUSE DETAIL
			//
			

			//#ifdef _DETAIL


			//===================== 1
				//fixed detailMaskX = tex2D(_DetailMask, xUV).a;
				//fixed detailMaskY = tex2D(_DetailMask, yUV).a;
				//fixed detailMaskZ = tex2D(_DetailMask, zUV).a;
				
				fixed detailMask = 1;
				//	detailMaskX * IN.powerNormal.x
				//  + detailMaskY * IN.powerNormal.y
				//  + detailMaskZ * IN.powerNormal.z;
				
				float2 xUVDetail = pos.zy * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float2 yUVDetail = pos.xz * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float2 zUVDetail = float2(-pos.x, pos.y) * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				
				float2 xUVDetail1 = xUVDetail; //pos.zy * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float2 yUVDetail1 = yUVDetail; //pos.xz * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float2 zUVDetail1 = zUVDetail; //float2(-pos.x, pos.y) * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				
				//#ifdef _UVFREE_FLIP_BACKWARD_TEXTURES
				//	xUVDetail.x *= powerSign.x;
				//	zUVDetail.x *= powerSign.z;
				//	yUVDetail.y *= powerSign.y;
				//#endif
							
				//#ifdef _PARALLAXMAP
					xUVDetail += parallaxOffset;
					yUVDetail += parallaxOffset;
					zUVDetail += parallaxOffset;
				//#endif
				
				fixed3 detailAlbedoX = tex2D (_DetailAlbedoMap, xUVDetail).rgb;
				fixed3 detailAlbedoY = tex2D (_DetailAlbedoMap, yUVDetail).rgb;
				fixed3 detailAlbedoZ = tex2D (_DetailAlbedoMap, zUVDetail).rgb;
				
				fixed3 detailAlbedo = 
					detailAlbedoX * IN.powerNormal.x
				  + detailAlbedoY * IN.powerNormal.y
				  + detailAlbedoZ * IN.powerNormal.z;
				 
				albedo *= LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, detailMask);

				//===================== 2

				
					xUVDetail1 += parallaxOffset1;
					yUVDetail1 += parallaxOffset1;
					zUVDetail1 += parallaxOffset1;

				fixed3 detailAlbedoX1 = tex2D (_DetailAlbedoMap1, xUVDetail1).rgb;
				fixed3 detailAlbedoY1 = tex2D (_DetailAlbedoMap1, yUVDetail1).rgb;
				fixed3 detailAlbedoZ1 = tex2D (_DetailAlbedoMap1, zUVDetail1).rgb;
				
				fixed3 detailAlbedo1 = 
					detailAlbedoX1 * IN.powerNormal.x
				  + detailAlbedoY1 * IN.powerNormal.y
				  + detailAlbedoZ1 * IN.powerNormal.z;
				 
				albedo1 *= LerpWhiteTo (detailAlbedo1 * unity_ColorSpaceDouble.rgb, detailMask);
				

			//#endif*/
			fixed firstMix =	(parallax * IN.texChoice.x  *_ParallaxTextureHeight);
			fixed secondMix =	(parallax1* IN.texChoice.y  *_ParallaxTextureHeight1);
			fixed thirdMix =	(parallax2* IN.texChoice.z *_ParallaxTextureHeight2);
			fixed fourthMix =	(parallax3* IN.texChoice.w *_ParallaxTextureHeight3);

			fixed totalMix = firstMix+secondMix+thirdMix+fourthMix+IN.color.a; //thirdMix+fourthMix
			
			fixed firstPercent = firstMix/totalMix;
			fixed secondPercent = secondMix/totalMix;
			fixed thirdPercent = thirdMix/totalMix;
			fixed fourthPercent = fourthMix/totalMix;

			fixed colorPercent = IN.color.a/totalMix;

			//float mixR = (parallax*IN.texcoord1.x*_ParallaxTextureHeight) 
			//	/ ((parallax*IN.texcoord1.x*_ParallaxTextureHeight) + (parallax1*IN.texcoord1.y *_ParallaxTextureHeight1));

			//float mixG = (parallax1*IN.texcoord1.y*_ParallaxTextureHeight1) 
			//	/ ((parallax*IN.texcoord1.x*_ParallaxTextureHeight) + (parallax1*IN.texcoord1.y *_ParallaxTextureHeight1));


			fixed3 mixedDiffuse;
			mixedDiffuse = 0.0f;
			mixedDiffuse += firstPercent * albedo; //IN.texcoord1.x * albedo; //IN.color.r * albedo; //IN.uv2.x * albedo;
			mixedDiffuse += secondPercent * albedo1; //IN.texcoord1.y * albedo1; //IN.color.g * albedo1; //IN.uv2.y * albedo1;
			mixedDiffuse += thirdPercent * albedo2;
			mixedDiffuse += fourthPercent * albedo3;
			mixedDiffuse += colorPercent * IN.color.rgb;

			o.Albedo = mixedDiffuse; //IN.color;//albedo1;
			//albedo
			o.Alpha = 1; //tex.a*IN.color.r+tex1.a*IN.color.g; //tex.a*IN.uv2.x+tex1.a*IN.uv2.y;

			//tex.a * tint.a;
						
			// ---------------------------------------------------------------
			// -------------------- BUMP -------------------------------------
			// ---------------------------------------------------------------
			
			//===================== 1
			fixed3 bumpX = UnpackScaleNormal(UNITY_SAMPLE_TEX2D(_BumpMap, xUV), _BumpScale); //UnpackScaleNormal(tex2D(_BumpMap, xUV), _BumpScale);
			fixed3 bumpY = UnpackScaleNormal(UNITY_SAMPLE_TEX2D(_BumpMap, yUV), _BumpScale); //UnpackScaleNormal(tex2D(_BumpMap, yUV), _BumpScale);
			fixed3 bumpZ = UnpackScaleNormal(UNITY_SAMPLE_TEX2D(_BumpMap, zUV), _BumpScale); //UnpackScaleNormal(tex2D(_BumpMap, zUV), _BumpScale);
			
			fixed3 bump = 
			    bumpX * IN.powerNormal.x
			  + bumpY * IN.powerNormal.y
			  + bumpZ * IN.powerNormal.z;

			//===================== 2
			fixed3 bumpX1 = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap1, _BumpMap, xUV1), _BumpScale1); //UnpackScaleNormal(tex2D(_BumpMap1, xUV1), _BumpScale);
			fixed3 bumpY1 = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap1, _BumpMap, yUV1), _BumpScale1); //UnpackScaleNormal(tex2D(_BumpMap1, yUV1), _BumpScale);
			fixed3 bumpZ1 = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap1, _BumpMap, zUV1), _BumpScale1); //UnpackScaleNormal(tex2D(_BumpMap1, zUV1), _BumpScale);
			
			fixed3 bump1 = 
			    bumpX1 * IN.powerNormal.x
			  + bumpY1 * IN.powerNormal.y
			  + bumpZ1 * IN.powerNormal.z;

			//===================== 3
			fixed3 bumpX2 = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap2, _BumpMap, xUV2), _BumpScale2); //UnpackScaleNormal(tex2D(_BumpMap2, xUV2), _BumpScale);
			fixed3 bumpY2 = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap2, _BumpMap, yUV2), _BumpScale2); //UnpackScaleNormal(tex2D(_BumpMap2, yUV2), _BumpScale);
			fixed3 bumpZ2 = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap2, _BumpMap, zUV2), _BumpScale2); //UnpackScaleNormal(tex2D(_BumpMap2, zUV2), _BumpScale);
			
			fixed3 bump2 = 
			    bumpX2 * IN.powerNormal.x
			  + bumpY2 * IN.powerNormal.y
			  + bumpZ2 * IN.powerNormal.z;

			//===================== 4
			fixed3 bumpX3 = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap3, _BumpMap, xUV3), _BumpScale3); //UnpackScaleNormal(tex2D(_BumpMap2, xUV2), _BumpScale);
			fixed3 bumpY3 = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap3, _BumpMap, yUV3), _BumpScale3); //UnpackScaleNormal(tex2D(_BumpMap2, yUV2), _BumpScale);
			fixed3 bumpZ3 = UnpackScaleNormal(UNITY_SAMPLE_TEX2D_SAMPLER(_BumpMap3, _BumpMap, zUV3), _BumpScale3); //UnpackScaleNormal(tex2D(_BumpMap2, zUV2), _BumpScale);
			
			fixed3 bump3 = 
			    bumpX3 * IN.powerNormal.x
			  + bumpY3 * IN.powerNormal.y
			  + bumpZ3 * IN.powerNormal.z;

			//===================== Color
			fixed3 bumpXColor = UnpackScaleNormal(tex2D(_BumpMapColor, xUVColor), _BumpScale);
			fixed3 bumpYColor = UnpackScaleNormal(tex2D(_BumpMapColor, yUVColor), _BumpScale);
			fixed3 bumpZColor = UnpackScaleNormal(tex2D(_BumpMapColor, zUVColor), _BumpScale);
			
			fixed3 bumpColor = 
			    bumpXColor * IN.powerNormal.x
			  + bumpYColor * IN.powerNormal.y
			  + bumpZColor * IN.powerNormal.z;

			// NORMAL DETAIL
			//
			//#ifdef _DETAIL

			//===================== 1
				fixed3 detailNormalTangentX = UnpackScaleNormal(tex2D (_DetailNormalMap, xUVDetail), _DetailNormalMapScale);
				fixed3 detailNormalTangentY = UnpackScaleNormal(tex2D (_DetailNormalMap, yUVDetail), _DetailNormalMapScale);
				fixed3 detailNormalTangentZ = UnpackScaleNormal(tex2D (_DetailNormalMap, zUVDetail), _DetailNormalMapScale);
				
				fixed3 detailNormalTangent = 
					detailNormalTangentX * IN.powerNormal.x
				  + detailNormalTangentY * IN.powerNormal.y
				  + detailNormalTangentZ * IN.powerNormal.z;
				  
				bump = lerp(
					bump,
					BlendNormals(bump, detailNormalTangent),
					detailMask);

			//===================== 2
				fixed3 detailNormalTangentX1 = UnpackScaleNormal(tex2D (_DetailNormalMap1, xUVDetail1), _DetailNormalMapScale);
				fixed3 detailNormalTangentY1 = UnpackScaleNormal(tex2D (_DetailNormalMap1, yUVDetail1), _DetailNormalMapScale);
				fixed3 detailNormalTangentZ1 = UnpackScaleNormal(tex2D (_DetailNormalMap1, zUVDetail1), _DetailNormalMapScale);
				
				fixed3 detailNormalTangent1 = 
					detailNormalTangentX1 * IN.powerNormal.x
				  + detailNormalTangentY1 * IN.powerNormal.y
				  + detailNormalTangentZ1 * IN.powerNormal.z;
				  
				bump1 = lerp(
					bump1,
					BlendNormals(bump1, detailNormalTangent1),
					detailMask);
			


			//#endif*/
			fixed3 finalBump =
					firstPercent * bump
				  + secondPercent * bump1
				  + thirdPercent * bump2
				  + fourthPercent * bump3
				  + colorPercent * bumpColor;
					//IN.uv2.x * bump
				  //+ IN.uv2.y * bump1;

			o.Normal = normalize(finalBump); //UnpackNormal(finalBump);

			// METALLIC/GLOSS
			//
			
			/*fixed2 mg = fixed2(
				_Metallic,
				_Glossiness
			);

			//#ifdef _METALLICGLOSSMAP
				fixed2 mgX = lerp(mg, tex2D(_MetallicGlossMap, xUV).ra, _UsingMetallicGlossMap);
				fixed2 mgY = lerp(mg, tex2D(_MetallicGlossMap, yUV).ra, _UsingMetallicGlossMap);
				fixed2 mgZ = lerp(mg, tex2D(_MetallicGlossMap, zUV).ra, _UsingMetallicGlossMap);				  

				mg = 
					mgX * IN.powerNormal.x
				  + mgY * IN.powerNormal.y
				  + mgZ * IN.powerNormal.z;
				  */

			//#endif
			o.Metallic = colorPercent*_ColorMetallic+(firstPercent+secondPercent)*_Metallic; //mg.x;
			o.Smoothness = colorPercent*_ColorGlossiness+(firstPercent+secondPercent)*_Glossiness; //_Glossiness; //mg.y;
			//+thirdPercent+fourthPercent

			o.Emission = colorPercent * IN.color.rgb * _ColorGlow;

			// EMISSION
			//
						
			/*#ifndef _EMISSION
				o.Emission = 0.0;
			#else
				fixed3 emissionX = tex2D(_EmissionMap, xUV).rgb;
				fixed3 emissionY = tex2D(_EmissionMap, yUV).rgb;
				fixed3 emissionZ = tex2D(_EmissionMap, zUV).rgb;
				
				o.Emission = (emissionX * IN.powerNormal.x + emissionY * IN.powerNormal.y + emissionZ * IN.powerNormal.z)
				  * _EmissionColor.rgb;

			#endif
			*/
					
			// OCCLUSION
			//
			
			//#ifdef _OCCLUSION
				
			//	fixed occX = tex2D(_OcclusionMap, xUV).g;
			//	fixed occY = tex2D(_OcclusionMap, yUV).g;
			//	fixed occZ = tex2D(_OcclusionMap, zUV).g;
				
			//	o.Occlusion = LerpOneTo(
			//		(occX * IN.powerNormal.x + occY * IN.powerNormal.y + occZ * IN.powerNormal.z),
			//	   _OcclusionStrength);
				
			//#else
			//	o.Occlusion = 1.0;
			//#endif
				
		}
		ENDCG
	} 
	FallBack "Diffuse"
	//CustomEditor "UVFreePBRShaderGUI"
	
}
			// TRIPLANAR UVs BASED ON WORLD OR LOCAL POSITION
			//
			
			/*#ifdef _UVFREE_LOCAL
				float3 pos = mul(unity_WorldToObject, float4(IN.worldPos, 1.0)).xyz;
				
				float2 posX = pos.zy;
				float2 posY = pos.xz;
				float2 posZ = float2(-pos.x, pos.y);
			#else*/
			//#endif

						//#ifdef _UVFREE_FLIP_BACKWARD_TEXTURES
			//	fixed3 powerSign = sign(IN.normal);
			//	xUV.x *= powerSign.x;
			//	zUV.x *= powerSign.z;
			//	yUV.y *= powerSign.y;
			//#endif
			
			// PARALLAX
			//
			
			//#ifdef _PARALLAXMAP
						/*fixed4 texX3 = tex2D(_MainTex3, xUV3);
			fixed4 texY3 = tex2D(_MainTex3, yUV3);
			fixed4 texZ3 = tex2D(_MainTex3, zUV3);			
			
			fixed4 tex3 = 
			    texX3 * IN.powerNormal.x
			  + texY3 * IN.powerNormal.y
			  + texZ3 * IN.powerNormal.z;
			
			fixed3 albedo3 = max(fixed3(0.0, 0.0, 0.0), tex3.rgb * tint.rgb);
			*/

			//#ifdef _UVFREE_VERTEX_COLOR			 
			//	tex *= lerp(fixed4(1.0,1.0,1.0,1.0), IN.color, _VertexColorStrength);
			//#endif
				