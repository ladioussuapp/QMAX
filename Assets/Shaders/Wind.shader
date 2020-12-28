Shader "QMax/Wind" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags { "Queue" = "Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Cull Off 
		Fog {Mode Off}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			SetTexture[_MainTex] {
				Combine texture
			}
		}
	}
}