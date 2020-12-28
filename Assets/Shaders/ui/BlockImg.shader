Shader "QMax/BlockImg"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		
		Tags
		{ 
			// "Queue"="Transparent" 
		}

		Cull front
		Lighting Off
		ZWrite Off

		Pass {  
 
		}
	}
}
