Shader "Custom/DissolveShader" 
{
	Properties 
	{
		//Base Rendering Stuff
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Emission("Emission", Color) = (1, 1, 1, 1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_EmissionTex("Emission", 2D) = "white" {}
		_NormalTex("Normal Map", 2D) = "bump" {}
		_MetallicTex("Metallic", 2D) = "white" {}

		//Dissolve Stuff
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

		//Hurt Stuff
		_HurtColor("HurtColor", Color) = (1, 0, 0, 0)
		_HurtScale("Hurt Toggle", Range(0.0,1.0)) = 0.0
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
			"RenderType" = "Fade"
		}
		LOD 200
			

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha:fade vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 4.6

		sampler2D _MainTex;
		sampler2D _EmissionTex;
		sampler2D _NormalTex;
		sampler2D _MetallicTex;

		sampler2D _DissolveTex;

		struct Input 
		{
			float2 uv_MainTex;
			float2 uv_EmissionTex;
			float2 uv_NormalTex;
			float2 uv_MetallicTex;

			float3 dGeometry;
			float2 uv_DissolveTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _Emission;

		half _DissolveScale;
		half _GlowScale;
		half _GlowIntensity;
		half _GlowColShift;
		fixed4 _Glow;
		fixed4 _GlowEnd;
		float _DissolveBand;
		float4 _DissolveStart;
		float4 _DissolveEnd;

		float4 _HurtColor;
		half _HurtScale;


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

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			//Metallic and Smoothness come from a texture (and one slider)
			fixed4 cSpec = tex2D(_MetallicTex, IN.uv_MetallicTex);
			o.Metallic = cSpec.r;
			o.Smoothness = cSpec.a * _Glossiness;

			o.Normal = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));

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

			//Shift the computed raw alhpa value based on the scale factor of the glow.
			//Scale the shifted value based on effect intensity.
			half dPredict = (_GlowScale - dFinal) * _GlowIntensity;
			//Change color interpolation by adding in another factor controlling the gradient.
			half dPredictCol = (_GlowScale * _GlowColShift - dFinal) * _GlowIntensity;

			//Calculate and clamp glow color.
			fixed4 glowCol = dPredict * lerp(_Glow, _GlowEnd, clamp(dPredictCol, 0.0f, 1.0f));
			glowCol = clamp(glowCol, 0.0f, 1.0f);

			fixed4 e = tex2D(_EmissionTex, IN.uv_EmissionTex) * _Emission;

			fixed4 hurtCol = _HurtScale * _HurtColor;
			hurtCol = clamp(hurtCol, 0.0f, 1.0f);

			o.Emission = e + glowCol + hurtCol;
			
			o.Alpha = alpha;
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
		ENDCG

	}
		
	FallBack "VertexLit"
}
