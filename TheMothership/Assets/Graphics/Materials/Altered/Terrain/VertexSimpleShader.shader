// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MixTerrain/VertexSimpleShader" {

	Properties {
		_TexPower("Texture Power", Range (0.0, 20.0)) = 10.0
		_UVScale("Texture Scale %", Float) = 100.0	
		_BumpScale("Bump Scale", Range(-2.0, 2.0)) = 1.0
				
		// set by terrain engine [HideInInspector]
		// _Control ("Control (RGBA)", 2D) = "red" {}
		 _Splat3 ("Layer 3 (A)", 2D) = "white" {}
		 _Splat2 ("Layer 2 (B)", 2D) = "white" {}
		 _Splat1 ("Layer 1 (G)", 2D) = "white" {}
		 _Splat0 ("Layer 0 (R)", 2D) = "white" {}
		 _Normal3 ("Normal 3 (A)", 2D) = "bump" {}
		 _Normal2 ("Normal 2 (B)", 2D) = "bump" {}
		 _Normal1 ("Normal 1 (G)", 2D) = "bump" {}
		 _Normal0 ("Normal 0 (R)", 2D) = "bump" {}
		 [Gamma] _Metallic0 ("Metallic 0", Range(0.0, 1.0)) = 0.0	
		 [Gamma] _Metallic1 ("Metallic 1", Range(0.0, 1.0)) = 0.0	
		 [Gamma] _Metallic2 ("Metallic 2", Range(0.0, 1.0)) = 0.0	
		 [Gamma] _Metallic3 ("Metallic 3", Range(0.0, 1.0)) = 0.0	
		 _Smoothness0 ("Smoothness 0", Range(0.0, 1.0)) = 1.0	
		 _Smoothness1 ("Smoothness 1", Range(0.0, 1.0)) = 1.0	
		 _Smoothness2 ("Smoothness 2", Range(0.0, 1.0)) = 1.0	
		 _Smoothness3 ("Smoothness 3", Range(0.0, 1.0)) = 1.0
		
		// used in fallback on old cards & base map
		[HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
		[HideInInspector] _Color ("Main Color", Color) = (1.0,1.0,1.0,1.0)
	}

	SubShader {
		Tags {
			"SplatCount" = "4"
			"Queue" = "Geometry-100"
			"RenderType" = "Opaque"
		}

		CGPROGRAM //finalcolor:SplatmapFinalColor
		#pragma surface surf Standard  fullforwardshadows

		#define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard

		#pragma multi_compile_fog
		#pragma target 3.0
		// needs more than 8 texcoords
		#pragma exclude_renderers gles
		#include "UnityPBSLighting.cginc"

		//#pragma multi_compile __ _TERRAIN_NORMAL_MAP
		
		// Uncomment to enable experimental feature which flips
		// backward textures. Note: Causes some normals to be flipped.
		// #define _UVFREE_FLIP_BACKWARD_TEXTURES
		
		//sampler2D _Control;
		//float4 _Control_ST;
		sampler2D _Splat0,_Splat1,_Splat2,_Splat3;
		//half4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
		
		//#ifdef _TERRAIN_NORMAL_MAP
			sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
		//#endif


		fixed _Metallic0, _Metallic1, _Metallic2, _Metallic3;
		fixed _Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3;
				
		half _TexPower;
		half _BumpScale;
		
		float _UVScale;

		struct Input
		{
			float2 uv_Splat0;
			float2 uv_Splat1;
			float2 uv_Splat2;
			float2 uv_Splat3;

			float4 color: Color;
		};
		
		
		void surf (Input IN, inout SurfaceOutputStandard o) {
		
			half4 tex0 = tex2D (_Splat0, IN.uv_Splat0);
			half4 tex1 = tex2D (_Splat1, IN.uv_Splat1);
            half4 tex2 = tex2D (_Splat2, IN.uv_Splat2);
            half4 tex3 = tex2D (_Splat3, IN.uv_Splat3);
				
			fixed4 mixedDiffuse;

			mixedDiffuse = 0.0f;
			mixedDiffuse += IN.color.r * tex0 * fixed4(1.0, 1.0, 1.0, _Smoothness0);
			mixedDiffuse += IN.color.g * tex1 * fixed4(1.0, 1.0, 1.0, _Smoothness1);
			mixedDiffuse += IN.color.b * tex2 * fixed4(1.0, 1.0, 1.0, _Smoothness2);
			mixedDiffuse += IN.color.a * tex3 * fixed4(1.0, 1.0, 1.0, _Smoothness3);

			half4 norm0 = tex2D (_Normal0, IN.uv_Splat0);
			half4 norm1 = tex2D (_Normal1, IN.uv_Splat1);
            half4 norm2 = tex2D (_Normal2, IN.uv_Splat2);
            half4 norm3 = tex2D (_Normal3, IN.uv_Splat3);

			half4 normalColor;

			normalColor = 0.0f;
			normalColor += IN.color.r * norm0;
			normalColor += IN.color.g * norm1;
			normalColor += IN.color.b * norm2;
			normalColor += IN.color.a * norm3;

			o.Albedo = max(fixed3(0.0, 0.0, 0.0), mixedDiffuse.rgb);
			o.Normal = UnpackNormal(normalColor);
			o.Alpha = 1;//weight;
			o.Smoothness = mixedDiffuse.a;
			
			o.Metallic = dot(IN.color, fixed4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
		}

		//void SplatmapFinalColor(Input IN, TERRAIN_SURFACE_OUTPUT o, inout fixed4 color)
		//{
		//	color *= o.Alpha;
			//#ifdef TERRAIN_SPLAT_ADDPASS
			//	UNITY_APPLY_FOG_COLOR(IN.fogCoord, color, fixed4(0,0,0,0));
			//#else
			//	UNITY_APPLY_FOG(IN.fogCoord, color);
			//#endif
		//}

		ENDCG
	}

	Dependency "AddPassShader" = "Hidden/UVFree/Terrain/StandardMetallic-AddPass"
	Dependency "BaseMapShader" = "Hidden/UVFree/Terrain/StandardMetallic-Base"

	Fallback "Nature/Terrain/Diffuse"
}
