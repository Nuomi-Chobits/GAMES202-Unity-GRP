using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    const string cmdName = "Shadows";
    const int maxShadowedDirectionalLightCount = 1;

    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

    CommandBuffer cmd = new CommandBuffer { name = cmdName };

    ScriptableRenderContext renderContext;
    CullingResults cullingResults;
    ShadowSettings shadowSettings;

    int shadowedDirectionalLightCount;

    ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    public void Setup(ScriptableRenderContext renderContext, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.renderContext = renderContext;
        this.cullingResults = cullingResults;
        this.shadowSettings = shadowSettings;
        shadowedDirectionalLightCount = 0;
    }

    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            //兼容webgl
            //为了减少因SM缺失生成的着色器变体生成一张1px的SM，管线上这样设计正确性存疑
            cmd.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }

    void RenderDirectionalShadows()
    {
        int shadowResolution = (int)shadowSettings.shadowResolution;
        cmd.GetTemporaryRT(dirShadowAtlasId, shadowResolution, shadowResolution, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        //RenderBufferLoadAction.Load : When this RenderBuffer is activated, preserve the existing contents of it. This setting is expensive on tile-based GPUs and should be avoided whenever possible.
        //RenderBufferLoadAction.DontCare : When this RenderBuffer is activated, the GPU is instructed not to care about the existing contents of that RenderBuffer. On tile-based GPUs this means that the RenderBuffer contents do not need to be loaded into the tile memory, providing a performance boost.
        cmd.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        cmd.ClearRenderTarget(true, false, Color.clear);
        cmd.BeginSample(cmdName);
        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, shadowResolution);
        }
        cmd.EndSample(cmdName);
        ExecuteBuffer();
    }

    void RenderDirectionalShadows(int index,int shadowResolution)
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(cullingResults,light.visibleLightIndex);
        //计算sm需要的信息
        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.visibleLightIndex, 0, 1, Vector3.zero, shadowResolution,0f,
            out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData
        );
        shadowSettings.splitData = shadowSplitData;
        cmd.SetViewProjectionMatrices(viewMatrix, projMatrix);
        ExecuteBuffer();
        renderContext.DrawShadows(ref shadowSettings);
    }
    public void Cleanup()
    {
        cmd.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();
    }

    void ExecuteBuffer()
    {
        renderContext.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount && light.shadows != LightShadows.None
            && light.shadowStrength > 0f && cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds outBounds))
        {
            shadowedDirectionalLights[shadowedDirectionalLightCount++] = new ShadowedDirectionalLight
            {
                visibleLightIndex = visibleLightIndex
            };
        }
    }
}
