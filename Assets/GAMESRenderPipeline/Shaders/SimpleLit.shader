Shader "GRP/SimpleLit"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor]_BaseColor("Color", Color) = (1, 1, 1, 1)

        _SpecularStrength("SpecularStrength",Float) = 1.0
        
        [Toggle(_CLIPPING)]_Clipping ("Alpha Clipping", Float) = 0
        _Cutoff("Alpha CutOff",Range(0.0,1.0)) = 0.5

        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend ("Dst Blend", Float) = 0.0
        [Enum(Off, 0, On, 1)]_ZWrite("Z Write", Float) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
        }

        Pass
        {
            Name "Blinn-Phong"
            Tags { "LightMode" = "GRPSimpleLit" }

            // Use same blending / depth states as Standard shader
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]

            HLSLPROGRAM
            #pragma fragmentoption ARB_precision_hint_nicest
            #pragma target 3.5

            #pragma vertex SimpleLitVertex
            #pragma fragment SimpleLitFragment

            #include "SimpleLitInput.hlsl"
            #include "SimpleLitPass.hlsl"
            
            ENDHLSL
        }

        Pass
        {
            Name "Shadow"
            Tags { "LightMode" = "ShadowCaster"}

            ColorMask 0

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex ShadowCasterVertex
            #pragma fragment ShadowCasterFragment

            #include "ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
