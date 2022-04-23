using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const string bufferName = "Lighting";
    CommandBuffer cmd = new CommandBuffer 
    {
		name = bufferName
	};

    private const int maxDirLightCount = 4;
    private static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    private static int dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors");
    private static int dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");   
    
    static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];

    CullingResults cullResults;

    public void Setup (ScriptableRenderContext context,CullingResults cullResults) 
    {
        this.cullResults = cullResults;
        cmd.BeginSample(bufferName);
        SetupLights();
        cmd.EndSample(bufferName);
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
    }

    void SetupDirectionalLight(int index, ref VisibleLight visibleLight) 
    {
        // VisibleLight.finalColor already returns color in active color space
        dirLightColors[index] = visibleLight.finalColor;
        //forward (0,0,1)
		dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
    }
}