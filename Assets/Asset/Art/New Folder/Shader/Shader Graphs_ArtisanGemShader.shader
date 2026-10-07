Shader "Shader Graphs/ArtisanGemShader" {
	Properties {
		[NoScaleOffset] _TextureMap ("TextureMap", 2D) = "white" {}
		[NoScaleOffset] _MatCap ("MatCap", 2D) = "white" {}
		[NoScaleOffset] _Casutic ("Casutic", 2D) = "white" {}
		_Amplitude ("Amplitude", Float) = 1
		_Step ("Step", Float) = 5
		_DepthColor ("DepthColor", Vector) = (0.4542542,0.6098285,0.8207547,0)
		[NoScaleOffset] _DepthTexture ("DepthTexture", 2D) = "white" {}
		_EffectTiling ("EffectTiling", Vector) = (2,2,0,0)
		[ToggleUI] _Boolean ("Boolean", Float) = 0
		_Color ("Color", Vector) = (1,1,1,0)
		_Brightness ("Brightness", Vector) = (0.5,0.03616348,0.03616348,0)
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] _XRMotionVectorsPass ("_XRMotionVectorsPass", Float) = 1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
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

			float4 _Color;

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return _Color; // RGBA
			}

			ENDHLSL
		}
	}
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}