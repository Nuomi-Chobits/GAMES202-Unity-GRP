using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{
    ScriptableRenderContext renderContext;

    Camera camera;
	CullingResults cullResults;

	const string bufferName = "GRP Single Camera";
	CommandBuffer cmd = new CommandBuffer {name = bufferName};

	static ShaderTagId defaultLitShaderTagId = new ShaderTagId("GRPSimpleLit");

	public void Render (ScriptableRenderContext renderContext, Camera camera) {
		this.renderContext = renderContext;
		this.camera = camera;

		PrepareBuffer();
		PrepareForSceneWindow();

		if (!Cull()) {
			return;
		}

		Setup();
		DrawVisibleGeometry();
		DrawUnsupportedShaders();
		DrawGizmos();
		Submit();
	}

	void Setup () {
		renderContext.SetupCameraProperties(camera);
		CameraClearFlags flags = camera.clearFlags;
		cmd.ClearRenderTarget(
			flags <= CameraClearFlags.Depth, 
			flags == CameraClearFlags.Color, 
			flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear
		);
		cmd.BeginSample(SampleName);
		ExecuteBuffer();
	}

	void DrawVisibleGeometry () {
		var sortingSettings = new SortingSettings(camera){
			criteria = SortingCriteria.CommonOpaque
		};
		var drawingSettings = new DrawingSettings(
			defaultLitShaderTagId,sortingSettings
		);
		var filteringSettings = new FilteringSettings(RenderQueueRange.all);

		renderContext.DrawRenderers(
			cullResults, ref drawingSettings, ref filteringSettings
		);

		renderContext.DrawSkybox(camera);

		sortingSettings.criteria = SortingCriteria.CommonTransparent;							
		drawingSettings.sortingSettings = sortingSettings;											
		filteringSettings.renderQueueRange = RenderQueueRange.transparent;	

		renderContext.DrawRenderers(
			cullResults, ref drawingSettings, ref filteringSettings
		);
	}

	bool Cull () {
		//TryGetCullingParameters()
		if(camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParams))
		{
			cullResults = renderContext.Cull(ref cullingParams);
			return true;
		} 
			
		return false;
	}

	void Submit () {
		cmd.EndSample(SampleName);
		ExecuteBuffer();
		renderContext.Submit();
	}

	void ExecuteBuffer () {
		renderContext.ExecuteCommandBuffer(cmd);
		cmd.Clear();
	}    
}
