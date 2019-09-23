Shader "Custom/minion-multi-purpose"
{
    Properties
    {
        _AddColor ("Add Color", Color) = (0,0,0,0)
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        //render double sides of a surface.
        //render back faces first
        Cull Front
        CGPROGRAM
        // Physically based Lambert lighting model, and enable shadows on all light types
        #pragma surface surf Lambert fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        
        
        // flip normal for back faces
        void vert (inout appdata_full v) {
            v.normal *= -1;
        }

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };
        
        fixed4 _AddColor;
        fixed4 _TintColor;

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            c.rgb += _AddColor.rgb * _AddColor.a;
            c *= _TintColor;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
        
        // render front faces
        Cull Back
        CGPROGRAM
        #pragma surface surf Lambert        
        
        sampler2D _MainTex;
        
        struct Input
        {
            float2 uv_MainTex;
        };
        
        fixed4 _AddColor;
        fixed4 _TintColor;
        
        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            c.rgb += _AddColor.rgb * _AddColor.a;
            c *= _TintColor;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        
        ENDCG        
    }
    FallBack "Diffuse"
}
