Shader "Pixyz/Vertex Colors"
{
	Properties
	{

	}
	SubShader
	{
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Overlay" }


		LOD 200

		Pass
		{
            Tags { "LightMode"="SRPDefaultUnlit" }

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct appdata
			{
				float4 position : POSITION;
				float4 color : COLOR;
			};

			struct input
			{
				float4 position : POSITION;
				float3 coords : TEXCOORD1;
				float4 color : COLOR;
			};

			float _Offset;

			input vert (appdata i)
			{
				input o;
				o.position = TransformObjectToHClip(i.position.xyz);
				o.coords = i.position.xyz;
				o.color = i.color;
				return o;
			}

			half4 frag (input i) : SV_Target
			{
				return i.color;
			}
			ENDHLSL
		}
	}
	SubShader
	{
		Tags { "Queue" = "Overlay" }
		LOD 200

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 position : POSITION;
				float4 color : COLOR;
			};

			struct input
			{
				float4 position : POSITION;
				float3 coords : TEXCOORD1;
				float4 color : COLOR;
			};

			float _Offset;

			input vert (appdata i)
			{
				input o;
				o.position = UnityObjectToClipPos(i.position);
				o.coords = i.position.xyz;
				o.color = i.color;
				return o;
			}

			fixed4 frag (input i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}
