Shader "Custom/WorldSpaceNoiseMask"
{
	Properties
	{
		//material 
		/*[NoScaleOffset]*/ _MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Length("Length", Range(1,99)) = 10

			//noise settings
			_NoiseSettings("Noise settings : scale xy, offset xy", Vector) = (1, 1, 0, 0)
			_NoiseSettings2("Noise settings gain, frequencyMul, baseWeight, na", Vector) = (0.5, 2.5, 0.5)

			//circle properties
			_CircleForce("Circle Force", Range(0,1)) = 0.25
			_Expand("Inner Expand", Range(0,1)) = 0.5
			_Radius("Radius", FLOAT) = 0.75
			//_WindowHeight("Window Height", FLOAT) = 0.0
			_ChangePoint("Change at this distance", Float) = 5
			//_MaxDistance("Max hide value", Float) = 5

			//outline properties
			_LineWidth("Line Width", Range(0,1)) = 0.5
			_LineColor("Line Color (alpha = emission)", Color) = (1,1,1,0)
			//_Cloak("Hide 1 (1 = hide)", Range(0,1)) = 1
			//_Cloak2("Hide 2 (1 = hide)", Range(0,1)) = 1
			//_Cloak3("Hide 3 (1 = hide)", Range(0,1)) = 1
			//_Cloak4("Hide 4 (1 = hide)", Range(0,1)) = 1
			//_TargetPosition("Target Pos 1", Vector) = (0,0,0)
			// _TargetPos2("Target Pos 2", Vector) = (0,0,0)
			// _TargetPos3("Target Pos 3", Vector) = (0,0,0)
			// _TargetPos4("Target Pos 4", Vector) = (0,0,0)
			//_Clip("Clip", Range(0,1)) = 0.025
			_Invert("Invert", Range(0,1)) = 0.

	}

		SubShader
		{

			Tags
			{
				"RenderType" = "Opaque"
			}
			Cull Off

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma vertex vert 
			#pragma surface surf Standard fullforwardshadows

			#pragma target 4.0
			#include "Assets/Shaders/Include/snoise.cginc"

			//TABLE!
			#define count 100
			uniform int _Length;
			uniform float3 _Positions[count];
			uniform float _Cloaks[count];

			uniform float _ChangeFactors[count];
			uniform float _CurDistances[count];

			uniform sampler2D _MainTex;
			uniform float _Glossiness;
			uniform float _Metallic;

			//noise 
			uniform float _Cloak;
			uniform float _Cloak2;
			uniform float _Cloak3;
			uniform float _Cloak4;

			uniform float4 _NoiseSettings;
			uniform float3 _NoiseSettings2;

			//radial gradient
			uniform float4 _CircleColor;
			uniform float _CircleForce;
			uniform float _Expand;
			uniform float2 _Center;
			uniform float _Radius;
			uniform float _WindowHeight;

			//outline
			uniform float _ChangePoint;
			uniform float4 _LineColor;
			uniform float _LineWidth;
			uniform float4 _Color;
			uniform float _MaxDistance;

			//target
			uniform float3 _TargetPosition;
			uniform float3 _TargetPos2;
			uniform float3 _TargetPos3;
			uniform float3 _TargetPos4;
			uniform float _Clip;
			uniform int _Invert;

			struct Input
			{
				float2 uv_MainTex : TEXCOORD0;
				float2 noiseUV : TEXCOORD1;
				float4 screenPos;
				float3 worldPos;
			};



			void vert(inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input,o);
				o.uv_MainTex = v.texcoord.xy;
				o.noiseUV = (v.texcoord.xy + _NoiseSettings.zw) * _NoiseSettings.xy;
			}

			float NoiseFunction(float baseNoiseValue, uint generation)
			{
				if (generation < 2)
				{
					return 1 - abs(baseNoiseValue);
				}
				else
				{
					return baseNoiseValue * baseNoiseValue;
				}

			}

			float PoseNoiseFunction(float baseNoiseValue)
			{
				return abs(baseNoiseValue);
			}


			void surf(Input IN, inout SurfaceOutputStandard o)
			{

				//***********************SCREEN-UV**********************//
				float2 screenUV = IN.screenPos.xy / IN.screenPos.w;


				//***************************NOISE*********************//
				float noise = 0;
				float weight = _NoiseSettings2.z;
				float frequency = 1;
				const float gain = _NoiseSettings2.x;
				const float frequencyMul = _NoiseSettings2.y;
				for (uint i = 0; i < 8; ++i)
				{
					noise += weight * NoiseFunction(snoise(screenUV * frequency), i);
					weight *= gain;
					frequency *= frequencyMul;
				}
				float postNoise = saturate(PoseNoiseFunction(noise));


				//************************CIRCLE SCREEN*********************//
				//float2 centerFromSfml = float2(0.5, _WindowHeight + 0.5);
				float2 centerFromSfml = float2(0.5, 0.5);
				float2 p = (screenUV.xy - centerFromSfml) / _Radius;
				float r = sqrt(dot(p, p));
				float circleLerp = 1 - _CircleForce;
				float circle = lerp(circleLerp, noise, (r - _Expand) / (1 - _Expand));
				float maskClip = 1 - (postNoise - circle);


				//***************************DISTANCE*********************//
				//maskClip *= 0.5; 
				float inLine = 1;
				_CurDistances[0] = distance(_Positions[0].xyz, IN.worldPos);
				_ChangeFactors[0] = maskClip + ((_Cloaks[0] * _ChangePoint) * inLine) - (1 - (_CurDistances[0] - _ChangePoint));
				_ChangeFactors[0] = clamp(_ChangeFactors[0], -1, 1);

				float sumT = 1 - _ChangeFactors[0];
				float multT = _ChangeFactors[0];

				for (uint i = 1; i < _Length; ++i)
				{
					_CurDistances[i] = distance(_Positions[i].xyz, IN.worldPos);
					_ChangeFactors[i] = maskClip + ((_Cloaks[i] * _ChangePoint) * inLine) - (1 - (_CurDistances[i] - _ChangePoint));
					_ChangeFactors[i] = clamp(_ChangeFactors[i], -1, 1);
					sumT += 1 - _ChangeFactors[i];
					multT *= 1 - _ChangeFactors[i];
				}

				float finalValue = sumT + multT;
				finalValue = sumT;

				float finalInvers = 1 - finalValue;
				//float clipRange = lerp(finalInvers, finalValue, _Invert);

				/*clipRange *= 1 + _LineWidth; */
				float clipV = lerp(-1, finalInvers, _Clip);
				float clipFinal = lerp(-clipV, clipV, _Invert);
				clip(clipFinal);
				//clip(-clipV + _LineWidth);
				//if (_Invert > 0.9)
				//{
				//	clip(clipV);
				//}
				//else
				//{
				//	clip(-clipV);
				//}

				//*********************TEXTURE****************************//
				float4 c = tex2D(_MainTex, IN.uv_MainTex);

				float shouldLine1 = step(-_LineWidth, -finalInvers);
				float shouldLine2 = step(0, -finalInvers);
				float shouldLine = shouldLine1 - shouldLine2;
				o.Albedo = c.rgb;
				//o.Albedo.b = shouldLine; 
				o.Albedo = lerp(o.Albedo, c.rgb * _LineColor.rgb, shouldLine);
				o.Emission = lerp(o.Emission, _LineColor.a, shouldLine);
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;

			}
			ENDCG
		}

			FallBack Off
}