using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

partial class CameraRenderer
{
	partial void DrawUnsupportedShaders();
	partial void DrawGizmos();
	partial void PrepareForSceneWindow();
	partial void PrepareBuffer();

#if UNITY_EDITOR
	static Material errorMaterial;

	string SampleName { get; set; }

	static ShaderTagId[] legacyShaderTagIds = {
		new ShaderTagId("Always"),
		new ShaderTagId("ForwardBase"),
		new ShaderTagId("PrepassBase"),
		new ShaderTagId("Vertex"),
		new ShaderTagId("VertexLMRGBM"),
		new ShaderTagId("VertexLM")
	};

	partial void DrawUnsupportedShaders ()
	{
		if (errorMaterial == null) {
			errorMaterial =
				new Material(Shader.Find("Hidden/InternalErrorShader"));
		}

		var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera)){
			overrideMaterial = errorMaterial
		};

		for (int i = 1; i < legacyShaderTagIds.Length; i++) {
			drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
		}

		var filteringSettings = FilteringSettings.defaultValue;

		renderContext.DrawRenderers(
			cullResults, ref drawingSettings, ref filteringSettings
		);

	}

	partial void DrawGizmos () {
		if (Handles.ShouldRenderGizmos()) {
			renderContext.DrawGizmos(camera, GizmoSubset.PreImageEffects);
			renderContext.DrawGizmos(camera, GizmoSubset.PostImageEffects);
		}
	}

	partial void PrepareForSceneWindow () {
		if (camera.cameraType == CameraType.SceneView) {
			ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
		}
	}

	partial void PrepareBuffer (){
		Profiler.BeginSample("Editor Only");
		cmd.name = SampleName = camera.name;
		Profiler.EndSample();
	}
#else
	const string SampleName => bufferName;
#endif	
}
