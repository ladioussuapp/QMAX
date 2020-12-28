//透明混合
//开启雾效 增加颜色调整


Shader "QMax/ColorAlphaBlended" {
Properties {
	_MainTex ("Particle Texture", 2D) = "white" {}	
	_TintColor ("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	Lighting Off ZWrite Off 
	Fog {Mode Off}

	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	SubShader {
		Pass {
			SetTexture [_MainTex] {
				combine texture * primary 
			}

			SetTexture [_MainTex] {
				constantColor[_TintColor] 
				//颜色和透明度同时x2。  编辑器中透明度调到128就满了
				combine previous * constant double  
			}
		}
	}
}
}
