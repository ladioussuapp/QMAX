Shader "QMax/Sky" { 
    Properties { 
        _MainTex ("Base (RGB)", 2D) = "white" {} 
		_Color ("Main Color", Color) = (1,1,1,1)
    } 
    SubShader { 
        Tags { "Queue"="Background" "IgnoreProjector"="True" "RenderType"="Transparent"} 
        ZWrite Off           
		Blend SrcAlpha OneMinusSrcAlpha
		                      
		Pass {
			SetTexture[_MainTex] {
				constantColor [_Color]
				Combine texture * constant, texture * constant
			}
		}
	}
}