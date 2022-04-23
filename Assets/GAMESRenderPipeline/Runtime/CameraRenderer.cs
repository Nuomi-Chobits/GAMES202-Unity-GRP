using UnityEngine;
using UnityEngine.Rendering;

public partial class CameraRenderer
{	
	
    private ScriptableRenderContext renderContext;
	private const string bufferName = "GRP Single Camera";
	private CullingResults cullResults;

	private Camera camera;
	private Lighting lighting = new Lighting();
	
	private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
	private static ShaderTagId defaultLitShaderTagId = new ShaderTagId("GRPSimpleLit");

	CommandBuffer cmd = new CommandBuffer {name = bufferName};

	public void Render (ScriptableRenderContext renderContext, Camera camera,bool useDynamicBatching,bool useGPUInstancing) {
		this.renderContext = renderContext;
		this.camera = camera;

		PrepareBuffer();
		PrepareForSceneWindow();

		//执行剔除
		if (!Cull()) {
			return;
		}

		Setup();
		lighting.Setup(renderContext,cullResults);
		DrawVisibleGeometry(useDynamicBatching,useGPUInstancing);
		DrawUnsupportedShaders();
		DrawGizmos();
		Submit();
	}

	void Setup () {
		renderContext.SetupCameraProperties(camera);
		CameraClearFlags flags = camera.clearFlags;
		//清空渲染目标，深度、颜色、使用何种颜色进行清空
		cmd.ClearRenderTarget(
			flags <= CameraClearFlags.Depth, 
			flags == CameraClearFlags.Color, 
			flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear
		);
		cmd.BeginSample(SampleName);
		ExecuteBuffer();
	}

	void DrawVisibleGeometry (bool useDynamicBatching,bool useGPUInstancing) {
		var sortingSettings = new SortingSettings(camera)
		{
			criteria = SortingCriteria.CommonOpaque
		};
		var drawingSettings = new DrawingSettings(unlitShaderTagId,sortingSettings)
		{
			enableDynamicBatching = useDynamicBatching,
			enableInstancing = useGPUInstancing
		};
		
		drawingSettings.SetShaderPassName(1, defaultLitShaderTagId);

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
