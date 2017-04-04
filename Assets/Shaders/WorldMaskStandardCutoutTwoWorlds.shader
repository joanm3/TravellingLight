Shader "Custom/WorldMask Two Worlds Standard Cutout"
{
	Properties
	{
		//material 
		/*[NoScaleOffset]*/ _MainTex("Albedo World 1", 2D) = "white" {}
		_Cutout("Cutout 1", Range(0,1)) = 0.0
		_Normal("Normal Map 1", 2D) = "bump" {}
		_Glossiness("Smoothness 1", Range(0,1)) = 0.5
		_Metallic("Specular 1", Range(0,1)) = 0.0
		_MainTex2("Albedo World 2", 2D) = "white" {}
		_Cutout2("Cutout 2", Range(0,1)) = 0.0

		_Normal2("Normal Map 2", 2D) = "bump" {}
		_Glossiness2("Smoothness 2", Range(0,1)) = 0.5
		_Metallic2("Specular 2", Range(0,1)) = 0.0

		_Length("Length", Range(1,99)) = 10

			//noise settings
			_NoiseSettings("Noise settings : scale xy, offset xy", Vector) = (1, 1, 0, 0)
			_NoiseSettings2("Noise settings gain, frequencyMul, baseWeight, na", Vector) = (0.5, 2.5, 0.5)

			//circle properties
			_CircleForce("Circle Force", Range(0,1)) = 0.25
			_Expand("Inner Expand", Range(0,1)) = 0.5
			_Radius("Radius", FLOAT) = 0.75
			_ChangePoint("Change at this distance", Float) = 5

			//outline properties
			_LineWidth("Line Width", Range(0,1)) = 0.5
			_LineColor("Line Color (alpha = emission)", Color) = (1,1,1,0)
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
			//#pragma vertex vert 
			#pragma surface surf Standard fullforwardshadows
			//#pragma surface surf Lambert
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
			uniform sampler2D _Normal;
			uniform float _Glossiness;
			uniform float _Metallic;


			uniform sampler2D _MainTex2;
			uniform sampler2D _Normal2;
			uniform float _Glossiness2;
			uniform float _Metallic2;

			uniform float _Cutout;
			uniform float _Cutout2;

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
				float2 uv_Normal;
				float2 uv_Normal2;
				float4 screenPos;
				float3 worldPos;
			};



			//void vert(inout appdata_full v, out Input o)
			//{
			//	UNITY_INITIALIZE_OUTPUT(Input,o);
			//	o.uv_MainTex = v.texcoord.xy;
			//	o.uv_Normal = v.texcoord.xy; 
			//	o.noiseUV = (v.texcoord.xy + _NoiseSettings.zw) * _NoiseSettings.xy; 
			//}

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
				float clipRange = lerp(finalInvers, finalValue, _Invert);
				/*clipRange *= 1 + _LineWidth; */
				float clipV = lerp(-1, clipRange, _Clip);
				//clip(-clipV + _LineWidth);

				//*********************TEXTURE****************************//

				float shouldLine1 = step(-_LineWidth, -clipRange);
				float shouldLine2 = step(0, -clipRange);
				float shouldLine = shouldLine1 - shouldLine2;

				//albedo
				float4 c = tex2D(_MainTex, IN.uv_MainTex);
				float4 c2 = tex2D(_MainTex2, IN.uv_MainTex);
				o.Albedo = c.rgb;
				o.Albedo = lerp(c2.rgb, c.rgb, shouldLine1);
				o.Albedo = lerp(o.Albedo, c.rgb * _LineColor.rgb, shouldLine);

				//normal
				o.Normal = lerp(UnpackNormal(tex2D(_Normal2, IN.uv_Normal2)) , UnpackNormal(tex2D(_Normal, IN.uv_Normal)) , shouldLine1);

				o.Emission = lerp(o.Emission, _LineColor.a, shouldLine);
				o.Metallic = lerp(_Metallic2, _Metallic, shouldLine1);
				o.Smoothness = lerp(_Glossiness2, _Glossiness, shouldLine1);


				//do this with steps. 
				//if (shouldLine1 > 1)
				//{
				if (c.a > _Cutout)
					o.Alpha = c.a;
				else
					clip(-1);

				//}
				//else
				//{
				//	if (c2.a > _Cutout2)
				//		o.Alpha = c2.a;
				//	else
				//		o.Alpha = 0;
				//}


			}
			ENDCG
		}

			FallBack Off
}