Shader "Scanner/HologramOverlay"
{
    Properties
    {
        _MainColor("Main Color", Color) = (0, 1, 1, 0.5)
        _ScanLineColor("Scan Line Color", Color) = (0, 1, 1, 1)
        _RimColor("Rim Color", Color) = (0, 1, 1, 1)
        _RimPower("Rim Power", Range(0.1, 10)) = 3.0
        _ScanLineWidth("Scan Line Width", Range(0, 1)) = 0.05
        _ScanLineSpeed("Scan Line Speed", Range(0, 5)) = 1
        _ScanLinePosition("Scan Line Position", Range(0, 1)) = 0
        _GridSize("Grid Size", Range(1, 100)) = 10
        _GridLineWidth("Grid Line Width", Range(0, 0.1)) = 0.02
        _Alpha("Overall Alpha", Range(0, 1)) = 0.5
        _Fresnel("Fresnel Strength", Range(0, 5)) = 1
    }

        SubShader
    {
        Tags
        {
            "Queue" = "Transparent+1"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
                float3 worldPos : TEXCOORD4;
            };

            float4 _MainColor;
            float4 _ScanLineColor;
            float4 _RimColor;
            float _RimPower;
            float _ScanLineWidth;
            float _ScanLineSpeed;
            float _ScanLinePosition;
            float _GridSize;
            float _GridLineWidth;
            float _Alpha;
            float _Fresnel;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                o.screenPos = ComputeScreenPos(o.pos);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float grid(float2 uv, float size, float width)
            {
                float2 grid = frac(uv * size);
                float2 smoothGrid = smoothstep(0, width, grid) * smoothstep(0, width, 1 - grid);
                return 1 - (smoothGrid.x + smoothGrid.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Calculate rim lighting
                float rim = 1.0 - saturate(dot(i.viewDir, i.worldNormal));
                rim = pow(rim, _RimPower);

                // Calculate fresnel effect
                float fresnel = pow(1.0 - saturate(dot(i.worldNormal, i.viewDir)), _Fresnel);

                // Animated scan line
                float scanLine = step(1 - _ScanLineWidth, frac(i.worldPos.y * 0.5 + _Time.y * _ScanLineSpeed + _ScanLinePosition));

                // Create grid pattern
                float gridPattern = grid(i.uv, _GridSize, _GridLineWidth);

                // Combine effects
                float4 col = _MainColor;
                col.rgb += _RimColor.rgb * rim * _RimColor.a;
                col.rgb += _ScanLineColor.rgb * scanLine * _ScanLineColor.a;
                col.rgb += gridPattern * _MainColor.rgb * 0.5;

                // Add fresnel
                col.rgb += _MainColor.rgb * fresnel * 0.5;

                // Add some vertical scan lines
                float verticalScan = step(0.98, frac(i.uv.x * 50 + _Time.y));
                col.rgb += verticalScan * _MainColor.rgb * 0.2;

                // Add screen space noise
                float noise = frac(sin(dot(i.screenPos.xy * _ScreenParams.xy, float2(12.9898, 78.233))) * 43758.5453);
                col.rgb += noise * 0.1;

                // Set alpha
                col.a = _MainColor.a * _Alpha;
                col.a *= (gridPattern * 0.5 + 0.5);
                col.a = saturate(col.a + rim * 0.5 + scanLine * 0.5);

                return col;
            }
            ENDCG
        }
    }
}