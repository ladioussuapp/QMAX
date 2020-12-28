//基于Mobile/Particles/Alpha Blended 关闭背面剪裁  开启雾效

Shader "QMax/MultiWather" {
Properties {
	_SurfaceTex ("Surface Texture", 2D) = "white" {}
	_BottomTex("Bottom Texture" , 2D) = "white"{}
	// _Color ("Color", Color) = (0.5,0.5,0.5,0.5)
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
	Blend SrcAlpha OneMinusSrcAlpha
	Lighting Off 
	ZWrite Off 

	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	SubShader {
		Pass {
			SetTexture [_BottomTex] {
				combine texture * primary  
			}
 
		}

		Pass {
			SetTexture [_SurfaceTex] {
				combine texture * previous  
			}
		}
	}
}
}
