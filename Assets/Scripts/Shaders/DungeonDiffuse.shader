Shader "Custom/DungeonDiffuse"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

		SubShader
	{
		Pass
		{
			Tags
			{
				"LightMode" = "ForwardBase"
			}
			CGPROGRAM

			#pragma	vertex Vertex
			#pragma fragment Fragment

			#include "UnityStandardBRDF.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

			struct VertexData
			{
				float4 position : POSITION;
				float4 color : COLOR0;
				float2 uv : TEXCOORD0;
			};

			struct Interpolators
			{
				float4 position : SV_POSITION;
				float4 color: COLOR0;
				float2 uv : TEXCOORD0;
			};

			Interpolators Vertex(VertexData v)
			{
				Interpolators i;
				i.position = UnityObjectToClipPos(v.position);
				i.uv = v.uv * _MainTex_ST.yx;
				i.color = v.color;
				return i;
			}

			float4 Fragment(Interpolators i) : SV_TARGET
			{
				return tex2D(_MainTex, i.uv) * i.color;
			}

			ENDCG
		}
	}
}