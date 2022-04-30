using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "GAMES202/Rendering/new GRP Asset")]
public class GAMESRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField] ShadowSettings shadowSettings = default;
	[SerializeField] bool useDynamicBatching = true;
    [SerializeField] bool useGPUInstancing = true;
    [SerializeField] bool useSRPBatcher = true;
    
    protected override RenderPipeline CreatePipeline()
    {
        return new GAMESRenderPipeline
        (
            useDynamicBatching,
            useGPUInstancing,
            useSRPBatcher,
            shadowSettings
        );
    }     
}

