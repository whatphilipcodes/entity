Shader "CustomRenderTexture/initCRT"
{
    Properties
    {
        _MainTex("InputTex", 2D) = "black" {}
    }

    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            Name "Init"

            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"

            #pragma vertex InitCustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

            sampler2D   _MainTex;

            float4 frag(v2f_init_customrendertexture IN) : COLOR
            {
                return tex2D(_MainTex, IN.texcoord.xy);
            }
            ENDCG
        }
    }
}
