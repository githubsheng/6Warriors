Shader "Custom/outline-vertex-extrude"
{
    Properties {
      _MainTex ("Texture", 2D) = "white" {}
      _Amount ("Extrusion Amount", Range(-1,1)) = 0.05
      _OutlineColor ("Outline color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry+1"}
        LOD 150
 
         Pass {
            ZWrite Off
            Lighting Off
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            

            struct v2f {
                float4 pos : SV_POSITION;
            };
            
            float _Amount;
            
            v2f vert (appdata_base v) {
                v2f o;
                v.vertex.xyz += v.normal * _Amount;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 _OutlineColor;
            
            fixed4 frag (v2f i) : SV_Target { 
                return _OutlineColor; 
            }
            
            ENDCG
        }   
              
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
        
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };        
        
            sampler2D _MainTex;
        
            v2f vert (appdata_base v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy;
                return o;
            }
            
            fixed4 frag (v2f i): SV_Target {
                fixed4 c = tex2D(_MainTex, i.uv);
                return c;
            }

            
            ENDCG
        }
        
        Pass {
            ZWrite On
            Lighting Off
            ColorMask 0
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            

            struct v2f {
                float4 pos : SV_POSITION;
            };
            
            float _Amount;
            
            v2f vert (appdata_base v) {
                v2f o;
                v.vertex.xyz += v.normal * _Amount;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
                        
            fixed4 frag (v2f i) : SV_Target { 
                return (1,1,1,1); 
            }
            
            ENDCG
        }
               
    } 
    Fallback "Diffuse"
}
