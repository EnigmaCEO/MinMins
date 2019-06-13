// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.05 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.05;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:0,uamb:True,mssp:True,lmpd:False,lprd:False,rprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:2,bsrc:0,bdst:0,culm:2,dpts:2,wrdp:False,dith:2,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1457,x:33741,y:32634,varname:node_1457,prsc:2|normal-5334-RGB,emission-5761-OUT,custl-9525-OUT,alpha-2230-A,clip-6138-OUT,voffset-5205-OUT;n:type:ShaderForge.SFN_Tex2d,id:2230,x:31891,y:32819,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:node_2230,prsc:2,tex:975b322382a24d644b37616b357e5103,ntxv:0,isnm:False|UVIN-6898-OUT;n:type:ShaderForge.SFN_Color,id:5327,x:32132,y:32537,ptovrint:False,ptlb:MainColor,ptin:_MainColor,varname:node_5327,prsc:2,glob:False,c1:1,c2:0.3529412,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:316,x:32493,y:32832,varname:node_316,prsc:2|A-2230-RGB,B-8967-OUT,C-5327-RGB;n:type:ShaderForge.SFN_ValueProperty,id:8967,x:32493,y:32713,ptovrint:False,ptlb:Emission,ptin:_Emission,varname:node_8967,prsc:2,glob:False,v1:16;n:type:ShaderForge.SFN_Multiply,id:7443,x:32438,y:33018,varname:node_7443,prsc:2|A-2230-R,B-2495-OUT;n:type:ShaderForge.SFN_Slider,id:2495,x:31808,y:33332,ptovrint:False,ptlb:AlphaClipAnimation,ptin:_AlphaClipAnimation,varname:node_2495,prsc:2,min:10,cur:1,max:0;n:type:ShaderForge.SFN_Vector1,id:2212,x:32368,y:33333,varname:node_2212,prsc:2,v1:0.1;n:type:ShaderForge.SFN_Add,id:7287,x:32637,y:33025,varname:node_7287,prsc:2|A-7443-OUT,B-4367-OUT;n:type:ShaderForge.SFN_Multiply,id:4367,x:32506,y:33180,varname:node_4367,prsc:2|A-2495-OUT,B-2212-OUT;n:type:ShaderForge.SFN_Add,id:4922,x:31009,y:32868,varname:node_4922,prsc:2|A-5795-UVOUT,B-347-OUT;n:type:ShaderForge.SFN_TexCoord,id:5795,x:30781,y:32666,varname:node_5795,prsc:2,uv:0;n:type:ShaderForge.SFN_Multiply,id:347,x:30819,y:32941,varname:node_347,prsc:2|A-4332-OUT,B-6096-T;n:type:ShaderForge.SFN_Append,id:4332,x:30645,y:32847,varname:node_4332,prsc:2|A-1583-OUT,B-4507-OUT;n:type:ShaderForge.SFN_Time,id:6096,x:30589,y:33064,varname:node_6096,prsc:2;n:type:ShaderForge.SFN_SwitchProperty,id:3552,x:31189,y:32821,ptovrint:False,ptlb:Use_Panner,ptin:_Use_Panner,varname:node_3552,prsc:2,on:False|A-5795-UVOUT,B-4922-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1583,x:30460,y:32823,ptovrint:False,ptlb:PannerSpeed_X,ptin:_PannerSpeed_X,varname:node_1583,prsc:2,glob:False,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:4507,x:30437,y:32960,ptovrint:False,ptlb:PannerSpeed_Y,ptin:_PannerSpeed_Y,varname:node_4507,prsc:2,glob:False,v1:0;n:type:ShaderForge.SFN_Vector1,id:1314,x:32707,y:32949,varname:node_1314,prsc:2,v1:1;n:type:ShaderForge.SFN_SwitchProperty,id:6138,x:33088,y:33009,ptovrint:False,ptlb:Use_AlphaClip,ptin:_Use_AlphaClip,varname:node_6138,prsc:2,on:False|A-1314-OUT,B-7287-OUT;n:type:ShaderForge.SFN_VertexColor,id:3762,x:33027,y:32494,varname:node_3762,prsc:2;n:type:ShaderForge.SFN_Multiply,id:9525,x:33230,y:32614,varname:node_9525,prsc:2|A-316-OUT,B-3762-RGB;n:type:ShaderForge.SFN_SwitchProperty,id:5205,x:33274,y:33634,ptovrint:False,ptlb:Use_Displace,ptin:_Use_Displace,varname:node_5670,prsc:2,on:False|A-4848-OUT,B-198-OUT;n:type:ShaderForge.SFN_Vector1,id:4848,x:33078,y:33647,varname:node_4848,prsc:2,v1:0;n:type:ShaderForge.SFN_NormalVector,id:8039,x:33154,y:33879,prsc:2,pt:False;n:type:ShaderForge.SFN_Multiply,id:198,x:33366,y:33774,varname:node_198,prsc:2|A-2230-RGB,B-7013-OUT,C-8039-OUT;n:type:ShaderForge.SFN_ValueProperty,id:7013,x:33154,y:34060,ptovrint:False,ptlb:DisplaceStrength,ptin:_DisplaceStrength,varname:node_9298,prsc:2,glob:False,v1:0.3;n:type:ShaderForge.SFN_Fresnel,id:2790,x:32821,y:32136,varname:node_2790,prsc:2|EXP-3614-OUT;n:type:ShaderForge.SFN_ValueProperty,id:3614,x:32599,y:32233,ptovrint:False,ptlb:Fresnel_Range,ptin:_Fresnel_Range,varname:node_3614,prsc:2,glob:False,v1:5;n:type:ShaderForge.SFN_Color,id:7002,x:32803,y:32330,ptovrint:False,ptlb:FresnelColor,ptin:_FresnelColor,varname:node_7002,prsc:2,glob:False,c1:0.4411765,c2:0.6794984,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:5761,x:33364,y:32230,varname:node_5761,prsc:2|A-2790-OUT,B-7002-RGB,C-7155-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2338,x:32791,y:32504,ptovrint:False,ptlb:Fresnel_Emission,ptin:_Fresnel_Emission,varname:node_2338,prsc:2,glob:False,v1:1;n:type:ShaderForge.SFN_SwitchProperty,id:7155,x:33132,y:32363,ptovrint:False,ptlb:Use_Fresnel,ptin:_Use_Fresnel,varname:node_7155,prsc:2,on:True|A-1336-OUT,B-2338-OUT;n:type:ShaderForge.SFN_Vector1,id:1336,x:32771,y:32574,varname:node_1336,prsc:2,v1:0;n:type:ShaderForge.SFN_Tex2d,id:5334,x:31808,y:33079,ptovrint:False,ptlb:Normal,ptin:_Normal,varname:node_5334,prsc:2,ntxv:3,isnm:True|UVIN-3552-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:6898,x:31481,y:32731,ptovrint:False,ptlb:Use_UVanim,ptin:_Use_UVanim,varname:node_6898,prsc:2,on:False|A-3552-OUT,B-1192-OUT;n:type:ShaderForge.SFN_Add,id:1192,x:31167,y:33422,varname:node_1192,prsc:2|A-2868-OUT,B-6988-OUT;n:type:ShaderForge.SFN_Append,id:6988,x:30986,y:33491,varname:node_6988,prsc:2|A-3830-OUT,B-4307-OUT;n:type:ShaderForge.SFN_OneMinus,id:4307,x:30801,y:33597,varname:node_4307,prsc:2|IN-667-OUT;n:type:ShaderForge.SFN_Divide,id:667,x:30617,y:33597,varname:node_667,prsc:2|A-1062-OUT,B-7229-OUT;n:type:ShaderForge.SFN_Floor,id:1062,x:30409,y:33597,varname:node_1062,prsc:2|IN-6290-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1054,x:29523,y:33345,ptovrint:False,ptlb:UV_Rows,ptin:_UV_Rows,varname:node_5095,prsc:2,glob:False,v1:4;n:type:ShaderForge.SFN_ValueProperty,id:7611,x:29512,y:33512,ptovrint:False,ptlb:UV_Columns,ptin:_UV_Columns,varname:node_3932,prsc:2,glob:False,v1:4;n:type:ShaderForge.SFN_Divide,id:6290,x:30230,y:33605,varname:node_6290,prsc:2|A-5475-OUT,B-5832-OUT;n:type:ShaderForge.SFN_Round,id:5475,x:30047,y:33558,varname:node_5475,prsc:2|IN-3644-OUT;n:type:ShaderForge.SFN_Multiply,id:3644,x:29889,y:33517,varname:node_3644,prsc:2|A-5481-OUT,B-2503-OUT;n:type:ShaderForge.SFN_Relay,id:5832,x:30117,y:33132,varname:node_5832,prsc:2|IN-1054-OUT;n:type:ShaderForge.SFN_Relay,id:7229,x:30105,y:33310,varname:node_7229,prsc:2|IN-7611-OUT;n:type:ShaderForge.SFN_Time,id:7610,x:29427,y:33746,varname:node_7610,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1080,x:29617,y:33692,varname:node_1080,prsc:2|A-5649-OUT,B-7610-T;n:type:ShaderForge.SFN_ValueProperty,id:5649,x:29323,y:33695,ptovrint:False,ptlb:UV_AnimSpeed,ptin:_UV_AnimSpeed,varname:node_5671,prsc:2,glob:False,v1:1;n:type:ShaderForge.SFN_Frac,id:2503,x:29775,y:33715,varname:node_2503,prsc:2|IN-1080-OUT;n:type:ShaderForge.SFN_Multiply,id:5481,x:29702,y:33388,varname:node_5481,prsc:2|A-1054-OUT,B-7611-OUT;n:type:ShaderForge.SFN_Divide,id:2868,x:30986,y:33343,varname:node_2868,prsc:2|A-8598-UVOUT,B-3502-OUT;n:type:ShaderForge.SFN_TexCoord,id:8598,x:30986,y:33189,varname:node_8598,prsc:2,uv:0;n:type:ShaderForge.SFN_Append,id:3502,x:30782,y:33256,varname:node_3502,prsc:2|A-5832-OUT,B-7229-OUT;n:type:ShaderForge.SFN_Divide,id:3830,x:30782,y:33452,varname:node_3830,prsc:2|A-4966-OUT,B-5832-OUT;n:type:ShaderForge.SFN_Fmod,id:4966,x:30585,y:33446,varname:node_4966,prsc:2|A-5475-OUT,B-5832-OUT;proporder:5327-2230-5334-8967-6898-1054-7611-5649-3552-1583-4507-6138-2495-5205-7013-7155-7002-2338-3614;pass:END;sub:END;*/

Shader "FT/FT_Additive" {
    Properties {
        _MainColor ("MainColor", Color) = (1,0.3529412,0,1)
        _Texture ("Texture", 2D) = "white" {}
        _Normal ("Normal", 2D) = "bump" {}
        _Emission ("Emission", Float ) = 16
        [MaterialToggle] _Use_UVanim ("Use_UVanim", Float ) = 0
        _UV_Rows ("UV_Rows", Float ) = 4
        _UV_Columns ("UV_Columns", Float ) = 4
        _UV_AnimSpeed ("UV_AnimSpeed", Float ) = 1
        [MaterialToggle] _Use_Panner ("Use_Panner", Float ) = 0
        _PannerSpeed_X ("PannerSpeed_X", Float ) = 0
        _PannerSpeed_Y ("PannerSpeed_Y", Float ) = 0
        [MaterialToggle] _Use_AlphaClip ("Use_AlphaClip", Float ) = 1
        _AlphaClipAnimation ("AlphaClipAnimation", Range(10, 0)) = 1
        [MaterialToggle] _Use_Displace ("Use_Displace", Float ) = 0
        _DisplaceStrength ("DisplaceStrength", Float ) = 0.3
        [MaterialToggle] _Use_Fresnel ("Use_Fresnel", Float ) = 1
        _FresnelColor ("FresnelColor", Color) = (0.4411765,0.6794984,1,1)
        _Fresnel_Emission ("Fresnel_Emission", Float ) = 1
        _Fresnel_Range ("Fresnel_Range", Float ) = 5
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "ForwardBase"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend One One
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            #pragma glsl
            // Dithering function, to use with scene UVs (screen pixel coords)
            // 3x3 Bayer matrix, based on https://en.wikipedia.org/wiki/Ordered_dithering
            float BinaryDither3x3( float value, float2 sceneUVs ) {
                float3x3 mtx = float3x3(
                    float3( 3,  7,  4 )/10.0,
                    float3( 6,  1,  9 )/10.0,
                    float3( 2,  8,  5 )/10.0
                );
                float2 px = floor(_ScreenParams.xy * sceneUVs);
                int xSmp = fmod(px.x,3);
                int ySmp = fmod(px.y,3);
                float3 xVec = 1-saturate(abs(float3(0,1,2) - xSmp));
                float3 yVec = 1-saturate(abs(float3(0,1,2) - ySmp));
                float3 pxMult = float3( dot(mtx[0],yVec), dot(mtx[1],yVec), dot(mtx[2],yVec) );
                return round(value + dot(pxMult, xVec));
            }
            uniform float4 _TimeEditor;
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform float4 _MainColor;
            uniform float _Emission;
            uniform float _AlphaClipAnimation;
            uniform fixed _Use_Panner;
            uniform float _PannerSpeed_X;
            uniform float _PannerSpeed_Y;
            uniform fixed _Use_AlphaClip;
            uniform fixed _Use_Displace;
            uniform float _DisplaceStrength;
            uniform float _Fresnel_Range;
            uniform float4 _FresnelColor;
            uniform float _Fresnel_Emission;
            uniform fixed _Use_Fresnel;
            uniform sampler2D _Normal; uniform float4 _Normal_ST;
            uniform fixed _Use_UVanim;
            uniform float _UV_Rows;
            uniform float _UV_Columns;
            uniform float _UV_AnimSpeed;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 binormalDir : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
                float4 vertexColor : COLOR;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.normalDir = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.binormalDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float4 node_6096 = _Time + _TimeEditor;
                float2 _Use_Panner_var = lerp( o.uv0, (o.uv0+(float2(_PannerSpeed_X,_PannerSpeed_Y)*node_6096.g)), _Use_Panner );
                float node_5832 = _UV_Rows;
                float node_7229 = _UV_Columns;
                float4 node_7610 = _Time + _TimeEditor;
                float node_5475 = round(((_UV_Rows*_UV_Columns)*frac((_UV_AnimSpeed*node_7610.g))));
                float2 _Use_UVanim_var = lerp( _Use_Panner_var, ((o.uv0/float2(node_5832,node_7229))+float2((fmod(node_5475,node_5832)/node_5832),(1.0 - (floor((node_5475/node_5832))/node_7229)))), _Use_UVanim );
                float4 _Texture_var = tex2Dlod(_Texture,float4(TRANSFORM_TEX(_Use_UVanim_var, _Texture),0.0,0));
                v.vertex.xyz += lerp( 0.0, (_Texture_var.rgb*_DisplaceStrength*v.normal), _Use_Displace );
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = o.pos;
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5;
                float3x3 tangentTransform = float3x3( i.tangentDir, i.binormalDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float4 node_6096 = _Time + _TimeEditor;
                float2 _Use_Panner_var = lerp( i.uv0, (i.uv0+(float2(_PannerSpeed_X,_PannerSpeed_Y)*node_6096.g)), _Use_Panner );
                float3 _Normal_var = UnpackNormal(tex2D(_Normal,TRANSFORM_TEX(_Use_Panner_var, _Normal)));
                float3 normalLocal = _Normal_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                
                float nSign = sign( dot( viewDirection, i.normalDir ) ); // Reverse normal if this is a backface
                i.normalDir *= nSign;
                normalDirection *= nSign;
                
                float node_5832 = _UV_Rows;
                float node_7229 = _UV_Columns;
                float4 node_7610 = _Time + _TimeEditor;
                float node_5475 = round(((_UV_Rows*_UV_Columns)*frac((_UV_AnimSpeed*node_7610.g))));
                float2 _Use_UVanim_var = lerp( _Use_Panner_var, ((i.uv0/float2(node_5832,node_7229))+float2((fmod(node_5475,node_5832)/node_5832),(1.0 - (floor((node_5475/node_5832))/node_7229)))), _Use_UVanim );
                float4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(_Use_UVanim_var, _Texture));
                clip( BinaryDither3x3(lerp( 1.0, ((_Texture_var.r*_AlphaClipAnimation)+(_AlphaClipAnimation*0.1)), _Use_AlphaClip ) - 1.5, sceneUVs) );
////// Lighting:
////// Emissive:
                float3 emissive = (pow(1.0-max(0,dot(normalDirection, viewDirection)),_Fresnel_Range)*_FresnelColor.rgb*lerp( 0.0, _Fresnel_Emission, _Use_Fresnel ));
                float3 finalColor = emissive + ((_Texture_var.rgb*_Emission*_MainColor.rgb)*i.vertexColor.rgb);
                return fixed4(finalColor,_Texture_var.a);
            }
            ENDCG
        }
        Pass {
            Name "ShadowCollector"
            Tags {
                "LightMode"="ShadowCollector"
            }
            Cull Off
            
            Fog {Mode Off}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCOLLECTOR
            #define SHADOW_COLLECTOR_PASS
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcollector
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            #pragma glsl
            // Dithering function, to use with scene UVs (screen pixel coords)
            // 3x3 Bayer matrix, based on https://en.wikipedia.org/wiki/Ordered_dithering
            float BinaryDither3x3( float value, float2 sceneUVs ) {
                float3x3 mtx = float3x3(
                    float3( 3,  7,  4 )/10.0,
                    float3( 6,  1,  9 )/10.0,
                    float3( 2,  8,  5 )/10.0
                );
                float2 px = floor(_ScreenParams.xy * sceneUVs);
                int xSmp = fmod(px.x,3);
                int ySmp = fmod(px.y,3);
                float3 xVec = 1-saturate(abs(float3(0,1,2) - xSmp));
                float3 yVec = 1-saturate(abs(float3(0,1,2) - ySmp));
                float3 pxMult = float3( dot(mtx[0],yVec), dot(mtx[1],yVec), dot(mtx[2],yVec) );
                return round(value + dot(pxMult, xVec));
            }
            uniform float4 _TimeEditor;
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform float _AlphaClipAnimation;
            uniform fixed _Use_Panner;
            uniform float _PannerSpeed_X;
            uniform float _PannerSpeed_Y;
            uniform fixed _Use_AlphaClip;
            uniform fixed _Use_Displace;
            uniform float _DisplaceStrength;
            uniform fixed _Use_UVanim;
            uniform float _UV_Rows;
            uniform float _UV_Columns;
            uniform float _UV_AnimSpeed;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_COLLECTOR;
                float2 uv0 : TEXCOORD5;
                float3 normalDir : TEXCOORD6;
                float4 screenPos : TEXCOORD7;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                float4 node_6096 = _Time + _TimeEditor;
                float2 _Use_Panner_var = lerp( o.uv0, (o.uv0+(float2(_PannerSpeed_X,_PannerSpeed_Y)*node_6096.g)), _Use_Panner );
                float node_5832 = _UV_Rows;
                float node_7229 = _UV_Columns;
                float4 node_7610 = _Time + _TimeEditor;
                float node_5475 = round(((_UV_Rows*_UV_Columns)*frac((_UV_AnimSpeed*node_7610.g))));
                float2 _Use_UVanim_var = lerp( _Use_Panner_var, ((o.uv0/float2(node_5832,node_7229))+float2((fmod(node_5475,node_5832)/node_5832),(1.0 - (floor((node_5475/node_5832))/node_7229)))), _Use_UVanim );
                float4 _Texture_var = tex2Dlod(_Texture,float4(TRANSFORM_TEX(_Use_UVanim_var, _Texture),0.0,0));
                v.vertex.xyz += lerp( 0.0, (_Texture_var.rgb*_DisplaceStrength*v.normal), _Use_Displace );
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = o.pos;
                TRANSFER_SHADOW_COLLECTOR(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5;
/////// Vectors:
                float4 node_6096 = _Time + _TimeEditor;
                float2 _Use_Panner_var = lerp( i.uv0, (i.uv0+(float2(_PannerSpeed_X,_PannerSpeed_Y)*node_6096.g)), _Use_Panner );
                float node_5832 = _UV_Rows;
                float node_7229 = _UV_Columns;
                float4 node_7610 = _Time + _TimeEditor;
                float node_5475 = round(((_UV_Rows*_UV_Columns)*frac((_UV_AnimSpeed*node_7610.g))));
                float2 _Use_UVanim_var = lerp( _Use_Panner_var, ((i.uv0/float2(node_5832,node_7229))+float2((fmod(node_5475,node_5832)/node_5832),(1.0 - (floor((node_5475/node_5832))/node_7229)))), _Use_UVanim );
                float4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(_Use_UVanim_var, _Texture));
                clip( BinaryDither3x3(lerp( 1.0, ((_Texture_var.r*_AlphaClipAnimation)+(_AlphaClipAnimation*0.1)), _Use_AlphaClip ) - 1.5, sceneUVs) );
                SHADOW_COLLECTOR_FRAGMENT(i)
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Cull Off
            Offset 1, 1
            
            Fog {Mode Off}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma exclude_renderers xbox360 ps3 flash d3d11_9x 
            #pragma target 3.0
            #pragma glsl
            // Dithering function, to use with scene UVs (screen pixel coords)
            // 3x3 Bayer matrix, based on https://en.wikipedia.org/wiki/Ordered_dithering
            float BinaryDither3x3( float value, float2 sceneUVs ) {
                float3x3 mtx = float3x3(
                    float3( 3,  7,  4 )/10.0,
                    float3( 6,  1,  9 )/10.0,
                    float3( 2,  8,  5 )/10.0
                );
                float2 px = floor(_ScreenParams.xy * sceneUVs);
                int xSmp = fmod(px.x,3);
                int ySmp = fmod(px.y,3);
                float3 xVec = 1-saturate(abs(float3(0,1,2) - xSmp));
                float3 yVec = 1-saturate(abs(float3(0,1,2) - ySmp));
                float3 pxMult = float3( dot(mtx[0],yVec), dot(mtx[1],yVec), dot(mtx[2],yVec) );
                return round(value + dot(pxMult, xVec));
            }
            uniform float4 _TimeEditor;
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform float _AlphaClipAnimation;
            uniform fixed _Use_Panner;
            uniform float _PannerSpeed_X;
            uniform float _PannerSpeed_Y;
            uniform fixed _Use_AlphaClip;
            uniform fixed _Use_Displace;
            uniform float _DisplaceStrength;
            uniform fixed _Use_UVanim;
            uniform float _UV_Rows;
            uniform float _UV_Columns;
            uniform float _UV_AnimSpeed;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = mul(unity_ObjectToWorld, float4(v.normal,0)).xyz;
                float4 node_6096 = _Time + _TimeEditor;
                float2 _Use_Panner_var = lerp( o.uv0, (o.uv0+(float2(_PannerSpeed_X,_PannerSpeed_Y)*node_6096.g)), _Use_Panner );
                float node_5832 = _UV_Rows;
                float node_7229 = _UV_Columns;
                float4 node_7610 = _Time + _TimeEditor;
                float node_5475 = round(((_UV_Rows*_UV_Columns)*frac((_UV_AnimSpeed*node_7610.g))));
                float2 _Use_UVanim_var = lerp( _Use_Panner_var, ((o.uv0/float2(node_5832,node_7229))+float2((fmod(node_5475,node_5832)/node_5832),(1.0 - (floor((node_5475/node_5832))/node_7229)))), _Use_UVanim );
                float4 _Texture_var = tex2Dlod(_Texture,float4(TRANSFORM_TEX(_Use_UVanim_var, _Texture),0.0,0));
                v.vertex.xyz += lerp( 0.0, (_Texture_var.rgb*_DisplaceStrength*v.normal), _Use_Displace );
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = o.pos;
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.normalDir = normalize(i.normalDir);
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5;
/////// Vectors:
                float4 node_6096 = _Time + _TimeEditor;
                float2 _Use_Panner_var = lerp( i.uv0, (i.uv0+(float2(_PannerSpeed_X,_PannerSpeed_Y)*node_6096.g)), _Use_Panner );
                float node_5832 = _UV_Rows;
                float node_7229 = _UV_Columns;
                float4 node_7610 = _Time + _TimeEditor;
                float node_5475 = round(((_UV_Rows*_UV_Columns)*frac((_UV_AnimSpeed*node_7610.g))));
                float2 _Use_UVanim_var = lerp( _Use_Panner_var, ((i.uv0/float2(node_5832,node_7229))+float2((fmod(node_5475,node_5832)/node_5832),(1.0 - (floor((node_5475/node_5832))/node_7229)))), _Use_UVanim );
                float4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(_Use_UVanim_var, _Texture));
                clip( BinaryDither3x3(lerp( 1.0, ((_Texture_var.r*_AlphaClipAnimation)+(_AlphaClipAnimation*0.1)), _Use_AlphaClip ) - 1.5, sceneUVs) );
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
