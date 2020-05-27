Shader "Custom/SurfacePlotFlat" {
     Properties{
        _VisibleMin("VisibleMin", Vector) = (0,0,0,0)
        _VisibleMax("VisibleMax", Vector) = (1,1,1,0)
        _Min("Min", Vector) = (0,0,0,0)
        _Max("Max", Vector) = (1,1,1,0)
        _StartColor("StartColor", Color) = (0,0,0,1)
        _EndColor("EndColor", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
     }
 
    SubShader{
        Cull off   
        Tags { "RenderType" = "Opaque" }
 
        CGPROGRAM
        //Notice the "vertex:vert" at the end of the next line
        #pragma surface surf Standard fullforwardshadows vertex:vert
 
        sampler2D _MainTex;
 
        struct Input {
            float2 uv_MainTex;
            float3 coord;
        };
 
        fixed4 _StartColor;
        fixed4 _EndColor;
        float4 _Min;
        float4 _Max;
        float4 _VisibleMin;
        float4 _VisibleMax;
 
        void vert(inout appdata_base v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.coord.x = v.vertex.x;
            o.coord.y = v.vertex.y;
            o.coord.z = v.vertex.z;
        }
 
        void surf(Input IN, inout SurfaceOutputStandard o) {
            fixed4 s = _StartColor;
            fixed4 e = _EndColor;
            //float scaledY = (IN.coord.y - _Min.y) / (_Max.y - _Min.y); //Map coord onto 0-1 range for lerp
            fixed4 c = s;//tex2D(_MainTex, IN.uv_MainTex);// * lerp(s, e, scaledY);
            o.Albedo = (0,0,0);
            o.Metallic = 0;
            o.Smoothness = 0;
            o.Emission = c.rgb;

            o.Alpha = c.a;

            
            clip(_VisibleMax.x - IN.coord.x);
            clip(_VisibleMax.y - IN.coord.y);
            clip(_VisibleMax.z - IN.coord.z);

            clip(IN.coord.x - _VisibleMin.x);
            clip(IN.coord.y - _VisibleMin.y);
            clip(IN.coord.z - _VisibleMin.z);
        }
        ENDCG
    }
    FallBack "Diffuse"
}