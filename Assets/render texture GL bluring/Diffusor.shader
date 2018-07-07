Shader "Hidden/Diffusor"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _emitters;
			sampler2D _MainTex;
			half w,h;
			float atten;
			float diffusion;

			float UVRandom(half2 uv)
			{
				return frac(sin(dot(uv, half2(12.9898, 78.233))) * 43758.5453);
			}

			fixed4 frag (v2f id) : SV_Target
			{
				half rnd = step(UVRandom(half2(id.uv.x/w, id.uv.y/h)),probability);
				half4 value =
					tex[id]
					+ (tex[uint2(id.uv.x,id.uv.y+1)]
					+ tex[uint2(id.uv.x,id.uv.uv.y-1)]
					+ tex[uint2(id.uv.x+1,id.uv.y)]
					+ tex[uint2(id.uv.x-1,id.uv.y)]
					)/5*diffusion
					;
				value = lerp(value, 0, atten);
				value += emitters[id];
				return value;
			}
			ENDCG
		}
	}
}
