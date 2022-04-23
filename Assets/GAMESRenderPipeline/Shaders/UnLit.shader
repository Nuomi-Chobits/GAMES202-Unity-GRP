Shader "GRP/UnLit"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor]_BaseColor("Color", Color) = (1, 1, 1, 1)

        [Toggle(_CLIPPING)]_Clipping ("Alpha Clipping", Float) = 0
        _Cutoff("Alpha CutOff",Range(0.0,1.0)) = 0.5

        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend ("Dst Blend", Float) = 0.0
        [Enum(Off, 0, On, 1)]_ZWrite("Z Write", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "UnLit"

            Blend[_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #pragma shader_feature _CLIPPING
            #pragma vertex UnLitVertex
            #pragma fragment UnLitFragment

            #include "Assets/GAMESRenderPipeline/Shaders/UnLitInput.hlsl"
            #include "Assets/GAMESRenderPipeline/Shaders/UnLitPass.hlsl"
            
            ENDHLSL
        }
    }
}
