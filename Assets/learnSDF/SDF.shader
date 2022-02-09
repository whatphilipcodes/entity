Shader "Custom/SDF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _a ("Point A", Vector) = (0,0,0,0)
        _b ("Point B", Vector) = (0,0,0,0)
        _thick ("Thickness", float) = 0
        _crisp ("Crispness", float) = 0
        _bgcol("BG Color", Color) = (0,0,0)
        _lcol("Line Color", Color) = (1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Line Segment Functions (NedMakesGames)
            #ifndef LINE_SEGMENT_2D_SDF_DEFINED
            #define LINE_SEGMENT_2D_SDF_DEFINED
            float LineSegment2DSDF(float2 p, float2 a, float2 b)
            {
                float2 ba = b - a;
                float2 pa = p - a;
                float k = saturate(dot(pa, ba) / dot(ba, ba));
                return length(pa - ba * k);
            }
            #endif

            float DrawLine(float2 i, float2 a, float2 b, float thick, float crisp)
            {
                float ct = thick + crisp;
                float d = LineSegment2DSDF(i,a,b);
                float o = smoothstep(thick, ct, d);
                return 1 - o;
            }
            ///////////////

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

            // SDF values
            float2 _a;
            float2 _b;

            float _thick;
            float _crisp;

            float4 _bgcol;
            float4 _lcol;
            //////////////

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float l = DrawLine(i.uv, _a, _b, _thick, _crisp);
                float4 col = lerp(_bgcol, _lcol, l);
                return col;
                /*
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
                */
            }
            ENDCG
        }
    }
}
