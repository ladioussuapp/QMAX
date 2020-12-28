//高亮混合   颜色叠加  双面

Shader "QMax/ColorAdditive" {
Properties {
	_MainTex ("Particle Texture", 2D) = "white" {}
	_TintColor ("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha One
	Lighting Off ZWrite Off
	Cull  Off 
	Fog {Mode Off}
	// Fog { Color (0,0,0,0) }

	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	SubShader {
		Pass {
			SetTexture [_MainTex] {
				combine texture * Primary
			}

			SetTexture [_MainTex] {
				constantColor[_TintColor] 
				combine previous * constant
			}
		}
	}
}
}