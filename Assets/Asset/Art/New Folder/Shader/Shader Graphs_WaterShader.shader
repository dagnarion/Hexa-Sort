Shader "Shader Graphs/WaterShader" {
	Properties {
		Color_36218622185947c6a5ae36366d8e21d8 ("Deep Color", Vector) = (0.03921569,0.09803922,0.2745098,0.7058824)
		Color_93e06cd551a5449091bcde90b46765a0 ("Shallow Color", Vector) = (0.03804734,0.5490196,0.5490196,0.1960784)
		Vector1_6f56a0970372485390c6587863c2374e ("Depth", Float) = -3.5
		Vector1_6c82dffdd68049bcb019d3a9c64c92a0 ("Strenght", Range(0, 2)) = 0.2
		Vector1_6269b1025b26473ca8bc61634f34b537 ("Smoothness", Range(0, 1)) = 0.95
		[NoScaleOffset] Texture2D_6d0f902902b04ba687ee00a51db7ba6d ("Main Normal", 2D) = "white" {}
		[NoScaleOffset] Texture2D_786b67b3efe14204b2f06f9afb9d8cf1 ("Second Normal", 2D) = "white" {}
		Vector1_687f54e8c371429f86b9eaab0e7dfe3e ("Normal Strenght", Range(0, 1)) = 0.05
		Vector2_4351ac2be1d74054986ec5378db9d578 ("Normal Tiling", Vector) = (10,10,0,0)
		_Normal_Rotation ("Normal Rotation", Float) = 0
		[NoScaleOffset] Texture2D_28de85506601443d82b6148f21ccc69c ("Reflection Texture", 2D) = "white" {}
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