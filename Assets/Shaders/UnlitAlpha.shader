//无光照透明
Shader "QMax/UnlitAlpha" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "Queue" = "Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend One OneMinusSrcAlpha
		ZWrite Off
		Cull Off
		Fog {Mode Off}
		Pass {
			SetTexture[_MainTex] {
				constantColor [_Color]
				Combine texture * constant, texture * constant
			}
		}
	}
}