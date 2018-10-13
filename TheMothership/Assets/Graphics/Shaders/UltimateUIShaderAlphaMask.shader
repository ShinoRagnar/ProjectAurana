// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "SumFX/Ultimate Unlit-UI (Alpha Mask)" {
    Properties {
        _MainTex ("Base Layer", 2D) = "white" {}
        _BaseIntensity ("Base Intensity", Float ) = 1
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseRotation ("Base Rotation", Float ) = 0
        _BaseXMov ("Base X Mov", Float ) = 0
        _BaseYMov ("Base Y Mov", Float ) = 0
        _FX1Mask ("FX 1 Mask", 2D) = "white" {}
        _FXLayer1 ("FX Layer 1", 2D) = "black" {}
        [MaterialToggle] _FX1Blend ("FX 1 Blend", Float ) = 1
        _FX1Intensity ("FX  1 Intensity", Float ) = 0
        _FX1Color ("FX 1 Color", Color) = (1,1,1,1)
        _FX1MIPBlur ("FX  1 MIP Blur", Range(0, 5)) = 0
        _FX1Distortion ("FX  1 Distortion", Range(0, 1)) = 0
        _FX1Rotation ("FX 1 Rotation", Float ) = 0
        _FX1XMov ("FX 1 X Mov", Float ) = 0
        _FX1YMov ("FX 1 Y Mov", Float ) = 0
        _FX1PivotX ("FX 1 Pivot X", Float ) = 0.5
        _FX1PivotY ("FX 1 Pivot Y", Float ) = 0.5
        _FX2Mask ("FX 2 Mask", 2D) = "white" {}
        _FXLayer2 ("FX Layer 2", 2D) = "white" {}
        [MaterialToggle] _FX2Blend ("FX 2 Blend", Float ) = 1
        _FX2Intensity ("FX  2 Intensity", Float ) = 0
        _FX2Color ("FX 2 Color", Color) = (1,1,1,1)
        _FX2MIPBlur ("FX  2 MIP Blur", Range(0, 5)) = 0
        _FX2Distortion ("FX  2 Distortion", Range(0, 1)) = 0
        _FX2Rotation ("FX 2 Rotation", Float ) = 0
        _FX2XMov ("FX 2 X Mov", Float ) = 0
        _FX2YMov ("FX 2 Y Mov", Float ) = 0
        _FX2PivotX ("FX 2 Pivot X", Float ) = 0.5
        _FX2PivotY ("FX 2 Pivot Y", Float ) = 0.5
        _FX3Mask ("FX 3 Mask", 2D) = "white" {}
        _FXLayer3 ("FX Layer 3", 2D) = "black" {}
        [MaterialToggle] _FX3Blend ("FX 3 Blend", Float ) = 1
        _FX3Intensity ("FX  3 Intensity", Float ) = 0
        _FX3Color ("FX 3 Color", Color) = (1,1,1,1)
        _FX3MIPBlur ("FX  3 MIP Blur", Range(0, 5)) = 0
        _FX3Distortion ("FX  3 Distortion", Range(0, 1)) = 0
        _FX3Rotation ("FX 3 Rotation", Float ) = 0
        _FX3XMov ("FX 3 X Mov", Float ) = 0
        _FX3YMov ("FX 3 Y Mov", Float ) = 0
        _FX3PivotX ("FX 3 Pivot X", Float ) = 0.5
        _FX3PivotY ("FX 3 Pivot Y", Float ) = 0.5
        _MultichannelDistortion ("Multichannel Distortion", 2D) = "white" {}
        _DistChannelFX1R ("Dist. Channel FX 1(R)", Float ) = 0
        _DistChannelFX2G ("Dist. Channel FX 2(G)", Float ) = 0
        _DistChannelFX3B ("Dist. Channel FX 3(B)", Float ) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            #pragma glsl
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _BaseRotation;
            uniform float _BaseYMov;
            uniform float _BaseXMov;
            uniform sampler2D _FXLayer1; uniform float4 _FXLayer1_ST;
            uniform float _FX1Rotation;
            uniform float _FX1YMov;
            uniform float _FX1XMov;
            uniform sampler2D _FXLayer2; uniform float4 _FXLayer2_ST;
            uniform float _FX2Rotation;
            uniform float _FX2YMov;
            uniform float _FX2XMov;
            uniform sampler2D _FXLayer3; uniform float4 _FXLayer3_ST;
            uniform float _FX3Rotation;
            uniform float _FX3YMov;
            uniform float _FX3XMov;
            uniform float4 _FX1Color;
            uniform float4 _FX2Color;
            uniform float4 _FX3Color;
            uniform float4 _BaseColor;
            uniform float _BaseIntensity;
            uniform float _FX1Intensity;
            uniform float _FX2Intensity;
            uniform float _FX3Intensity;
            uniform sampler2D _FX1Mask; uniform float4 _FX1Mask_ST;
            uniform sampler2D _FX2Mask; uniform float4 _FX2Mask_ST;
            uniform sampler2D _FX3Mask; uniform float4 _FX3Mask_ST;
            uniform float _FX1PivotX;
            uniform float _FX1PivotY;
            uniform float _FX2PivotX;
            uniform float _FX3PivotX;
            uniform float _FX3PivotY;
            uniform sampler2D _MultichannelDistortion; uniform float4 _MultichannelDistortion_ST;
            uniform float _DistChannelFX1R;
            uniform float _DistChannelFX2G;
            uniform float _DistChannelFX3B;
            uniform fixed _FX1Blend;
            uniform fixed _FX2Blend;
            uniform fixed _FX3Blend;
            uniform float _FX2PivotY;
            uniform float _FX2Distortion;
            uniform float _FX2MIPBlur;
            uniform float _FX1Distortion;
            uniform float _FX1MIPBlur;
            uniform float _FX3Distortion;
            uniform float _FX3MIPBlur;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float uuu5272 = 0.0;
                float uuu1541_if_leA = step(_FX3Blend,uuu5272);
                float uuu1541_if_leB = step(uuu5272,_FX3Blend);
                float4 _FX3Mask_var = tex2D(_FX3Mask,TRANSFORM_TEX(i.uv0, _FX3Mask));
                float uuu7941_if_leA = step(_FX2Blend,uuu5272);
                float uuu7941_if_leB = step(uuu5272,_FX2Blend);
                float4 _FX2Mask_var = tex2D(_FX2Mask,TRANSFORM_TEX(i.uv0, _FX2Mask));
                float uuu6547_if_leA = step(_FX1Blend,uuu5272);
                float uuu6547_if_leB = step(uuu5272,_FX1Blend);
                float4 _FX1Mask_var = tex2D(_FX1Mask,TRANSFORM_TEX(i.uv0, _FX1Mask));
                float4 uuuf9939 = _Time + _TimeEditor;
                float g_ang = uuuf9939.g;
                float g_spd = _BaseRotation;
                float g_cos = cos(g_spd*g_ang);
                float g_sin = sin(g_spd*g_ang);
                float2 g_piv = float2(0.5,0.5);
                float2 g = (mul(i.uv0-g_piv,float2x2( g_cos, -g_sin, g_sin, g_cos))+g_piv);
                float4 e = _Time + _TimeEditor;
                float4 f = _Time + _TimeEditor;
                float2 h = ((g+float2(0.0,(_BaseYMov*e.g)))+float2((_BaseXMov*f.g),0.0));
                float4 _MultichannelDistortion_var = tex2D(_MultichannelDistortion,TRANSFORM_TEX(i.uv0, _MultichannelDistortion));
                float k_ang = uuuf9939.g;
                float k_spd = _FX1Rotation;
                float k_cos = cos(k_spd*k_ang);
                float k_sin = sin(k_spd*k_ang);
                float2 k_piv = float2(_FX1PivotX,_FX1PivotY);
                float2 k = (mul(i.uv0-k_piv,float2x2( k_cos, -k_sin, k_sin, k_cos))+k_piv);
                float4 ii = _Time + _TimeEditor;
                float4 j = _Time + _TimeEditor;
                float2 l = (((k+(_MultichannelDistortion_var.r*_DistChannelFX1R)*float2(1,1))+float2(0.0,(_FX1YMov*ii.g)))+float2((_FX1XMov*j.g),0.0));
                float4 _FXLayer1_var = tex2Dlod(_FXLayer1,float4(TRANSFORM_TEX(l, _FXLayer1),0.0,_FX1MIPBlur));
                float o_ang = uuuf9939.g;
                float o_spd = _FX2Rotation;
                float o_cos = cos(o_spd*o_ang);
                float o_sin = sin(o_spd*o_ang);
                float2 o_piv = float2(_FX2PivotX,_FX2PivotY);
                float2 o = (mul(i.uv0-o_piv,float2x2( o_cos, -o_sin, o_sin, o_cos))+o_piv);
                float4 m = _Time + _TimeEditor;
                float4 n = _Time + _TimeEditor;
                float2 p = (((o+(_MultichannelDistortion_var.g*_DistChannelFX2G)*float2(1,1))+float2(0.0,(_FX2YMov*m.g)))+float2((_FX2XMov*n.g),0.0));
                float4 _FXLayer2_var = tex2Dlod(_FXLayer2,float4(TRANSFORM_TEX(p, _FXLayer2),0.0,_FX2MIPBlur));
                float3 uuu5536 = ((1.0*_FX2Distortion)*(_FXLayer2_var.rgb.r*_FX2Mask_var.rgb));
                float s_ang = uuuf9939.g;
                float s_spd = _FX3Rotation;
                float s_cos = cos(s_spd*s_ang);
                float s_sin = sin(s_spd*s_ang);
                float2 s_piv = float2(_FX3PivotX,_FX3PivotY);
                float2 s = (mul(i.uv0-s_piv,float2x2( s_cos, -s_sin, s_sin, s_cos))+s_piv);
                float4 q = _Time + _TimeEditor;
                float4 r = _Time + _TimeEditor;
                float2 t = (((s+(_MultichannelDistortion_var.b*_DistChannelFX3B)*float2(1,1))+float2(0.0,(_FX3YMov*q.g)))+float2((_FX3XMov*r.g),0.0));
                float4 _FXLayer3_var = tex2Dlod(_FXLayer3,float4(TRANSFORM_TEX(t, _FXLayer3),0.0,_FX3MIPBlur));
                float2 uuu3399 = (((h+((1.0*_FX1Distortion)*(_FXLayer1_var.rgb.r*_FX1Mask_var.rgb)).r)+uuu5536.r)+((1.0*_FX3Distortion)*(_FXLayer3_var.rgb.r*_FX3Mask_var.rgb)).r);
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(uuu3399, _MainTex));
                float3 uuu1673 = (saturate((_MainTex_var.rgb*_BaseColor.rgb))*_BaseIntensity);
                float3 u = (saturate((_FXLayer1_var.rgb*_FX1Color.rgb))*_FX1Intensity);
                float3 uuu7335 = (lerp( lerp( lerp( _FX1Mask_var.rgb, u, _FX1Mask_var.rgb.r ), u, _FX1Mask_var.rgb.g ), u, _FX1Mask_var.rgb.b ));
                float3 uuu8549 = (lerp( lerp( lerp( uuu1673, uuu7335, _FX1Mask_var.rgb.r ), uuu7335, _FX1Mask_var.rgb.g ), uuu7335, _FX1Mask_var.rgb.b ));
                float3 uuu6547 = lerp((uuu6547_if_leA*uuu8549)+(uuu6547_if_leB*saturate((uuu1673+uuu7335))),uuu8549,uuu6547_if_leA*uuu6547_if_leB);
                float3 v = (saturate((_FXLayer2_var.rgb*_FX2Color.rgb))*_FX2Intensity);
                float3 uuu9411 = (lerp( lerp( lerp( _FX2Mask_var.rgb, v, _FX2Mask_var.rgb.r ), v, _FX2Mask_var.rgb.g ), v, _FX2Mask_var.rgb.b ));
                float3 uuu5354 = (lerp( lerp( lerp( uuu6547, uuu9411, _FX2Mask_var.rgb.r ), uuu9411, _FX2Mask_var.rgb.g ), uuu9411, _FX2Mask_var.rgb.b ));
                float3 uuu7941 = lerp((uuu7941_if_leA*uuu5354)+(uuu7941_if_leB*saturate((uuu6547+uuu9411))),uuu5354,uuu7941_if_leA*uuu7941_if_leB);
                float3 x = (saturate((_FXLayer3_var.rgb*_FX3Color.rgb))*_FX3Intensity);
                float3 uuu7492 = (lerp( lerp( lerp( _FX3Mask_var.rgb, x, _FX3Mask_var.rgb.r ), x, _FX3Mask_var.rgb.g ), x, _FX3Mask_var.rgb.b ));
                float3 uuu308 = (lerp( lerp( lerp( uuu7941, uuu7492, _FX3Mask_var.rgb.r ), uuu7492, _FX3Mask_var.rgb.g ), uuu7492, _FX3Mask_var.rgb.b ));
                float3 emissive = lerp((uuu1541_if_leA*uuu308)+(uuu1541_if_leB*saturate((uuu7941+uuu7492))),uuu308,uuu1541_if_leA*uuu1541_if_leB);
                float3 finalColor = emissive;
                return fixed4(finalColor,_MainTex_var.a);
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}
