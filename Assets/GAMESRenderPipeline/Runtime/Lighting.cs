using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const string cmdName = "Lighting";
    CommandBuffer cmd = new CommandBuffer
    {
        name = cmdName
    };

    private const int maxDirLightCount = 4;

    private static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    private static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    private static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");
    private static int dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");

    Shadows shadows = new Shadows();

    static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];
    static Vector4[] dirLightShadowData = new Vector4[maxDirLightCount];

    CullingResults cullResults;

    public void Setup(ScriptableRenderContext context, CullingResults cullResults, ShadowSettings shadowSettings)
    {
        this.cullResults = cullResults;
        cmd.BeginSample(cmdName);
        shadows.Setup(context, cullResults, shadowSettings);
        SetupLights();
        shadows.Render();
        cmd.EndSample(cmdName);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(i, ref visibleLight);
                if (dirLightCount >= maxDirLightCount)
                {
                    break;
                }
            }
        }

        cmd.SetGlobalInt(dirLightCountId, visibleLights.Length);
        cmd.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        cmd.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);
        cmd.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
    }

    void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        // VisibleLight.finalColor already returns color in active color space
        dirLightColors[index] = visibleLight.finalColor;
        //forward (0,0,1)
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
        dirLightShadowData[index] = shadows.ReserveDirectionalShadows(visibleLight.light, index);
    }

    public void Cleanup()
    {
        shadows.Cleanup();
    }
}
