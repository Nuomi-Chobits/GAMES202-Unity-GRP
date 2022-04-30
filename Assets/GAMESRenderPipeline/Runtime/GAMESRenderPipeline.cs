using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class GAMESRenderPipeline : RenderPipeline
{
    CameraRenderer renderer = new CameraRenderer();

    private ShadowSettings shadowSettings;
    private bool useDynamicBatching;
    private bool useGPUInstancing;

    public GAMESRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher, ShadowSettings shadowSettings)
    {

        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        this.shadowSettings = shadowSettings;
        //启用SRP合批处理
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        //灯光使用 LinearSpace
        GraphicsSettings.lightsUseLinearIntensity = true;
    }

    protected override void Render(ScriptableRenderContext renderContext, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            renderer.Render
            (
                renderContext,
                camera,
                useDynamicBatching,
                useGPUInstancing,
                shadowSettings
            );
        }
    }
}
