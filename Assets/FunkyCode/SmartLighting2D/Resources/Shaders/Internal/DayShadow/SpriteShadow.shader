﻿Shader "Light2D/Internal/SpriteShadow" {

	Properties
	{
		_MainTex ("Sprite Texture", 2D) = "white" {}

		_Darkness("Darkness",Color) = (0, 0, 0, 1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		BlendOp Min

		Pass
		{
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	
			#include "UnityCG.cginc"

			sampler2D _MainTex;

			float4 _Darkness;

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
				float2 texcoord  : TEXCOORD0;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;

				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color;
		
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 color = tex2D (_MainTex, IN.texcoord);

				return float4(lerp(1 + _Darkness.rgb - color.a + 1 - IN.color.a, 1, 1 - _Darkness.a), 1);
			}

			ENDCG
		}
	}
}