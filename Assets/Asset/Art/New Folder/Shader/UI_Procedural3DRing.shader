Shader "UI/Procedural3DRing" {
	Properties {
		_Fill ("Fill Amount", Range(0, 1)) = 0.5
		_Radius ("Ring Radius", Range(0, 1)) = 0.4
		_Thickness ("Line Thickness", Range(0, 1)) = 0.2
		[Space(10)] _ColorStart ("Ring Color (Arc Start)", Vector) = (0.1,0.8,0.2,1)
		_ColorEnd ("Ring Color (Arc End)", Vector) = (0.1,0.8,0.2,1)
		_HighlightStrength ("Center Highlight Brightness", Range(0, 1)) = 0.25
		_ShadowStrength ("Edge Shadow Darkness", Range(0, 1)) = 0.6
		_ShadowThickness ("Edge Shadow Thickness", Range(0, 1)) = 0.3
		_MidtoneThickness ("Midtone Thickness", Range(0, 1)) = 0.75
		_ShadowFeather ("Shadow-Midtone Feather", Range(0.01, 0.5)) = 0.35
		_MidtoneFeather ("Midtone-Highlight Feather", Range(0.01, 0.5)) = 0.25
		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector] _ColorMask ("Color Mask", Float) = 15
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
}