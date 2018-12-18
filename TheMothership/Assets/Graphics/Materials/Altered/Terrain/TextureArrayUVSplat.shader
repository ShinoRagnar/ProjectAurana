// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

// Use the non-batching version of this shader if you are using
// local mode, and are seeing the textures move when you zoom in
// due to dynamic batching. (Dynamic batching converts local vertex
// data into world space, making local vertex position data unavailable
// to the shader.)

Shader "MixTerrain/ArrayTriplanarSplat" {
	Properties {
		// Triplanar space, for UI
		[HideInInspector] _TriplanarSpace("Triplanar Space", Float) = 0.0

		_TexPower("Texture Power", Range(0.0, 20.0)) = 10.0

		_BumpScale("Bump Scale", Float) = 1.0
		_TextureScale ("TextureScale", Range(0.0,1.0)) = 0.2
		_DetailScale ("DetailScale", Range(0.0,2)) = 1.5


		_SamplerCount ("SamplerCount", Range(0,16)) = 1
		_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02


		_Glossiness ("Smoothness", Range(0.0,1.0)) = 0.5
		[Gamma] _Metallic ("Metallic", Range(0.0,1.0)) = 0.0
		
		_ColorGlow ("Color glow", Range(0.0,1.0)) = 0.2
		[Gamma] _ColorMetallic ("Metallic", Range(0.0,1.0)) = 0.0
		_ColorGlossiness ("Smoothness", Range(0.0,1.0)) = 0.5

		

		_Albedo ("Albedo", 2DArray) = "" {}
		_Bump ("Bump", 2DArray) = "" {}
		_Height ("Height", 2DArray) = "" {}

		_Occlusion ("Occlusion", 2DArray) = "" {}
		_DetailAlbedo ("DetailAlbedo", 2DArray) = "" {}
		_DetailBump ("DetailBump", 2DArray) = "" {}

		_BumpMapColor("Normal Map Color", 2D) = "bump" {}


		_Test("Test", 2D) = "white" {}


		//_HeightblendFactor("Heightmap Blending Factor", Float) = 0.05

		//_Color ("Color", Color) = (1.0,1.0,1.0,1.0)

		//_MainTex ("Albedo (RGB)", 2D) = "white" {}
		//_MainTex1 ("Albedo2 (RGB)", 2D) = "white" {}
		//_MainTex2 ("Albedo3 (RGB)", 2D) = "white" {}
		//_MainTex3 ("Albedo4 (RGB)", 2D) = "white" {}
		
		//_VertexColorStrength("Vertex Color Strength", Range(0.0,1.0)) = 1.0
		
		
		//_Glossiness1 ("Smoothness 2", Range(0.0,1.0)) = 0.5

		
		//[Gamma] _Metallic1 ("Metallic 2", Range(0.0,1.0)) = 0.0

		//_MetallicGlossMap("Metallic", 2D) = "black" {}
		//_UsingMetallicGlossMap("Using Metallic Gloss Map", float) = 0.0
		//[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		//[ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

		
		//[Gamma] _ColorMetallic ("Metallic", Range(0.0,1.0)) = 0.0
		//_ColorGlossiness ("Smoothness", Range(0.0,1.0)) = 0.5
		//_BumpMapColor("Normal Map Color", 2D) = "bump" {}

		//
		//_BumpMap("Normal Map", 2D) = "bump" {}
		//_BumpScale1("Bump Scale 2", Float) = 1.0
		//_BumpMap1("Normal Map 2", 2D) = "bump" {}
		//_BumpScale2("Bump Scale 3", Float) = 1.0
		//_BumpMap2("Normal Map 3", 2D) = "bump" {}
		//_BumpScale3("Bump Scale 4", Float) = 1.0
		//_BumpMap3("Normal Map 4", 2D) = "bump" {}



		//_Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
		//_ParallaxTextureHeight ("Height Texture", Range (0.005, 1)) = 0.5
		//_ParallaxMap ("Height Map", 2D) = "black" {}

		//_Parallax1 ("Height Scale 2", Range (0.005, 0.08)) = 0.02
		//_ParallaxTextureHeight1  ("Height Texture 2", Range (0.005, 1)) = 0.5
		//_ParallaxMap1 ("Height Map 2", 2D) = "black" {}

		//_OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
		//_OcclusionMap("Occlusion", 2D) = "white" {}
		//_OcclusionMap1("Occlusion 2", 2D) = "white" {}

		//_EmissionColor("Emission Color", Color) = (0.0,0.0,0.0)
		//_EmissionMap("Emission", 2D) = "white" {}

		//_DetailMask("Detail Mask", 2D) = "white" {}

		//_DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
		//_DetailNormalMapScale("Scale", Float) = 1.0
		//_DetailNormalMap("Normal Map", 2D) = "bump" {}

		//_DetailAlbedoMap1("Detail Albedo x2 2", 2D) = "grey" {}
		//_DetailNormalMapScale1("Scale 2", Float) = 1.0
		//_DetailNormalMap1("Normal Map 2", 2D) = "bump" {}

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
		#pragma target 3.0
		#pragma require 2darray

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
		//#define _UVFREE_VERTEX_COLOR
		
		// Instanced Properties
		// https://docs.unity3d.com/Manual/GPUInstancing.html
		//UNITY_INSTANCING_BUFFER_START (MyProperties)
		//UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
		////#define _Color_arr MyProperties
		//UNITY_INSTANCING_BUFFER_END(MyProperties)

		// Non-instanced properties
		half _TexPower;
		//half _HeightblendFactor;

		//sampler2D _MainTex;
		//float4 _MainTex_ST;
		//sampler2D _MainTex1;
		//sampler2D _MainTex2;
		//sampler2D _MainTex3;

		//sampler2D _BumpMap;
		//sampler2D _BumpMap1;
		//sampler2D _BumpMap2;
		//sampler2D _BumpMap3;

		sampler2D _BumpMapColor;
		float4 _BumpMapColor_ST;

		sampler2D _Test;
		float4 _Test_ST;

		half _BumpScale;
		half _TextureScale;
		half _DetailScale;
		half _Parallax;

		float _SamplerCount;
		float _HeightScales[100];
		//half _ParallaxTextureHeight;
		//half _ParallaxTextureHeight1;

		fixed _Metallic;
		fixed _Glossiness;
		fixed _ColorMetallic;
		fixed _ColorGlossiness;
		fixed _ColorGlow;

		UNITY_DECLARE_TEX2DARRAY(_Albedo);
		UNITY_DECLARE_TEX2DARRAY(_Bump);
		UNITY_DECLARE_TEX2DARRAY(_Height);
		UNITY_DECLARE_TEX2DARRAY(_Occlusion);
		UNITY_DECLARE_TEX2DARRAY(_DetailAlbedo);
		UNITY_DECLARE_TEX2DARRAY(_DetailBump);

		//#ifdef _DETAIL
		//	sampler2D _DetailAlbedoMap;
		//	sampler2D _DetailAlbedoMap1;
		//	float4 _DetailAlbedoMap_ST;

			//sampler2D _DetailMask;
		//	sampler2D _DetailNormalMap;
		//	sampler2D _DetailNormalMap1;

		//	half _DetailNormalMapScale;
		//#endif

		//#ifdef _METALLICGLOSSMAP
		//	sampler2D _MetallicGlossMap;
		//	fixed _UsingMetallicGlossMap;		
		//#endif

		//#ifdef _OCCLUSION
		//	sampler2D _OcclusionMap;
		//	fixed _OcclusionStrength;
			
		//#endif

		//#ifdef _PARALLAXMAP
		//	sampler2D _ParallaxMap;
		//	half _Parallax;

		//	sampler2D _ParallaxMap1;
		//	half _Parallax1;
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
            float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
			float2 texcoord2 : TEXCOORD2;
			float2 texcoord3 : TEXCOORD3;

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
				
				o.texcoord.x = v.texcoord.x;
				o.texcoord.y = v.texcoord.y;

				o.texcoord1.x = v.texcoord1.x;
				o.texcoord1.y = v.texcoord1.y;

				o.texcoord2.x = v.texcoord2.x;
				o.texcoord2.y = v.texcoord2.y;

				o.texcoord3.x = v.texcoord3.x;
				o.texcoord3.y = v.texcoord3.y;

			//#endif

			//#ifdef _PARALLAXMAP
        		TANGENT_SPACE_ROTATION;
        		o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
        	//#endif
		}
		
		void surf (Input IN, inout SurfaceOutputStandard o) {
		
			// TRIPLANAR UVs BASED ON WORLD OR LOCAL POSITION
			//
			
			/*#ifdef _UVFREE_LOCAL
				float3 pos = mul(unity_WorldToObject, float4(IN.worldPos, 1.0)).xyz;
				
				float2 posX = pos.zy;
				float2 posY = pos.xz;
				float2 posZ = float2(-pos.x, pos.y);
			#else*/
				float3 pos = IN.worldPos;
			
				float2 posX = IN.worldPos.zy;
				float2 posY = IN.worldPos.xz;
				float2 posZ = float2(-IN.worldPos.x, IN.worldPos.y);				
			//#endif

			float3 xUV = float3(posX.x/2.0, posX.y/2.0, 0); //_MainTex_ST.xy + _MainTex_ST.zw;  IN.texcoord.x*_SamplerCount
			float3 yUV = float3(posY.x/2.0, posY.y/2.0, 0); //posY * //_MainTex_ST.xy + _MainTex_ST.zw;
			float3 zUV = float3(posZ.x/2.0, posZ.y/2.0, 0); //posZ * //_MainTex_ST.xy + _MainTex_ST.zw;

			float3 xUV1 = float3(posX.x * _TextureScale, posX.y * _TextureScale, IN.texcoord1.x*_SamplerCount);
			float3 yUV1 = float3(posY.x * _TextureScale, posY.y * _TextureScale, IN.texcoord1.x*_SamplerCount);
			float3 zUV1 = float3(posZ.x * _TextureScale, posZ.y * _TextureScale, IN.texcoord1.x*_SamplerCount);

			float3 xUV2 = float3(posX.x * _TextureScale, posX.y * _TextureScale, IN.texcoord2.x*_SamplerCount);
			float3 yUV2 = float3(posY.x * _TextureScale, posY.y * _TextureScale, IN.texcoord2.x*_SamplerCount);
			float3 zUV2 = float3(posZ.x * _TextureScale, posZ.y * _TextureScale, IN.texcoord2.x*_SamplerCount);

			float3 xUV3 = float3(posX.x * _TextureScale, posX.y * _TextureScale, IN.texcoord3.x*_SamplerCount);
			float3 yUV3 = float3(posY.x * _TextureScale, posY.y * _TextureScale, IN.texcoord3.x*_SamplerCount);
			float3 zUV3 = float3(posZ.x * _TextureScale, posZ.y * _TextureScale, IN.texcoord3.x*_SamplerCount);

			float2 xUVColor = posX * _BumpMapColor_ST.xy + _BumpMapColor_ST.zw;
			float2 yUVColor = posY * _BumpMapColor_ST.xy + _BumpMapColor_ST.zw;
			float2 zUVColor = posZ * _BumpMapColor_ST.xy + _BumpMapColor_ST.zw;

			//#ifdef _UVFREE_FLIP_BACKWARD_TEXTURES
			//	fixed3 powerSign = sign(IN.normal);
			//	xUV.x *= powerSign.x;
			//	zUV.x *= powerSign.z;
			//	yUV.y *= powerSign.y;
			//#endif
			
			// PARALLAX
			//
			
			//#ifdef _PARALLAXMAP

			//===================== 1
			/*	half parallaxX = UNITY_SAMPLE_TEX2DARRAY(_Height, xUV); //tex2D (_ParallaxMap, xUV).r;
				half parallaxY = UNITY_SAMPLE_TEX2DARRAY(_Height, yUV); // tex2D (_ParallaxMap, yUV).g;
				half parallaxZ = UNITY_SAMPLE_TEX2DARRAY(_Height, zUV); //tex2D (_ParallaxMap, zUV).b;
				
				half parallax = 
					parallaxX * IN.powerNormal.x
				  + parallaxY * IN.powerNormal.y
				  + parallaxZ * IN.powerNormal.z
				  ;
				float2 parallaxOffset = ParallaxOffset (parallax, _Parallax, IN.viewDirForParallax);
				xUV = float3(xUV.x+parallaxOffset.x, xUV.y+parallaxOffset.y , xUV.z);
				yUV = float3(yUV.x+parallaxOffset.x, yUV.y+parallaxOffset.y , yUV.z); //+= parallaxOffset;
				zUV = float3(zUV.x+parallaxOffset.x, zUV.y+parallaxOffset.y , zUV.z); //+= parallaxOffset;

			//===================== 2
				half parallaxX1 = UNITY_SAMPLE_TEX2DARRAY(_Height, xUV1); //tex2D (_ParallaxMap1, xUV1).r;
				half parallaxY1 = UNITY_SAMPLE_TEX2DARRAY(_Height, yUV1); //tex2D (_ParallaxMap1, yUV1).g;
				half parallaxZ1 = UNITY_SAMPLE_TEX2DARRAY(_Height, zUV1); //tex2D (_ParallaxMap1, zUV1).b;
				
				half parallax1 = 
					parallaxX1 * IN.powerNormal.x
				  + parallaxY1 * IN.powerNormal.y
				  + parallaxZ1 * IN.powerNormal.z
				  ;
				float2 parallaxOffset1 = ParallaxOffset (parallax1, _Parallax, IN.viewDirForParallax);

				xUV1 = float3(xUV1.x+parallaxOffset1.x, xUV.y+parallaxOffset1.y , xUV1.z);
				yUV1 = float3(yUV1.x+parallaxOffset1.x, yUV.y+parallaxOffset1.y , yUV1.z); //+= parallaxOffset;
				zUV1 = float3(zUV1.x+parallaxOffset1.x, zUV.y+parallaxOffset1.y , zUV1.z); //+= parallaxOffset;

			//===================== 3
				half parallaxX2 = UNITY_SAMPLE_TEX2DARRAY(_Height, xUV2); //tex2D (_ParallaxMap1, xUV1).r;
				half parallaxY2 = UNITY_SAMPLE_TEX2DARRAY(_Height, yUV2); //tex2D (_ParallaxMap1, yUV1).g;
				half parallaxZ2 = UNITY_SAMPLE_TEX2DARRAY(_Height, zUV2); //tex2D (_ParallaxMap1, zUV1).b;
				
				half parallax2 = 
					parallaxX2 * IN.powerNormal.x
				  + parallaxY2 * IN.powerNormal.y
				  + parallaxZ2 * IN.powerNormal.z
				  ;
				float2 parallaxOffset2 = ParallaxOffset (parallax2, _Parallax, IN.viewDirForParallax);

				xUV2 = float3(xUV2.x+parallaxOffset2.x, xUV.y+parallaxOffset2.y , xUV2.z);
				yUV2 = float3(yUV2.x+parallaxOffset2.x, yUV.y+parallaxOffset2.y , yUV2.z); //+= parallaxOffset;
				zUV2 = float3(zUV2.x+parallaxOffset2.x, zUV.y+parallaxOffset2.y , zUV2.z); //+= parallaxOffset;

			//===================== 4
				half parallaxX3 = UNITY_SAMPLE_TEX2DARRAY(_Height, xUV3); //tex2D (_ParallaxMap1, xUV1).r;
				half parallaxY3 = UNITY_SAMPLE_TEX2DARRAY(_Height, yUV3); //tex2D (_ParallaxMap1, yUV1).g;
				half parallaxZ3 = UNITY_SAMPLE_TEX2DARRAY(_Height, zUV3); //tex2D (_ParallaxMap1, zUV1).b;
				
				half parallax3 = 
					parallaxX3 * IN.powerNormal.x
				  + parallaxY3 * IN.powerNormal.y
				  + parallaxZ3 * IN.powerNormal.z
				  ;
				float2 parallaxOffset3 = ParallaxOffset (parallax3, _Parallax, IN.viewDirForParallax);

				xUV3 = float3(xUV3.x+parallaxOffset3.x, xUV.y+parallaxOffset3.y , xUV3.z);
				yUV3 = float3(yUV3.x+parallaxOffset3.x, yUV.y+parallaxOffset3.y , yUV3.z); //+= parallaxOffset;
				zUV3 = float3(zUV3.x+parallaxOffset3.x, zUV.y+parallaxOffset3.y , zUV3.z); //+= parallaxOffset;
				*/

			//#endif
			
			// DIFFUSE
			//
			
			//===================== 1
			fixed4 texX =  UNITY_SAMPLE_TEX2DARRAY(_Albedo, xUV); //tex2D(_Test, xUV); //UNITY_SAMPLE_TEX2DARRAY(_Albedo, xUV); //tex2D(_MainTex, xUV);
			fixed4 texY = UNITY_SAMPLE_TEX2DARRAY(_Albedo, yUV); //tex2D(_Test, yUV); //UNITY_SAMPLE_TEX2DARRAY(_Albedo, yUV); //tex2D(_MainTex, yUV);
			fixed4 texZ = UNITY_SAMPLE_TEX2DARRAY(_Albedo, zUV);//tex2D(_Test, zUV); //UNITY_SAMPLE_TEX2DARRAY(_Albedo, zUV); //tex2D(_MainTex, zUV);			
			
			fixed4 tex = 
			    texX * IN.powerNormal.x
			  + texY * IN.powerNormal.y
			  + texZ * IN.powerNormal.z;

			//fixed4 tint = UNITY_ACCESS_INSTANCED_PROP (_Color_arr, _Color);

			fixed3 albedo = max(fixed3(0.0, 0.0, 0.0), tex.rgb ); //* tint.rgb

			//===================== 2
			fixed4 texX1 = UNITY_SAMPLE_TEX2DARRAY(_Albedo, xUV1); //tex2D(_MainTex1, xUV1);
			fixed4 texY1 = UNITY_SAMPLE_TEX2DARRAY(_Albedo, yUV1); //tex2D(_MainTex1, yUV1);
			fixed4 texZ1 = UNITY_SAMPLE_TEX2DARRAY(_Albedo, zUV1); //tex2D(_MainTex1, zUV1);			
			
			fixed4 tex1 = 
			    texX1 * IN.powerNormal.x
			  + texY1 * IN.powerNormal.y
			  + texZ1 * IN.powerNormal.z;
			
			fixed3 albedo1 = max(fixed3(0.0, 0.0, 0.0), tex1.rgb); 


			//===================== 3
			fixed4 texX2 = UNITY_SAMPLE_TEX2DARRAY(_Albedo, xUV2); //tex2D(_MainTex2, xUV2);
			fixed4 texY2 = UNITY_SAMPLE_TEX2DARRAY(_Albedo, yUV2); //tex2D(_MainTex2, yUV2);
			fixed4 texZ2 = UNITY_SAMPLE_TEX2DARRAY(_Albedo, zUV2); //tex2D(_MainTex2, zUV2);			
			
			fixed4 tex2 = 
			    texX2 * IN.powerNormal.x
			  + texY2 * IN.powerNormal.y
			  + texZ2 * IN.powerNormal.z;
			
			fixed3 albedo2 = max(fixed3(0.0, 0.0, 0.0), tex2.rgb ); //* tint.rgb


			//===================== 4
			fixed4 texX3 = UNITY_SAMPLE_TEX2DARRAY(_Albedo, xUV3); //tex2D(_MainTex2, xUV2);
			fixed4 texY3 = UNITY_SAMPLE_TEX2DARRAY(_Albedo, yUV3); //tex2D(_MainTex2, yUV2);
			fixed4 texZ3 = UNITY_SAMPLE_TEX2DARRAY(_Albedo, zUV3); //tex2D(_MainTex2, zUV2);			
			
			fixed4 tex3 = 
			    texX3 * IN.powerNormal.x
			  + texY3 * IN.powerNormal.y
			  + texZ3 * IN.powerNormal.z;
			
			fixed3 albedo3 = max(fixed3(0.0, 0.0, 0.0), tex3.rgb ); //* tint.rgb


			//#ifdef _UVFREE_VERTEX_COLOR			 
			//	tex *= lerp(fixed4(1.0,1.0,1.0,1.0), IN.color, _VertexColorStrength);
			//#endif
				


			// DIFFUSE DETAIL
			//
			

			//#ifdef _DETAIL


			//===================== 1
				//fixed detailMaskX = tex2D(_DetailMask, xUV).a;
				//fixed detailMaskY = tex2D(_DetailMask, yUV).a;
				//fixed detailMaskZ = tex2D(_DetailMask, zUV).a;
				
				//fixed detailMask = 1;
				//	detailMaskX * IN.powerNormal.x
				//  + detailMaskY * IN.powerNormal.y
				//  + detailMaskZ * IN.powerNormal.z;
				

				/*

				float3 xUVDetail = float3(pos.z * _DetailScale, pos.y * _DetailScale, IN.texcoord.x*_SamplerCount); //pos.zy * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float3 yUVDetail = float3(pos.x * _DetailScale, pos.z * _DetailScale, IN.texcoord.x*_SamplerCount); //pos.xz * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float3 zUVDetail =  float3(-pos.x * _DetailScale, pos.y * _DetailScale, IN.texcoord.x*_SamplerCount); //float2(-pos.x, pos.y) * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				
				float3 xUVDetail1 = float3(pos.z * _DetailScale, pos.y * _DetailScale, IN.texcoord1.x*_SamplerCount); //xUVDetail; //pos.zy * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float3 yUVDetail1 = float3(pos.x * _DetailScale, pos.z * _DetailScale, IN.texcoord1.x*_SamplerCount); //yUVDetail; //pos.xz * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float3 zUVDetail1 =  float3(-pos.x * _DetailScale, pos.y * _DetailScale, IN.texcoord1.x*_SamplerCount);  //zUVDetail; //float2(-pos.x, pos.y) * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				
				float3 xUVDetail2 = float3(pos.z * _DetailScale, pos.y * _DetailScale, IN.texcoord2.x*_SamplerCount); //xUVDetail; //pos.zy * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float3 yUVDetail2 = float3(pos.x * _DetailScale, pos.z * _DetailScale, IN.texcoord2.x*_SamplerCount); //yUVDetail; //pos.xz * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float3 zUVDetail2 =  float3(-pos.x * _DetailScale, pos.y * _DetailScale, IN.texcoord2.x*_SamplerCount);  //zUVDetail; //float2(-pos.x, pos.y) * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				
				float3 xUVDetail3 = float3(pos.z * _DetailScale, pos.y * _DetailScale, IN.texcoord3.x*_SamplerCount); //xUVDetail; //pos.zy * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float3 yUVDetail3 = float3(pos.x * _DetailScale, pos.z * _DetailScale, IN.texcoord3.x*_SamplerCount); //yUVDetail; //pos.xz * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				float3 zUVDetail3 =  float3(-pos.x * _DetailScale, pos.y * _DetailScale, IN.texcoord3.x*_SamplerCount);  //zUVDetail; //float2(-pos.x, pos.y) * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
				
				//#ifdef _UVFREE_FLIP_BACKWARD_TEXTURES
				//	xUVDetail.x *= powerSign.x;
				//	zUVDetail.x *= powerSign.z;
				//	yUVDetail.y *= powerSign.y;
				//#endif
							
				//#ifdef _PARALLAXMAP
					xUVDetail = float3(parallaxOffset.x+xUVDetail.x, parallaxOffset.y+xUVDetail.y, xUVDetail.z);
					yUVDetail = float3(parallaxOffset.x+yUVDetail.x, parallaxOffset.y+yUVDetail.y, yUVDetail.z); //+= parallaxOffset;
					zUVDetail = float3(parallaxOffset.x+zUVDetail.x, parallaxOffset.y+zUVDetail.y, zUVDetail.z); //+= parallaxOffset;
				//#endif
				
				fixed3 detailAlbedoX = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, xUVDetail).rgb; //tex2D (_DetailAlbedoMap, xUVDetail).rgb;
				fixed3 detailAlbedoY = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, yUVDetail).rgb; //tex2D (_DetailAlbedoMap, yUVDetail).rgb;
				fixed3 detailAlbedoZ = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, zUVDetail).rgb; //tex2D (_DetailAlbedoMap, zUVDetail).rgb;
				
				fixed3 detailAlbedo = 
					detailAlbedoX * IN.powerNormal.x
				  + detailAlbedoY * IN.powerNormal.y
				  + detailAlbedoZ * IN.powerNormal.z;
				 
				albedo *= detailAlbedo * unity_ColorSpaceDouble.rgb; //LerpWhiteTo (detailAlbedo * unity_ColorSpaceDouble.rgb, detailMask);

				//===================== 2

				
					xUVDetail1 = float3(parallaxOffset1.x+xUVDetail1.x, parallaxOffset1.y+xUVDetail1.y, xUVDetail1.z);
					yUVDetail1 = float3(parallaxOffset1.x+yUVDetail1.x, parallaxOffset1.y+yUVDetail1.y, yUVDetail1.z); //+= parallaxOffset;
					zUVDetail1 = float3(parallaxOffset1.x+zUVDetail1.x, parallaxOffset1.y+zUVDetail1.y, zUVDetail1.z); //+= parallaxOffset;

				fixed3 detailAlbedoX1 = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, xUVDetail1).rgb; //tex2D (_DetailAlbedoMap, xUVDetail).rgb;
				fixed3 detailAlbedoY1 = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, yUVDetail1).rgb; //tex2D (_DetailAlbedoMap, yUVDetail).rgb;
				fixed3 detailAlbedoZ1 = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, zUVDetail1).rgb; //tex2D (_DetailAlbedoMap, zUVDetail).rgb;
				
				fixed3 detailAlbedo1 = 
					detailAlbedoX1 * IN.powerNormal.x
				  + detailAlbedoY1 * IN.powerNormal.y
				  + detailAlbedoZ1 * IN.powerNormal.z;
				 
				albedo1 *= detailAlbedo1 * unity_ColorSpaceDouble.rgb; //LerpWhiteTo (detailAlbedo1 * unity_ColorSpaceDouble.rgb, detailMask);

				//===================== 3

				
					xUVDetail2 = float3(parallaxOffset2.x+xUVDetail2.x, parallaxOffset2.y+xUVDetail2.y, xUVDetail2.z);
					yUVDetail2 = float3(parallaxOffset2.x+yUVDetail2.x, parallaxOffset2.y+yUVDetail2.y, yUVDetail2.z); //+= parallaxOffset;
					zUVDetail2 = float3(parallaxOffset2.x+zUVDetail2.x, parallaxOffset2.y+zUVDetail2.y, zUVDetail2.z); //+= parallaxOffset;

				fixed3 detailAlbedoX2 = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, xUVDetail2).rgb; //tex2D (_DetailAlbedoMap, xUVDetail).rgb;
				fixed3 detailAlbedoY2 = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, yUVDetail2).rgb; //tex2D (_DetailAlbedoMap, yUVDetail).rgb;
				fixed3 detailAlbedoZ2 = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, zUVDetail2).rgb; //tex2D (_DetailAlbedoMap, zUVDetail).rgb;
				
				fixed3 detailAlbedo2 = 
					detailAlbedoX2 * IN.powerNormal.x
				  + detailAlbedoY2 * IN.powerNormal.y
				  + detailAlbedoZ2 * IN.powerNormal.z;
				 
				albedo2 *= detailAlbedo2 * unity_ColorSpaceDouble.rgb;
			

				//===================== 4

					xUVDetail3 = float3(parallaxOffset3.x+xUVDetail3.x, parallaxOffset3.y+xUVDetail3.y, xUVDetail3.z);
					yUVDetail3 = float3(parallaxOffset3.x+yUVDetail3.x, parallaxOffset3.y+yUVDetail3.y, yUVDetail3.z); //+= parallaxOffset;
					zUVDetail3 = float3(parallaxOffset3.x+zUVDetail3.x, parallaxOffset3.y+zUVDetail3.y, zUVDetail3.z); //+= parallaxOffset;

				fixed3 detailAlbedoX3 = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, xUVDetail3).rgb; //tex2D (_DetailAlbedoMap, xUVDetail).rgb;
				fixed3 detailAlbedoY3 = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, yUVDetail3).rgb; //tex2D (_DetailAlbedoMap, yUVDetail).rgb;
				fixed3 detailAlbedoZ3 = UNITY_SAMPLE_TEX2DARRAY(_DetailAlbedo, zUVDetail3).rgb; //tex2D (_DetailAlbedoMap, zUVDetail).rgb;
				
				fixed3 detailAlbedo3 = 
					detailAlbedoX3 * IN.powerNormal.x
				  + detailAlbedoY3 * IN.powerNormal.y
				  + detailAlbedoZ3 * IN.powerNormal.z;
				 
				albedo3 *= detailAlbedo3 * unity_ColorSpaceDouble.rgb;
				
				*/
			
			//int coord1 =
			fixed firstMix = IN.texcoord.y; //(parallax * IN.texcoord.y * _HeightScales[ IN.texcoord.x * _SamplerCount]);
			fixed secondMix = IN.texcoord1.y; //(parallax1 *  * _HeightScales[ IN.texcoord1.x * _SamplerCount]);
			fixed thirdMix =  IN.texcoord2.y; //(parallax2 * IN.texcoord2.y * _HeightScales[ IN.texcoord2.x * _SamplerCount]);
			fixed fourthMix = IN.texcoord3.y; //(parallax3 * IN.texcoord3.y * _HeightScales[ IN.texcoord3.x * _SamplerCount]);

			fixed totalMix = firstMix + secondMix + thirdMix + fourthMix + IN.color.a; //thirdMix+fourthMix
			
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

			o.Albedo = albedo; //mixedDiffuse; //IN.color;//albedo1;
			//albedo
			o.Alpha = 1; //tex.a*IN.color.r+tex1.a*IN.color.g; //tex.a*IN.uv2.x+tex1.a*IN.uv2.y;

			//tex.a * tint.a;
						
			// NORMAL
			//
			
			//===================== 1
			
			/*
			fixed3 bumpX = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, xUV),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap, xUV), _BumpScale);
			fixed3 bumpY = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, yUV),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap, yUV), _BumpScale);
			fixed3 bumpZ = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, zUV),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap, zUV), _BumpScale);
			
			fixed3 bump = 
			    bumpX * IN.powerNormal.x
			  + bumpY * IN.powerNormal.y
			  + bumpZ * IN.powerNormal.z;
			//===================== 2
			fixed3 bumpX1 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, xUV1),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap1, xUV1), _BumpScale);
			fixed3 bumpY1 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, yUV1),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap1, yUV1), _BumpScale);
			fixed3 bumpZ1 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, zUV1),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap1, zUV1), _BumpScale);
			
			fixed3 bump1 = 
			    bumpX1 * IN.powerNormal.x
			  + bumpY1 * IN.powerNormal.y
			  + bumpZ1 * IN.powerNormal.z;

			//===================== 3
			fixed3 bumpX2 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, xUV2),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap2, xUV2), _BumpScale);
			fixed3 bumpY2 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, yUV2),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap2, yUV2), _BumpScale);
			fixed3 bumpZ2 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, zUV2),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap2, zUV2), _BumpScale);
			
			fixed3 bump2 = 
			    bumpX2 * IN.powerNormal.x
			  + bumpY2 * IN.powerNormal.y
			  + bumpZ2 * IN.powerNormal.z;

			//===================== 4
			fixed3 bumpX3 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, xUV3),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap2, xUV2), _BumpScale);
			fixed3 bumpY3 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, yUV3),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap2, yUV2), _BumpScale);
			fixed3 bumpZ3 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_Bump, zUV3),_BumpScale); //UnpackScaleNormal(tex2D(_BumpMap2, zUV2), _BumpScale);
			
			fixed3 bump3 = 
			    bumpX3 * IN.powerNormal.x
			  + bumpY3 * IN.powerNormal.y
			  + bumpZ3 * IN.powerNormal.z;

			  */
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
			/*	fixed3 detailNormalTangentX = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, xUVDetail),_BumpScale); //UnpackScaleNormal(tex2D (_DetailNormalMap, xUVDetail), _DetailNormalMapScale);
				fixed3 detailNormalTangentY = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, yUVDetail),_BumpScale); //UnpackScaleNormal(tex2D (_DetailNormalMap, yUVDetail), _DetailNormalMapScale);
				fixed3 detailNormalTangentZ = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, zUVDetail),_BumpScale);  //UnpackScaleNormal(tex2D (_DetailNormalMap, zUVDetail), _DetailNormalMapScale);
				
				fixed3 detailNormalTangent = 
					detailNormalTangentX * IN.powerNormal.x
				  + detailNormalTangentY * IN.powerNormal.y
				  + detailNormalTangentZ * IN.powerNormal.z;
				  
				bump = BlendNormals(bump, detailNormalTangent);
					//lerp(
					//bump,
					//BlendNormals(bump, detailNormalTangent),
					//detailMask);

			//===================== 2
				fixed3 detailNormalTangentX1 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, xUVDetail1),_BumpScale); //UnpackScaleNormal(tex2D (_DetailNormalMap1, xUVDetail1), _DetailNormalMapScale);
				fixed3 detailNormalTangentY1 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, yUVDetail1),_BumpScale); //UnpackScaleNormal(tex2D (_DetailNormalMap1, yUVDetail1), _DetailNormalMapScale);
				fixed3 detailNormalTangentZ1 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, zUVDetail1),_BumpScale); //UnpackScaleNormal(tex2D (_DetailNormalMap1, zUVDetail1), _DetailNormalMapScale);
				
				fixed3 detailNormalTangent1 = 
					detailNormalTangentX1 * IN.powerNormal.x
				  + detailNormalTangentY1 * IN.powerNormal.y
				  + detailNormalTangentZ1 * IN.powerNormal.z;
				  
				bump1 = BlendNormals(bump1, detailNormalTangent1);
					//lerp(
					//bump1,
					//BlendNormals(bump1, detailNormalTangent1),
					//detailMask);
			
			//===================== 3
				fixed3 detailNormalTangentX2 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, xUVDetail2),_BumpScale); //UnpackScaleNormal(tex2D (_DetailNormalMap1, xUVDetail1), _DetailNormalMapScale);
				fixed3 detailNormalTangentY2 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, yUVDetail2),_BumpScale); //UnpackScaleNormal(tex2D (_DetailNormalMap1, yUVDetail1), _DetailNormalMapScale);
				fixed3 detailNormalTangentZ2 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, zUVDetail2),_BumpScale); //UnpackScaleNormal(tex2D (_DetailNormalMap1, zUVDetail1), _DetailNormalMapScale);
				
				fixed3 detailNormalTangent2 = 
					detailNormalTangentX2 * IN.powerNormal.x
				  + detailNormalTangentY2 * IN.powerNormal.y
				  + detailNormalTangentZ2 * IN.powerNormal.z;
				  
				bump2 = BlendNormals(bump2, detailNormalTangent2);

			//===================== 4
				fixed3 detailNormalTangentX3 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, xUVDetail3),_BumpScale); //UnpackScaleNormal(tex2D (_DetailNormalMap1, xUVDetail1), _DetailNormalMapScale);
				fixed3 detailNormalTangentY3 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, yUVDetail3),_BumpScale); //UnpackScaleNormal(tex2D (_DetailNormalMap1, yUVDetail1), _DetailNormalMapScale);
				fixed3 detailNormalTangentZ3 = UnpackScaleNormal(UNITY_SAMPLE_TEX2DARRAY(_DetailBump, zUVDetail3),_BumpScale); //UnpackScaleNormal(tex2D (_DetailNormalMap1, zUVDetail1), _DetailNormalMapScale);
				
				fixed3 detailNormalTangent3 = 
					detailNormalTangentX3 * IN.powerNormal.x
				  + detailNormalTangentY3 * IN.powerNormal.y
				  + detailNormalTangentZ3 * IN.powerNormal.z;
				  
				bump3 = BlendNormals(bump3, detailNormalTangent3);
				*/

			//#endif*/
			/*fixed3 finalBump =
					firstPercent * bump
				  + secondPercent * bump1
				  + thirdPercent * bump2
				  + fourthPercent * bump3
				  + colorPercent * bumpColor;
				  */
					//IN.uv2.x * bump
				  //+ IN.uv2.y * bump1;

			o.Normal = normalize(bumpColor); //UnpackNormal(finalBump);

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
			o.Metallic = _ColorMetallic; //+(firstPercent+secondPercent+thirdPercent+fourthPercent)*_Metallic; //mg.x;
			o.Smoothness = _ColorGlossiness; //+(firstPercent+secondPercent+thirdPercent+fourthPercent)*_Glossiness; //_Glossiness; //mg.y;
			//+thirdPercent+fourthPercent

			o.Emission = 0; //colorPercent * IN.color.rgb * _ColorGlow;

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
