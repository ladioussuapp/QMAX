//基于moblie下的叠加
Shader "QMax/Additive" {
	Properties {
		_MainTex ("Particle Texture", 2D) = "white" {}
	}

	Category {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
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
					combine texture * Primary
				}
			}
		}
	}
}