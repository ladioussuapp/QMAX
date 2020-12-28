// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//保持与mobile 下的 Additive 一致
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask
Shader "Particles/Additive Simple (ETC+Alpha using R channel)" 
{
	Properties 
	{
		_MainTex ("Particle Texture", 2D) = "white" {}
		_AlphaTex ("Trans (A)", 2D) = "white" {}
	}

	Category 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off

		SubShader 
		{
			Pass 
			{			
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_particles

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				sampler2D _AlphaTex;

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float2 texcoord : TEXCOORD0;
 
				};
				
				float4 _MainTex_ST;

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
 
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}

				sampler2D _CameraDepthTexture;
 
				fixed4 frag (v2f i) : COLOR
				{
					fixed4 col =tex2D(_MainTex, i.texcoord);
					fixed4 col_a=tex2D(_AlphaTex, i.texcoord);
					col.a=col_a.r;
					return 2.0f * i.color * col;
				}
				ENDCG 
			}
		}	
	}
}
