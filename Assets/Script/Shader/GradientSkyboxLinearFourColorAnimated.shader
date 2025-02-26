Shader "GradientSkybox/Linear/FourColorAnimated" {
    Properties {
        _TopColor ("Top Color", Color) = (1, 0.3, 0.3, 1)
        _UpperMiddleColor ("Upper Middle Color", Color) = (1.0, 0.8, 0.5, 1)
        _LowerMiddleColor ("Lower Middle Color", Color) = (0.5, 0.8, 1.0, 1)
        _BottomColor ("Bottom Color", Color) = (0.3, 0.3, 1, 1)
        _Up ("Up", Vector) = (0, 1, 0)
        _Exp ("Exp", Range(0, 16)) = 1
        _AnimSpeed ("Animation Speed", Range(0, 2)) = 0.5
        _UpperMiddlePos ("Upper Middle Position", Range(0,1)) = 0.66
        _LowerMiddlePos ("Lower Middle Position", Range(0,1)) = 0.33
        _MinBrightness ("Minimum Brightness", Range(0, 1)) = 0.2  
        _MaxBrightness ("Maximum Brightness", Range(0, 1)) = 0.8  
    }
    SubShader {
        Tags {
            "RenderType" = "Background"
            "Queue" = "Background"
            "PreviewType" = "Skybox"
        }
        Pass {
            ZWrite Off
            Cull Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            fixed4 _TopColor, _UpperMiddleColor, _LowerMiddleColor, _BottomColor;
            float3 _Up;
            float _Exp;
            float _AnimSpeed;
            float _UpperMiddlePos;
            float _LowerMiddlePos;
            float _MinBrightness;  
            float _MaxBrightness;  


            struct appdata {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float3 texcoord : TEXCOORD0;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_TARGET {
                float3 texcoord = normalize(i.texcoord);
                float3 up = normalize(_Up);
                float d = dot(texcoord, up);
                float s = sign(d);
                float t = (d + 1) * 0.5;  

                
                float time = _Time.y * _AnimSpeed;

                
                float brightnessTop = _MinBrightness + (_MaxBrightness - _MinBrightness) * (0.5 + 0.5 * sin(time + 0.0));
                float brightnessUpperMid = _MinBrightness + (_MaxBrightness - _MinBrightness) * (0.5 + 0.5 * sin(time + 1.57));
                float brightnessLowerMid = _MinBrightness + (_MaxBrightness - _MinBrightness) * (0.5 + 0.5 * sin(time + 3.14));
                float brightnessBottom = _MinBrightness + (_MaxBrightness - _MinBrightness) * (0.5 + 0.5 * sin(time + 4.71));

                
                fixed4 animTop = lerp(_TopColor * _MinBrightness, _TopColor * _MaxBrightness, (0.5 + 0.5 * sin(time + 0.0)));
                fixed4 animUpperMid = lerp(_UpperMiddleColor* _MinBrightness, _UpperMiddleColor * _MaxBrightness, (0.5 + 0.5 * sin(time + 1.57)));
                fixed4 animLowerMid = lerp(_LowerMiddleColor* _MinBrightness, _LowerMiddleColor* _MaxBrightness, (0.5 + 0.5 * sin(time + 3.14)));
                fixed4 animBottom = lerp(_BottomColor * _MinBrightness, _BottomColor* _MaxBrightness, (0.5 + 0.5 * sin(time + 4.71)));



                float upperPos = clamp(_UpperMiddlePos, 0.001, 0.999);
                float lowerPos = clamp(_LowerMiddlePos, 0.001, 0.999);
                upperPos = max(upperPos, lowerPos + 0.001);

                fixed4 color;

                if (t > upperPos) {
                   color = lerp(animUpperMid, animTop, pow((t - upperPos) / (1.0 - upperPos) , _Exp));
                } else if (t > lowerPos) {
                    color = lerp(animLowerMid, animUpperMid, pow((t - lowerPos) / (upperPos - lowerPos) , _Exp));
                } else {
                   color = lerp(animBottom, animLowerMid,  pow(t / lowerPos , _Exp));
                }
                return color;
            }

            ENDCG
        }
    }
    CustomEditor "GradientSkybox.LinearFourColorAnimatedGradientSkyboxGUI"
}