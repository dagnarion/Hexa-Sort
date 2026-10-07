Shader "Shader Graphs/GemBallShader" {
	Properties {
		[NoScaleOffset] _TextureMap ("TextureMap", 2D) = "white" {}
		_TextureIntensity ("TextureIntensity", Float) = 0.46
		_GlowIntensity ("GlowIntensity", Float) = 0.14
		_Fresnal ("Fresnal", Float) = 0
		_EdgeSoftness2 ("EdgeSoftness2", Float) = 0.46
		_EdgeSoftness ("EdgeSoftness", Float) = 0
		[NoScaleOffset] _MatCap ("MatCap", 2D) = "white" {}
		[NoScaleOffset] _Casutic ("Casutic", 2D) = "white" {}
		_Amplitude ("Amplitude", Float) = 1
		_Step ("Step", Float) = 5
		_DepthColor ("DepthColor", Vector) = (0.4542542,0.6098285,0.8207547,0)
		_EffectTiling ("EffectTiling", Vector) = (2,2,0,0)
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] _XRMotionVectorsPass ("_XRMotionVectorsPass", Float) = 1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}