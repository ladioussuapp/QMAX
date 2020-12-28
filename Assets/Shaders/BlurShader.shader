// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "QMax/BlurShader"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		// _StencilComp ("Stencil Comparison", Float) = 8
		// _Stencil ("Stencil ID", Float) = 0
		// _StencilOp ("Stencil Operation", Float) = 0
		// _StencilWriteMask ("Stencil Write Mask", Float) = 255
		// _StencilReadMask ("Stencil Read Mask", Float) = 255
		// _Dis("blur dis" , _Range ("Range", Range(0.0, 0.01))) = (0.0)
		_Dis ("blur dis", Range(0.0, 0.01)) = 0.0
		_light ("light", Float) = 2
 
		// _ColorMask ("Color Mask", Float) = 15
	}

	SubShader
	{
		Tags
		{ 
			// "Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Opaque" 
			"Queue" = "Geometry"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			// ReadMask [_StencilReadMask]
			// WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		// ZTest [unity_GUIZTestMode]
		// Blend SrcAlpha OneMinusSrcAlpha
		// ColorMask [_ColorMask]

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// #pragma exclude_renderers 

			#include "UnityCG.cginc"

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				// float4 worldPosition : TEXCOORD1;
			};
			
			// fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float _Dis;
			half _light;
			// bool _UseClipRect;
			// float4 _ClipRect;
			// bool _UseAlphaClip;
 
			v2f vert(appdata_t IN)
			{
				v2f OUT;
				// OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;

				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
				#endif
				
				// OUT.color = IN.color * _Color;
				return OUT;
			}

			sampler2D _MainTex;

			fixed4 frag(v2f IN) : SV_Target
			{
				float2 uv = IN.texcoord.xy;

				// half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
				half4 color = (tex2D(_MainTex, uv));

				uv =  float2 (IN.texcoord.x + _Dis , IN.texcoord.y);		//x偏移
				color += (tex2D(_MainTex, uv));
				uv = float2 (IN.texcoord.x - _Dis , IN.texcoord.y);
				color += (tex2D(_MainTex, uv));

				uv =  float2 (IN.texcoord.x, IN.texcoord.y + _Dis);		//y偏移
				color += (tex2D(_MainTex, uv));
				uv = float2 (IN.texcoord.x, IN.texcoord.y - _Dis);
				color += (tex2D(_MainTex, uv));

				uv =  float2 (IN.texcoord.x + _Dis, IN.texcoord.y + _Dis);		//右上偏移
				color += (tex2D(_MainTex, uv));
				uv = float2 (IN.texcoord.x + _Dis, IN.texcoord.y - _Dis);
				color += (tex2D(_MainTex, uv));
				uv = float2 (IN.texcoord.x - _Dis, IN.texcoord.y - _Dis);
				color += (tex2D(_MainTex, uv));
				uv = float2 (IN.texcoord.x - _Dis, IN.texcoord.y + _Dis);
				color += (tex2D(_MainTex, uv));
 				
				color = color / _light;
				color.a = 1;
				return color;
			}
		ENDCG
		}
	}
}
