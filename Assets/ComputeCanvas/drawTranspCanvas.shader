Shader "ComputeShaderAddons/drawTranspCanvas"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Opacity ("Opacity", Range(0.0, 1.0)) = 1.0
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent" // tag to inform the render pipeline of what type this is
            "Queue"="Transparent" // changes the render order
        }

        Pass
        {
            Cull Off
            ZWrite Off
            Blend One One // additive
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Opacity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture and apply opacity
                fixed4 col = tex2D(_MainTex, i.uv) * _Opacity;

                //return
                return col;
            }
            ENDCG
        }
    }
}
