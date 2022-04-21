using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class GAMESRenderPipeline : RenderPipeline
{
    CameraRenderer renderer = new CameraRenderer();

    public GAMESRenderPipeline () {
		GraphicsSettings.useScriptableRenderPipelineBatching = true;
	}

    protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        Render(renderContext, new List<Camera>(cameras));
    }

    protected override void Render(ScriptableRenderContext renderContext, List<Camera> cameras)
    {
        foreach (Camera camera in cameras) {
			renderer.Render(renderContext, camera);
		}
    }
}
