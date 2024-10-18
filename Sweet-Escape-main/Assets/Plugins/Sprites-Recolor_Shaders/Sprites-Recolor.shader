Shader "Sprites/Recolor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0

        [Header(Recolor 0)] [Space]
        _SourceMask_0 ("Source Mask 0", 2D) = "white" {}
        _SourceColor_0 ("Source Color 0", Color) = (1,1,1,1)
        _SourceColorRange_0 ("Source Color Range 0", Vector) = (0.001, 0.001, 0.001, 0.999)
        _ReplacementColor_0 ("Replacement Color 0", Color) = (1,1,1,1)

        [Header(Recolor 1)] [Space]
        _SourceMask_1 ("Source Mask 1", 2D) = "white" {}
        _SourceColor_1 ("Source Color 1", Color) = (1,1,1,1)
        _SourceColorRange_1 ("Source Color Range 1", Vector) = (0.001, 0.001, 0.001, 0.999)
        _ReplacementColor_1 ("Replacement Color 1", Color) = (1,1,1,1)

        [Header(Recolor 2)][Space]
        _SourceMask_2 ("Source Mask 2", 2D) = "white" {}
        _SourceColor_2 ("Source Color 2", Color) = (1,1,1,1)
        _SourceColorRange_2 ("Source Color Range 2", Vector) = (0.001, 0.001, 0.001, 0.999)
        _ReplacementColor_2 ("Replacement Color 2", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "RecolorSprites.cginc"
        ENDCG
        }
    }
}