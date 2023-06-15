Shader "PhysicsBall/CurvedSurface"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "black" {}
		_lineDensity("Line Density", float) = 0.1
		_texturedArea("Textured Area", Vector) = (0,0,10,10)
		_untexturedColor("Untextured Color", Color) = (0,0,0,1)
	}
	SubShader
	{
        LOD 200
    	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    	ZWrite true
    	Blend SrcAlpha OneMinusSrcAlpha
    	Cull Off
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f {
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};
			

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _lineDensity;
			float4 _texturedArea;
			float4 _untexturedColor;
			float _wavePropagationOffset;

			
			float GetYPosAtUvPoint (float2 pos)
			{
				float firstTerm = 0.3f * sin(_wavePropagationOffset + 3 * sqrt(pow(pos.x - 5, 2) + pow(pos.y - 5, 2)));
				float secondTerm = 0.5f * cos(pos.x + pos.y);
				
				float yPos = firstTerm + secondTerm;
				return yPos;
			}
    		float3 GetNormalAtUvPoint(float2 pos)
    		{
    		    float delta = 0.001f; 
    		    
    		    float fx = GetYPosAtUvPoint(float2(pos.x + delta, pos.y)) - GetYPosAtUvPoint(float2(pos.x - delta, pos.y));
    		    float fz = GetYPosAtUvPoint(float2(pos.x, pos.y + delta)) - GetYPosAtUvPoint(float2(pos.x, pos.y - delta));
    		    float3 normal = normalize(float3(-fx, 2 * delta, -fz));
		
    		    return normal;
    		}

			float mapf(float s, float a1, float a2, float b1, float b2)
			{
				return b1 + (s-a1)*(b2-b1)/(a2-a1);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				o.normal = GetNormalAtUvPoint(o.uv);
				
				float yPos = GetYPosAtUvPoint(o.uv);
				v.vertex.y = yPos;
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				return o;
			}
			
			float4 WhiteHightGraph (float yPos)
			{
				float modular = fmod(abs(yPos), _lineDensity) / _lineDensity;
				
				float c3 = (modular < 0.05) ? 1 : 0;
				
				float4 col = float4(0, 0, 0, 0);
				return col;
			}
			float4 GetColor (v2f i)
			{
				float yPos = GetYPosAtUvPoint(i.uv);
				
				float4 linesColor = WhiteHightGraph(yPos);
				float4 textureColor = tex2D(_MainTex, i.uv.xy);

				
				float2 bottomLeft = _texturedArea.xy;
				float2 topRight = _texturedArea.xy + _texturedArea.zw;
			
				// If uv is outside the textured area, set the textureColor to _untexturedColor
				if (i.uv.x < bottomLeft.x || i.uv.y < bottomLeft.y || i.uv.x > topRight.x || i.uv.y > topRight.y) {
					textureColor = _untexturedColor;
				}

				textureColor.rgb *= float3(1,1,1) * mapf(i.normal.y, 0.5, 1, 0, 1);
				
				float4 finalColor = linesColor.a > 0 ? linesColor : textureColor;
				
				return finalColor;
			}
			fixed4 frag (v2f i) : SV_Target {
				return GetColor(i);
			}
			ENDCG
		}
	}
}
