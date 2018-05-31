Shader "Custom/MonsterShader" 
{
	Properties 
	{
		//Unlit Properties
		_Tint ("Tint Color", Color) = (1,0,0,1)
		_HurtValue("Tint Opacity", Range(0.0, 1.0)) = 0.0

		_Color ("Colorization", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}

		_NormalTex("Normal Map", 2D) = "bump" {}
		_NormalScale("Normal Map Scale", Float) = 1.0

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_GlossTex("Smoothness Map", 2D) = "white"{}

		_Metallic ("Metallic", Range(0,1)) = 0.0
		_MetalTex("Metallic Map", 2D) = "white" {}

		_EmissiveColor("Emission Color", Color) = (1, 1, 1, 1)
		_EmissiveTex("Emmission Map", 2D) = "white" {}

		_DissolveScale("Dissolve Scale", Range(0,1)) = 0.0
		_DissolveTex("Dissolve Texture", 2D) = "white"{}
		_DissolveStart("Dissolve Start Point", Vector) = (1, 1, 1, 1)
		_DissolveEnd("Dissolve End Point", Vector) = (0, 0, 0, 1)
		_DissolveBand("Dissolve Band Size", Float) = 0.25

		_Glow("Glow Color", Color) = (1, 1, 1, 1)
		_GlowIntensity("Glow Intensity", Range(0.0,5.0)) = 0.05
		_GlowScale("Glow Size", Range(0.0,5.0)) = 1.0
		_GlowEnd("Glow End Color", Color) = (1, 1, 1, 1)
		_GlowColShift("Glow Colorshift", Range(0.0,2.0)) = 0.075

	}
	SubShader 
	{

		Pass
		{
			ZWrite On
			ColorMask 0
		}
		Tags 
		{ 
			"Queue" = "Transparent"
			"RenderType"="Fade" 
		}
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:fade vertex:vert

		// Custom Unlit surface shader, allows up to blend in the tint color over the top of the rest of the materials
		//#pragma surface surf Unlit fullforwardshadows 

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		

		sampler2D _MainTex;
		sampler2D _DissolveTex;
		sampler2D _MetalTex;
		sampler2D _GlossTex;
		sampler2D _EmissiveTex;
		sampler2D _NormalMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_MetalTex;
			float2 uv_EmissiveTex;
			float2 uv_DissolveTex;
			float2 uv_GlossTex;
			float2 uv_NormalMap;
			float3 dGeometry;
		};

		half _Glossiness;
		half _Metallic;
		half _DissolveScale;
		half _GlowScale;
		half _GlowIntensity;
		half _GlowColShift;
		float _NormalScale;
		fixed4 _EmissiveColor;
		fixed4 _Glow;
		fixed4 _GlowEnd;
		fixed4 _Color;
		float _DissolveBand;
		float4 _DissolveStart;
		float4 _DissolveEnd;

		//Precompute dissolve direction
		static float3 dDir = normalize(_DissolveEnd - _DissolveStart);

		//Precompute gradient start position
		static float3 dissolveStartConverted = _DissolveStart - _DissolveBand * dDir;

		//Precompute reciprocal of band size.
		static float dBandFactor = 1.0f / _DissolveBand;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;

			o.Normal = UnpackNormal (tex2D(_NormalMap, IN.uv_NormalMap) * _NormalScale);

			//Convert dissolve progression to -1 to 1 scale.
			half dBase = -2.0f * _DissolveScale + 1.0f;
			//Read from noise texture,
			fixed4 dTex = tex2D(_DissolveTex, IN.uv_MainTex);
			//Convert dissolve texture sample based on dissolve progression
			half dTexRead = dTex.r + dBase;
			//Combine texture factor with geometry coefficient from vertex routine.
			half dFinal = dTexRead + IN.dGeometry;
			//Set output alpha value
			half alpha = clamp(dFinal, 0.0f, 1.0f);
			o.Alpha = alpha;

			fixed4 metal = tex2D(_MetalTex, IN.uv_MetalTex) * _Metallic;
			// Metallic and smoothness come from slider variables
			o.Metallic = metal;
			o.Smoothness = _Glossiness;

			//Prep base Emissive tex for blending.
			fixed4 eTex = tex2D(_EmissiveTex, IN.uv_EmissiveTex) * _EmissiveColor;


			//Shift the computed raw alhpa value based on the scale factor of the glow.
			//Scale the shifted value based on effect intensity.
			half dPredict = (_GlowScale - dFinal) * _GlowIntensity;
			//Change color interpolation by adding in a nother factor controlling the gradient.
			half dPredictCol = (_GlowScale * _GlowColShift - dFinal) * _GlowIntensity;

			//Calculate and clamp glow color.
			fixed4 glowCol = dPredict * lerp(_Glow, _GlowEnd, clamp(dPredictCol, 0.0f, 1.0f));
			glowCol = clamp(glowCol, 0.0f, 1.0f);

			//Composite the glow color and emissive texture
			fixed4 eFinal = eTex + glowCol;

			o.Emission = eFinal;

		}
		
		
		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);

			//Calculate deometry-based disslove coefficient.
			//Compute top of dissolution gradient according to dissolve progression.
			float3 dPoint = lerp(dissolveStartConverted, _DissolveEnd, _DissolveScale);

			//Project vector between current vertex and top of gradient onto dissolve direction.
			//Scale coeficient by band (gradient) size.
			o.dGeometry = dot(v.vertex - dPoint, dDir) * dBandFactor;
		}
		
		inline half4 LightingCustom(SurfaceOutputStandard s, half3 lightDir, UnityGI gi)
		{
			return LightingStandard(s, lightDir, gi);
		}

		inline void LightingCustom_GI(SurfaceOutputStandard s, UnityGIInput data, inout UnityGI gi)
		{
			LightingStandard_GI(s, data, gi);
		}

		ENDCG
	}
	FallBack "Diffuse"
}
