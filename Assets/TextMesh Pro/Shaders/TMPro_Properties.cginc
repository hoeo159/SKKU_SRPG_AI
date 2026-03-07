// UI Editable properties
uniform sampler2D	_FaceTex;					// Alpha : Signed Distance
uniform float		_FaceUVSpeedX;
uniform float		_FaceUVSpeedY;
uniform fixed4		_FaceColor;					// RGBA : Color + Opacity
uniform float		_FaceDilate;				// value[ 0, 1]
uniform float		_OutlineSoftness;			// value[ 0, 1]

uniform sampler2D	_OutlineTex;				// RGBA : Color + Opacity
uniform float		_OutlineUVSpeedX;
uniform float		_OutlineUVSpeedY;
uniform fixed4		_OutlineColor;				// RGBA : Color + Opacity
uniform float		_OutlineWidth;				// value[ 0, 1]

uniform float		_Bevel;						// value[ 0, 1]
uniform float		_BevelOffset;				// value[-1, 1]
uniform float		_BevelWidth;				// value[-1, 1]
uniform float		_BevelClamp;				// value[ 0, 1]
uniform float		_BevelRoundness;			// value[ 0, 1]

uniform sampler2D	_BumpMap;					// Normal map
uniform float		_BumpOutline;				// value[ 0, 1]
uniform float		_BumpFace;					// value[ 0, 1]

uniform samplerCUBE	_Cube;						// Cube / sphere map
uniform fixed4 		_ReflectFaceColor;			// RGB intensity
uniform fixed4		_ReflectOutlineColor;
//uniform float		_EnvTiltX;					// value[-1, 1]
//uniform float		_EnvTiltY;					// value[-1, 1]
uniform float3      _EnvMatrixRotation;
uniform float4x4	_EnvMatrix;

uniform fixed4		_SpecularColor;				// RGB intensity
uniform float		_LightAngle;				// value[ 0,Tau]
uniform float		_SpecularPower;				// value[ 0, 1]
uniform float		_Reflectivity;				// value[ 5, 15]
uniform float		_Diffuse;					// value[ 0, 1]
uniform float		_Ambient;					// value[ 0, 1]

uniform fixed4		_UnderlayColor;				// RGBA : Color + Opacity
uniform float		_UnderlayOffsetX;			// value[-1, 1]
uniform float		_UnderlayOffsetY;			// value[-1, 1]
uniform float		_UnderlayDilate;			// value[-1, 1]
uniform float		_UnderlaySoftness;			// value[ 0, 1]

uniform fixed4 		_GlowColor;					// RGBA : Color + Intesity
uniform float 		_GlowOffset;				// value[-1, 1]
uniform float 		_GlowOuter;					// value[ 0, 1]
uniform float 		_GlowInner;					// value[ 0, 1]
uniform float 		_GlowPower;					// value[ 1, 1/(1+4*4)]

// API Editable properties
uniform float 		_ShaderFlags;
uniform float		_WeightNormal;
uniform float		_WeightBold;

uniform float		_ScaleRatioA;
uniform float		_ScaleRatioB;
uniform float		_ScaleRatioC;

uniform float		_VertexOffsetX;
uniform float		_VertexOffsetY;

//uniform float		_UseClipRect;
uniform float		_MaskID;
uniform sampler2D	_MaskTex;
uniform float4		_MaskCoord;
uniform float4		_ClipRect;	// bottom left(x,y) : top right(z,w)
uniform float		_MaskSoftnessX;
uniform float		_MaskSoftnessY;

// Font Atlas properties
uniform sampler2D	_MainTex;
uniform float		_TextureWidth;
uniform float		_TextureHeight;
uniform float 		_GradientScale;
uniform float		_ScaleX;
uniform float		_ScaleY;
uniform float		_PerspectiveFilter;
uniform float		_Sharpness;
