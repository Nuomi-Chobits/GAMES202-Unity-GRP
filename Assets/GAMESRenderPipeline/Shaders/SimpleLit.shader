Shader "GRP/SimpleLit"
{
    Properties
    {
        _BaseMap ("BaseMap", 2D) = "white" {}
        _SpecularStrength("Specular Strength",Range(0,1)) = 0.1

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
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
            Cull[_Cull]

            HLSLPROGRAM
            #pragma fragmentoption ARB_precision_hint_nicest

            #pragma vertex SimpleLitVertex
            #pragma fragment SimpleLitFragment

            #include "SimpleLitInput.hlsl"
            #include "SimpleLitPass.hlsl"
            
            ENDHLSL
        }
    }
}
