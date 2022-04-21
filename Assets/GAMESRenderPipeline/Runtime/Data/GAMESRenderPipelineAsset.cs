using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "GAMES202/Rendering/new GRP Asset")]
public class GAMESRenderPipelineAsset : RenderPipelineAsset
{
    protected override RenderPipeline CreatePipeline()
    {
        return new GAMESRenderPipeline();
    }     
}

