Shader "Custom/CuteToonShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Ramp ("Ramp Texture", 2D) = "white" {} // Текстура градиента для toon shading
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Toon

        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Ramp;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;

        // Custom lighting function
        half4 LightingToon (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half NdotL = dot (s.Normal, lightDir);
            half diff = NdotL * 0.5 + 0.5; // Remap to 0-1 range
            half3 ramp = tex2D(_Ramp, float2(diff, 0.5)).rgb; // Sample the ramp texture
            half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp * atten;

            c.a = s.Alpha;
            return c;
        }


        void surf (Input IN, inout SurfaceOutput s)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            s.Albedo = c.rgb;
            s.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}