using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    const string cmdName = "Shadows";
    const int maxShadowedDirectionalLightCount = 4;

    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    static int dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
    static int shadowAtlasSizeId = Shader.PropertyToID("_ShadowAtlasSize");

    static string[] directionalFilterKeywords = {
        "_DIRECTIONAL_PCF3",
        "_DIRECTIONAL_PCF5",
        "_DIRECTIONAL_PCF7",
    };

    static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount];

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
            switch(shadowSettings.shadowType)
            {
                case ShadowSettings.ShadowType.SM:
                    RenderDirectionalSM();
                    break;
                case ShadowSettings.ShadowType.CSM:
                    RenderDirectionalCSM();
                    break;
            }
     
        }
        else
        {
            //兼容webgl
            cmd.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }

    //SM
    void RenderDirectionalSM()
    {
        int shadowResolution = (int)shadowSettings.shadowResolution;
        cmd.GetTemporaryRT(dirShadowAtlasId, shadowResolution, shadowResolution, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        //RenderBufferLoadAction.Load : When this RenderBuffer is activated, preserve the existing contents of it. This setting is expensive on tile-based GPUs and should be avoided whenever possible.
        //RenderBufferLoadAction.DontCare : When this RenderBuffer is activated, the GPU is instructed not to care about the existing contents of that RenderBuffer. On tile-based GPUs this means that the RenderBuffer contents do not need to be loaded into the tile memory, providing a performance boost.
        cmd.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        cmd.ClearRenderTarget(true, false, Color.clear);
        cmd.BeginSample(cmdName);

        int split = shadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = shadowResolution / split;

        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split, tileSize);
        }
        cmd.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
        SetKeywords();
        cmd.SetGlobalVector(shadowAtlasSizeId, new Vector4(tileSize, 1f / tileSize));
        cmd.EndSample(cmdName);
        ExecuteBuffer();
    }

    void RenderDirectionalCSM()
    {
        int shadowResolution = (int)shadowSettings.shadowResolution;
        cmd.GetTemporaryRT(dirShadowAtlasId, shadowResolution, shadowResolution, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        //RenderBufferLoadAction.Load : When this RenderBuffer is activated, preserve the existing contents of it. This setting is expensive on tile-based GPUs and should be avoided whenever possible.
        //RenderBufferLoadAction.DontCare : When this RenderBuffer is activated, the GPU is instructed not to care about the existing contents of that RenderBuffer. On tile-based GPUs this means that the RenderBuffer contents do not need to be loaded into the tile memory, providing a performance boost.
        cmd.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        cmd.ClearRenderTarget(true, false, Color.clear);
        cmd.BeginSample(cmdName);
        int split = shadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = shadowResolution / split;

        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split, tileSize);
        }
        cmd.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
        SetKeywords();
        cmd.EndSample(cmdName);
        ExecuteBuffer();
    }

    void SetKeywords()
    {
        int enabledIndex = (int)shadowSettings.shadowFilterMode;
        for (int i = 0; i < directionalFilterKeywords.Length; i++)
        {
            if (i == enabledIndex)
            {
                cmd.EnableShaderKeyword(directionalFilterKeywords[i]);
            }
            else
            {
                cmd.DisableShaderKeyword(directionalFilterKeywords[i]);
            }
        }
    }
    void RenderDirectionalShadows(int index, int split, int tileSize)
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[index];
        var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        //计算sm需要的信息
        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(
            light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f,
            out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix, out ShadowSplitData shadowSplitData
        );
        //https://docs.unity3d.com/Manual/SL-PlatformDifferences.html
        //如果是传递LVP（LightViewProjection）矩阵到shader，需要进行如下操作
        //projMatrix = GL.GetGPUProjectionMatrix(projMatrix, true);
        //_M = projMatrix * viewMatrix;
        //shader中:
        //mul(_M, posWS));
        shadowSettings.splitData = shadowSplitData;
        dirShadowMatrices[index] = CalcLightMVP(
            projMatrix ,
            viewMatrix,
            SetTileViewport(index, split, tileSize),
            split
        );
        cmd.SetViewProjectionMatrices(viewMatrix, projMatrix);
        cmd.SetGlobalDepthBias(30000f, 0f);
        ExecuteBuffer();
        renderContext.DrawShadows(ref shadowSettings);
        cmd.SetGlobalDepthBias(0f, 0f);
    }

    //也可以借鉴URP ShadowUtils.GetShadowTransform 写法
    static Matrix4x4 CalcLightMVP(Matrix4x4 proj,Matrix4x4 view, Vector2 offset, int split)
    {
        // Currently CullResults ComputeDirectionalShadowMatricesAndCullingPrimitives doesn't
        // apply z reversal to projection matrix. We need to do it manually here.
        if (SystemInfo.usesReversedZBuffer)
        {
            proj.m20 = -proj.m20;
            proj.m21 = -proj.m21;
            proj.m22 = -proj.m22;
            proj.m23 = -proj.m23;
        }

        Matrix4x4 worldToShadow = proj * view;
        float scale = 1f / split;

        worldToShadow.m00 = (0.5f * (worldToShadow.m00 + worldToShadow.m30) + offset.x * worldToShadow.m30) * scale;
        worldToShadow.m01 = (0.5f * (worldToShadow.m01 + worldToShadow.m31) + offset.x * worldToShadow.m31) * scale;
        worldToShadow.m02 = (0.5f * (worldToShadow.m02 + worldToShadow.m32) + offset.x * worldToShadow.m32) * scale;
        worldToShadow.m03 = (0.5f * (worldToShadow.m03 + worldToShadow.m33) + offset.x * worldToShadow.m33) * scale;
        worldToShadow.m10 = (0.5f * (worldToShadow.m10 + worldToShadow.m30) + offset.x * worldToShadow.m30) * scale;
        worldToShadow.m11 = (0.5f * (worldToShadow.m11 + worldToShadow.m31) + offset.x * worldToShadow.m31) * scale;
        worldToShadow.m12 = (0.5f * (worldToShadow.m12 + worldToShadow.m32) + offset.x * worldToShadow.m32) * scale;
        worldToShadow.m13 = (0.5f * (worldToShadow.m13 + worldToShadow.m33) + offset.x * worldToShadow.m33) * scale;
        worldToShadow.m20 = 0.5f * (worldToShadow.m20 + worldToShadow.m30);
        worldToShadow.m21 = 0.5f * (worldToShadow.m21 + worldToShadow.m31);
        worldToShadow.m22 = 0.5f * (worldToShadow.m22 + worldToShadow.m32);
        worldToShadow.m23 = 0.5f * (worldToShadow.m23 + worldToShadow.m33);

        return worldToShadow;
    }

    Vector2 SetTileViewport(int index, int split, float tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        cmd.SetViewport(new Rect(
            offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
        ));
        return offset;
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

    public Vector2 ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount && light.shadows != LightShadows.None
            && light.shadowStrength > 0f && cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds outBounds))
        {
            shadowedDirectionalLights[shadowedDirectionalLightCount] = new ShadowedDirectionalLight
            {
                visibleLightIndex = visibleLightIndex
            };
            return new Vector2(
                light.shadowStrength, shadowedDirectionalLightCount++
            );
        }
        return Vector2.zero;
    }
}
