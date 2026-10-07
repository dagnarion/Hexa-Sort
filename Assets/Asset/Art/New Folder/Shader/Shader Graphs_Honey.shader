Shader "Shader Graphs/Honey" {
	Properties {
		[NoScaleOffset] _Texture ("Texture", 2D) = "white" {}
		[NoScaleOffset] _Matcap ("Matcap", 2D) = "white" {}
		_Emission_Intensity ("Emission Intensity", Float) = 0
		_Deep_Colour ("Deep Colour", Vector) = (0.9056604,0.3595015,0,0)
		_Shallow_Colour ("Shallow Colour", Vector) = (1,1,1,0)
		_Translucency ("Translucency", Float) = 0
		_Fersnal ("Fersnal", Float) = 1.87
		_Softness1 ("Softness1", Float) = -0.04
		_Softness2 ("Softness2", Float) = 0.31
		_Smoothness ("Smoothness", Float) = 0
		_Spec ("Spec", Vector) = (0,0,0,0)
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