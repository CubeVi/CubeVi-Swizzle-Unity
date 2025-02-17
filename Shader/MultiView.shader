Shader "CustomRenderTexture/MultiView"
{  
    Properties  
    {  
        _MainTex ("Texture", 2D) = "white" {}  
        _Slope ("Slope", Float) = -0.1004 
        _Interval ("Interval", Float) = 19.6122 
        _X0 ("X0 Offset", Float) = 15.4  
        _ImgsCountX ("Images Count X", Float) = 8.0  
        _ImgsCountY ("Images Count Y", Float) = 5.0  
        _ImgsCountAll ("Total Images", Float) = 40.0  
        _Gamma ("Gamma Correction", Float) = 1.8  
        _OutputSizeX ("Output Size X", Float) = 1440.0  
        _OutputSizeY ("Output Size Y", Float) = 2560.0 
    }  
    SubShader  
    {  
        Tags { "RenderType"="Opaque" }  
        LOD 100  
  
        Pass  
        {   
            Name "CustomRenderTexture/MultiView"

            CGPROGRAM  
            #pragma vertex vert  
            #pragma fragment frag  
  
            #include "UnityCG.cginc"  
            #include "UnityCustomRenderTexture.cginc"
  
            struct appdata_t
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
            float _OutputSizeX;
            float _OutputSizeY;
            float _Slope;
            float _X0;
            float _Interval;
            float _ImgsCountAll;
            float _ImgsCountX;
            float _ImgsCountY; 

            float get_choice_float(float2 pos, float bias)
            {
                float x = (pos.x) * _OutputSizeX + 0.5;
                float y = (1.0 - pos.y) * _OutputSizeY + 0.5;
                float x1 = (x + y * _Slope) * 3.0 + bias; 
                float x_local = fmod(x1 + _X0, _Interval);
                return (x_local / _Interval);
            }

            float2 get_uv_from_choice(float2 pos, float choice_float)
            {
                float choice = floor(choice_float * _ImgsCountAll);
                float2 choice_vec = float2(
                    _ImgsCountX - fmod(float(choice), _ImgsCountX) -1,
                    floor(float(choice) / _ImgsCountX)
                );

                float2 reciprocals = float2(1.0 / _ImgsCountX, 1.0 / _ImgsCountY);
                return (choice_vec.xy + pos) * reciprocals;
            }

            float4 get_color(float2 i, float bias) {
                float choice_float = get_choice_float(i, bias);

                float2 sel_pos = get_uv_from_choice(i, choice_float);
                float color = choice_float;
                float4 res = float4(sel_pos,sel_pos);
                res = tex2D(_MainTex, sel_pos);
                return res;
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float4 color = get_color(i.uv, 0.0);
                color.g = get_color(i.uv, 1.0).g;
                color.b = get_color(i.uv, 2.0).b; 

                return color;
            }
            ENDCG  
        }  
    }  
}