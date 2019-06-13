// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.05 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.05;sub:START;pass:START;ps:flbk:,lico:1,lgpr:1,nrmq:1,limd:0,uamb:True,mssp:True,lmpd:False,lprd:False,rprd:False,enco:False,frtr:True,vitr:True,dbil:False,rmgx:True,rpth:0,hqsc:True,hqlp:False,tesm:0,blpr:2,bsrc:0,bdst:0,culm:2,dpts:2,wrdp:False,dith:2,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:1457,x:33288,y:32719,varname:node_1457,prsc:2|emission-9525-OUT;n:type:ShaderForge.SFN_Tex2d,id:2230,x:31727,y:32709,ptovrint:False,ptlb:Texture,ptin:_Texture,varname:node_2230,prsc:2,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:5327,x:31909,y:33055,ptovrint:False,ptlb:Color_R,ptin:_Color_R,varname:node_5327,prsc:2,glob:False,c1:1,c2:0.3529412,c3:0,c4:1;n:type:ShaderForge.SFN_ValueProperty,id:8967,x:32605,y:32665,ptovrint:False,ptlb:Emission,ptin:_Emission,varname:node_8967,prsc:2,glob:False,v1:16;n:type:ShaderForge.SFN_VertexColor,id:3762,x:32672,y:33057,varname:node_3762,prsc:2;n:type:ShaderForge.SFN_Multiply,id:9525,x:33123,y:32805,varname:node_9525,prsc:2|A-8511-OUT,B-3762-RGB,C-3762-A,D-8967-OUT,E-698-OUT;n:type:ShaderForge.SFN_Multiply,id:4821,x:32385,y:32813,varname:node_4821,prsc:2|A-3144-OUT,B-5327-RGB;n:type:ShaderForge.SFN_Multiply,id:6442,x:32385,y:32980,varname:node_6442,prsc:2|A-2319-OUT,B-5203-RGB;n:type:ShaderForge.SFN_Color,id:5203,x:31913,y:33229,ptovrint:False,ptlb:Color_G,ptin:_Color_G,varname:node_5203,prsc:2,glob:False,c1:0.1623053,c2:1,c3:0.06617647,c4:1;n:type:ShaderForge.SFN_Color,id:4165,x:31913,y:33425,ptovrint:False,ptlb:Color_B,ptin:_Color_B,varname:node_4165,prsc:2,glob:False,c1:0.1017182,c2:0.04880831,c3:0.9482759,c4:1;n:type:ShaderForge.SFN_Multiply,id:7646,x:32385,y:33166,varname:node_7646,prsc:2|A-8730-OUT,B-4165-RGB;n:type:ShaderForge.SFN_Add,id:8511,x:32672,y:32865,varname:node_8511,prsc:2|A-4821-OUT,B-6442-OUT,C-7646-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:2319,x:32092,y:32646,ptovrint:False,ptlb:Use_Green,ptin:_Use_Green,varname:node_2319,prsc:2,on:True|A-4585-OUT,B-2230-G;n:type:ShaderForge.SFN_Vector1,id:4585,x:31813,y:32435,varname:node_4585,prsc:2,v1:0;n:type:ShaderForge.SFN_SwitchProperty,id:8730,x:32092,y:32796,ptovrint:False,ptlb:Use_Blue,ptin:_Use_Blue,varname:node_8730,prsc:2,on:True|A-4585-OUT,B-2230-B;n:type:ShaderForge.SFN_SwitchProperty,id:3144,x:32092,y:32495,ptovrint:False,ptlb:Use_Red,ptin:_Use_Red,varname:node_3144,prsc:2,on:True|A-4585-OUT,B-2230-R;n:type:ShaderForge.SFN_Slider,id:7521,x:32381,y:32539,ptovrint:False,ptlb:SoftParticle_Value,ptin:_SoftParticle_Value,varname:node_5073,prsc:2,min:0,cur:0,max:1;n:type:ShaderForge.SFN_SwitchProperty,id:698,x:32938,y:32484,ptovrint:False,ptlb:Use_SoftPaticle,ptin:_Use_SoftPaticle,varname:node_287,prsc:2,on:False|A-2836-OUT,B-6308-OUT;n:type:ShaderForge.SFN_DepthBlend,id:6308,x:32731,y:32549,varname:node_6308,prsc:2|DIST-7521-OUT;n:type:ShaderForge.SFN_Vector1,id:2836,x:32731,y:32484,varname:node_2836,prsc:2,v1:1;proporder:2230-8967-3144-2319-8730-5327-5203-4165-698-7521;pass:END;sub:END;*/

Shader "FT/EmissionChannel_DoubleSide" {
    Properties {
        _Texture ("Texture", 2D) = "white" {}
        _Emission ("Emission", Float ) = 16
        [MaterialToggle] _Use_Red ("Use_Red", Float ) = 1
        [MaterialToggle] _Use_Green ("Use_Green", Float ) = 1
        [MaterialToggle] _Use_Blue ("Use_Blue", Float ) = 1
        _Color_R ("Color_R", Color) = (1,0.3529412,0,1)
        _Color_G ("Color_G", Color) = (0.1623053,1,0.06617647,1)
        _Color_B ("Color_B", Color) = (0.1017182,0.04880831,0.9482759,1)
        [MaterialToggle] _Use_SoftPaticle ("Use_SoftPaticle", Float ) = 1
        _SoftParticle_Value ("SoftParticle_Value", Range(0, 1)) = 0
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
            uniform sampler2D _CameraDepthTexture;
            uniform sampler2D _Texture; uniform float4 _Texture_ST;
            uniform float4 _Color_R;
            uniform float _Emission;
            uniform float4 _Color_G;
            uniform float4 _Color_B;
            uniform fixed _Use_Green;
            uniform fixed _Use_Blue;
            uniform fixed _Use_Red;
            uniform float _SoftParticle_Value;
            uniform fixed _Use_SoftPaticle;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 vertexColor : COLOR;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 vertexColor : COLOR;
                float4 projPos : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                return o;
            }
            fixed4 frag(VertexOutput i) : COLOR {
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
/////// Vectors:
////// Lighting:
////// Emissive:
                float node_4585 = 0.0;
                float4 _Texture_var = tex2D(_Texture,TRANSFORM_TEX(i.uv0, _Texture));
                float3 emissive = (((lerp( node_4585, _Texture_var.r, _Use_Red )*_Color_R.rgb)+(lerp( node_4585, _Texture_var.g, _Use_Green )*_Color_G.rgb)+(lerp( node_4585, _Texture_var.b, _Use_Blue )*_Color_B.rgb))*i.vertexColor.rgb*i.vertexColor.a*_Emission*lerp( 1.0, saturate((sceneZ-partZ)/_SoftParticle_Value), _Use_SoftPaticle ));
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
