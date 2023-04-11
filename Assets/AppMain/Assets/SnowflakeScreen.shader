Shader "MyShader/SnowflakeScreen"
{
    Properties
    {
    }
    SubShader
    {
        pass
        {
            Tags{ "Lighting" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Lighting.cginc"

            struct a2v
            {
                float4 vertex:POSITION;
                float2 texcoord:TEXCOORD0;
            };

            struct v2f
            {
                float4 pos:SV_POSITION;
                float2 uv:TEXCOORD1;
            };

            v2f vert(a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                o.uv = v.texcoord;

                return o;
            }

            fixed4 frag(v2f i):SV_TARGET0
            {
                float2 a = frac(sin(dot(i.uv, float2(1, 100)) * _Time.y) * 100000 * i.uv);
                return fixed4(a, a);
            }
            ENDCG
        }
    }
}