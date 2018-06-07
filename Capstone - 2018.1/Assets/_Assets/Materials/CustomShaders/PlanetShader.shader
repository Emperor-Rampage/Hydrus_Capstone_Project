Shader "Custom/PlanetShader" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_SpecularTex("Specular Map", 2D) = "white" {}
		_NormalTex("Normal Map", 2D) = "bump" {}
		_CloudTex("Cloud Map", 2D) = "white" {}
		_AtmosphereRadius("Atmosphere Radius", float) = 0.0
		_PlanetRadius("Planet Radius", float) = 0.0
		_PlanetCenter("Planet Center", vector3) = (0,0,0)
		_ViewSamples("Precision of Scattering", int) = 3
		_SunIntensity("Sun Intensity", float) = 1.0
		_ScatteringCoefficient("Scattering Coefficient", float) = 1.0
	}

	SubShader
	{

			//======First Pass========
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows

		#pragma target 4.6

		sampler2D _MainTex;
		sampler2D _SpecularTex;
		sampler2D _NormalTex;
		sampler2D _CloudTex;

		struct Input
		{
			float2 uv_MainTex;
			float2 uv_SpecularTex;
			float2 uv_NormalTex;
			float2 uv_CloudTex;
		};

		fixed4 _Color;
		half _Metallic;
		half _Glossiness;


		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)
		//Cg Code Here

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;

			o.Normal = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));
		}

		ENDCG
		// =======================

		//======Second Pass======
		Tags { "RenderType" = "Transparent"
		"Queue"="Transparent"}
		LOD 200
		Cull Back

		Blend One One

		CGPROGRAM

		#pragma surface surf StandardScattering vertex:vert
		#include "UnityPBSLighting.cginc"

	

		//CG Code here
		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos; //Initialised automatically by Unity
			float3 center; //Initialised in the vertex function
		};

		float _AtmosphereRadius;
		float _PlanetRadius;
		float3 _PlanetCenter;
		float _SunIntensity;
		int _ViewSamples;
		float _ScatteringCoefficient;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			v.vertex.xyz += v.normal * (_AtmosphereRadius - _PlanetRadius);
			o.center = mul(unity_ObjectToWorld, half4(0, 0, 0, 1));
		}

		bool rayIntersect
		(
			//Ray
			float3 O,//Origin
			float3 D, //Direction

					  //Sphere
			float3 C, //Center
			float R, //Radius
			out float AO, //First intersection time
			out float BO // Second intersection time
		)
		{
			float3 L = C - O;
			float DT = dot(L, D);
			float R2 = R * R;

			float CT2 = dot(L, L) - DT * DT;

			//Intersection point outside the circle
			if (CT2 > R2)
				return false;

			float AT = sqrt(R2 - CT2);
			float BT = AT;
			AO = DT - AT;
			BO = DT + BT;

			return true;
		}

		bool lightSampling
		(
			float3 P, //Current point within the atmospheric sphere
			float S, //Direction towards the sun
			out float opticalDepthCA
			)
		{
			float _; //Don't care about this one (?)
			float C;

			rayIntersect(P, S, _PlanetCenter, _AtmosphereRadius, _, C); //Ah, I see.

			//samples on segment PC
			float time = 0;
			float ds = distance(P, P + S * C) / (float)(_LightSamples);
			for (int i = 0; i < _LightSamples; i++)
			{
				float3 Q = P + S * (time + lightSampleSize * 0.5);
				float height = distance(_PlanetCenter, Q) - _PlanetRadius;

				//Inside the planet

				if (height < 0)
					return false;

				//Optical depth for the light ray
				opticalDepthCA += exp(-height / _RayScaleHeight) * ds;

				time += ds;
			}
			return true;
		}

		inline fixed4 LightingStandardScattering(SurfaceOutputStandard s, fixed3 viewDir, UnityGI gi)
		{
			float3 L = gi.light.dir;
			float3 V = viewDir;
			float3 N = s.Normal;

			float3 S = L; //Direction of light from the sun
			float3 D = -V; //Direction of view ray piercing the atmosphere

						   //Intersections with the actomspheric sphere
			float tA; //Atmosphere entry point (worldPos + V * tA)
			float tB; //Atmosphere exit point (worldPos + V * tB)

			float O;

			if (!rayIntersect(O, D, _PlanetCenter, _AtmosphereRadius, tA, tB))
				return fixed4(0, 0, 0, 0); //The view fays is looking into deep space

										   // Is the ray passing through the planet core?
			float pA, pB;
			if (rayIntersect(O, D, _PlanetCenter, _PlanetRadius, pA, pB))
				tB = pA;

			//Accumulator for the optical depth
			float opticalDepthPA = 0;


			//Numerical integration to calculate
			//the light contribution of each point P in AB
			float3 totalViewSamples = 0;
			float time = tA;
			float ds = (tB - tA) / (float)(_ViewSamples);

			if (overground)
			{
				//Point position
				//(sampling in the middle of the view sample segment
				float3 P = O + D * (time + ds * 0.5);

				//Optical depth of current segment
				// p(h) * ds
				float height = distance(C, P) - _PlanetRadius;
				float opticalDepthSegment = exp(-height / _ScaleHeight) * ds;

				// Accumulates the optical depths
				// D(PA)
				opticalDepthPA += opticalDepthSegment;


				// T(CP) * T(PA) * o(h) * ds
				totalViewSamples += viewSampling(P, ds);
				
				time += ds;
			}

			//I = I_S * 
			float3 I = _SunIntensity * _ScatteringCoefficient * phase * totalViewSamples;
		}

		void LightingStandardScattering_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
		{
			LightingStandard_GI(s, data, gi);
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{

		}
		ENDCG
		//======================
	}
	FallBack "Diffuse"
}
