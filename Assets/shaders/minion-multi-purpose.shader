Shader "Custom/minion-multi-purpose"
{
    Properties
    {
        _AddColor ("Add Color", Color) = (0,0,0,0)
        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _DissolveTexture("Dissolve Texture", 2D) = "white" {}
        _DissolveAmount("Dissovle Amount", Range(0,1)) = 0
        _DissolveBorderColor ("Disolve Border Color", Color) = (1,1,1,1)
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
        
        
        // flip normal for back faces, notice that in each cycle (cg program -> end cg)
        // the input is gonna be "reset" and provided again.
        // so even though i flipped the normal of each vertex here, in the next cycle,
        // the normal will be provided with their original value again. 
        void vert (inout appdata_full v) {
            v.normal *= -1;
        }

        sampler2D _MainTex;
        sampler2D _DissolveTexture;

        struct Input
        {
            float2 uv_MainTex;
        };
        
        fixed4 _AddColor;
        fixed4 _TintColor;
        float _DissolveAmount;
        fixed4 _DissolveBorderColor;

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            c.rgb += _AddColor.rgb * _AddColor.a;
            c *= _TintColor;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            
            half dissolve_value = tex2D(_DissolveTexture, IN.uv_MainTex).r; //Get how much we have to dissolve based on our dissolve texture
            o.Emission = _DissolveBorderColor * step( dissolve_value - _DissolveAmount, 0.02f);
            clip(dissolve_value - _DissolveAmount);
        }
        ENDCG
        
        // render front faces
        Cull Back
        CGPROGRAM
        #pragma surface surf Lambert        
        
        sampler2D _MainTex;
        sampler2D _DissolveTexture;
        
        struct Input
        {
            float2 uv_MainTex;
        };
        
        fixed4 _AddColor;
        fixed4 _TintColor;
        float _DissolveAmount;
        fixed4 _DissolveBorderColor;
        
        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            c.rgb += _AddColor.rgb * _AddColor.a;
            c *= _TintColor;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            
            half dissolve_value = tex2D(_DissolveTexture, IN.uv_MainTex).r; //Get how much we have to dissolve based on our dissolve texture
            o.Emission = _DissolveBorderColor * step( dissolve_value - _DissolveAmount, 0.02f);
            clip(dissolve_value - _DissolveAmount);
        }
        
        ENDCG        
    }
    FallBack "Diffuse"
}
