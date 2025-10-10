Shader "Unlit/OccludedOutline"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0.2, 0.9, 1, 1)
        _OutlineWidth("Outline Width (world)", Float) = 0.02
    }
    SubShader
    {
        Tags
        { 
            "Queue"="Geometry+1" "RenderType"="Opaque" 
        }
        ZTest Greater
        ZWrite Off
        Cull Front
        Blend SrcAlpha OneMinusSrcAlpha
        Stencil
        {
            Ref 1
            Comp NotEqual
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OutlineColor;
            float  _OutlineWidth;

            struct appdata 
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct v2f 
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float3 nWS = UnityObjectToWorldNormal(v.normal);
                float3 pWS = mul(unity_ObjectToWorld, v.vertex).xyz + nWS * _OutlineWidth;
                o.pos = UnityWorldToClipPos(float4(pWS,1));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}
