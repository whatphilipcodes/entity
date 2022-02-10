Shader "CustomRenderTexture/updateCRT"
{
    Properties
    {
        _MainTex("InputTex", 2D) = "black" {}
        _a ("Point A", Vector) = (0,0,0,0)
        _b ("Point B", Vector) = (1,1,0,0)
        _thick ("Thickness", float) = 0.01
        _crisp ("Crispness", float) = 1
        _bgcol("BG Color", Color) = (0,0,0,0)
        _lcol("Line Color", Color) = (1,1,1,1)

        _Coordinate("Coordinate", Vector) = (0.0,0.5,0,0)
        _Color("Draw Color", Color) = (1,0,0,0)
     }

     SubShader
     {
        Lighting Off
        Blend One Zero

        Pass
        {
            Name "Update"

            CGPROGRAM
            #include "UnityCustomRenderTexture.cginc"
            #pragma vertex CustomRenderTextureVertexShader
            #pragma fragment frag
            #pragma target 3.0

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

            sampler2D   _MainTex;
            float4      _Coordinate;
            float4      _Color;

            // SDF values
            float2 _a;
            float2 _b;

            float _thick;
            float _crisp;

            float4 _bgcol = (0,0,0,0);
            float4 _lcol;
            //////////////

            float4 frag(v2f_customrendertexture IN) : COLOR
            {   /*
                float2 uv = IN.localTexcoord.xy;
                float4 lastFrame = tex2D(_SelfTexture2D, uv);

                float l = DrawLine(uv, _a, _b, _thick, _crisp);
                float4 col = lerp(_bgcol, _lcol, l);
                return col;
                //return col + lastFrame;
                */
                // _MainTex_TexelSize -> Vector4(1 / width, 1 / height, width, height)
                // https://docs.unity3d.com/2021.2/Documentation/Manual/SL-PropertiesInPrograms.html
                
                float2 uv = IN.localTexcoord.xy;

                float4 lastFrame = tex2D(_SelfTexture2D, uv);
                float draw = pow(saturate(1 - distance(uv, _Coordinate.xy)), 200);
                float4 drawcol = _Color * (draw * 1);
                return smoothstep(0,1,lastFrame + drawcol);
            }
            ENDCG
        }
    }
}