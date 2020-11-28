
#include "pbr_lib.fx"
#include "tone_mapping.fx"

//#include "vs_const_buffers.hlsli"
//#include "ps_vs_structs.hlsli"
//#include "common_functions.hlsli"



float4x4 World;
float4x4 View;
float4x4 Projection;


float3 cameraLookAt;

float4x4  ViewInverse;    // Inverse view?

float4x4  mRotEnv;
float4x4 rot_x;
float4x4 rot_y;

bool debug = true;
float debugVal = 0;
int show_reflections = false;
bool is_diffuse_linear = false;
bool is_specular_linear = false;
float exposure = 1;
bool scale_by_one_over_pi = false;

float light0_roughnessFactor = 1;
float light0_radiannce = 1;
float light0_ambientFactor = 1;


// Textures
Texture2D<float4> DiffuseTexture;
Texture2D<float4> SpecularTexture;
Texture2D<float4> NormalTexture;
Texture2D<float4> GlossTexture;

TextureCube<float4> tex_cube_diffuse;
TextureCube<float4> tex_cube_specular;
Texture2D<float4> specularBRDF_LUT;

SamplerState SampleType
{
	//Texture = <tex_cube_specular>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	Mipfilter = LINEAR;
	Filter = Anisotropic;
	MaxAnisotropy = 16;
	AddressU = Wrap;
	AddressV = Wrap;
};

SamplerState spBRDF_Sampler
{
	//Texture = <tex_cube_specular>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	Mipfilter = LINEAR;
	Filter = LINEAR;
	MaxAnisotropy = 16;
	AddressU = Clamp;
	AddressV = Clamp;
};

SamplerState s_normal
{
	//Texture = <tex_cube_specular>;
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	Mipfilter = LINEAR;
	Filter = LINEAR;
	MaxAnisotropy = 16;
	AddressU = Wrap;
	AddressV = Wrap;
};

//



struct VertexShaderInput
{
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 UVCoord : TEXCOORD0;
	float4 Tangent : TANGENT0;
	float3 binormal : BINORMAL;

};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : NORMAL0;
	float2 SampleUV : TEXCOORD0;
	float3 WorldPos : TEXCOORD1;
	float3 Tangent : TANGENT0;
	float3 Bitangent : BITANGENT0;
};

float3 LightDir = float3(-1, 0, 0);
matrix WorldViewProjection;
matrix Transform;
float AmbientBrightness;
float3 CameraPosition;
int FlipCulling = -1;

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(mul(input.Position, Transform), WorldViewProjection);
	output.WorldPos = input.Position; //mul(input.Position, Transform).xyz;
	output.Normal = normalize(mul(input.Normal, (float3x3)Transform));

	float3 tangent = normalize(mul(input.Tangent.xyz, (float3x3)Transform));
	float3 bitangent = cross(tangent.xyz, output.Normal.xyz).xyz * input.Tangent.w;
	output.Tangent = normalize(tangent);
	output.Bitangent = normalize(bitangent);
	output.SampleUV = input.UVCoord;// *UVTiling;

	return output;
}

// --------------------- Vertex shader
float GetAtten(float3 normal, float3 worldPos)
{
	return saturate(dot(normal, -LightDir));
}

float4 NormalToColor(float3 val)
{
	return float4(val * 0.5 + 0.5, 1.0);
}

// --------------------- Vertex shader End

// --------------------- Pixel shader
float substance_smoothness_get_our_smoothness(in float substance_smoothness)
{
	//	This value is correct for roughnesses from second_join_pos to 1.0.  This is valid for
	//	the end of the roughness curve...
	float original_roughness = 1.0f - substance_smoothness;

	float original_roughness_x2 = original_roughness * original_roughness;
	float original_roughness_x3 = original_roughness_x2 * original_roughness;
	float original_roughness_x4 = original_roughness_x3 * original_roughness;
	float original_roughness_x5 = original_roughness_x4 * original_roughness;

	return 1.0f - saturate((28.09f * original_roughness_x5) - (64.578f * original_roughness_x4) + (48.629f * original_roughness_x3) - (12.659f * original_roughness_x2) + (1.5459f * original_roughness));
}

static const float texture_alpha_ref = 0.7f;

void alpha_test(in const float pixel_alpha)
{
	clip(pixel_alpha - texture_alpha_ref);
}

//------

float GetMipFromRoughness(float roughness)
{
	return ((roughness) * 7.0);// - pow(roughness, 7.0) * 1.5;
}

#define SPECULAR
#define M_PI 3.141596
#define M_EPSILON 0.0001
#define ROUGHNESS_FLOOR 0.004
#define METALNESS_FLOOR 0.03
#define GAMMA 2.2

float3 FixCubeLookup(float3 v)
{
	float M = max(max(abs(v.x), abs(v.y)), abs(v.z));
	float scale = (128 - 1) / 128;

	if (abs(v.x) != M) v.x += scale;
	if (abs(v.y) != M) v.y += scale;
	if (abs(v.z) != M) v.z += scale;

	return v;
}

float2 GetIBLBrdf(in float ndv, in float roughness)
{
	return (specularBRDF_LUT.Sample(spBRDF_Sampler, float2(ndv, 1.0 - roughness))).xy;
}




float4 FromGamma(float4 val)
{
	return pow(val, 2.2);
}

float3 FromGamma(float3 val)
{
	return pow(val, 2.2);
}

float4 ToGamma(float4 val)
{
	return pow(val, 1.0 / 2.2);
}

float3 ToGamma(float3 val)
{
	return pow(val, 1.0 / 2.2);
}

float FromGamma(float val)
{
	return pow(val, 2.2);
}

float3 GetMetalnessSpecular(float4 diffColor, float metalness)
{
	return lerp(0.04, diffColor.rgb, clamp(metalness, 0, 1.0));
}

float4 GetMetalnessDiffuse(float4 diffColor, float metalness, float roughness)
{
	return float4(max(diffColor.rgb - diffColor.rgb * metalness, 0.04), diffColor.a);
}


float3 SchlickFresnel(float3 specular, float VdotH)
{
	return specular + (float3(1.0, 1.0, 1.0) - specular) * pow(1.0 - VdotH, 5.0);
}

float3 SchlickFresnelCustom(float3 specular, float LdotH)
{
	float ior = 0.25;
	float airIor = 1.000277;
	float f0 = (ior - airIor) / (ior + airIor);
	const float max_ior = 2.5;
	f0 = clamp(f0 * f0, 0.0, (max_ior - airIor) / (max_ior + airIor));
	return specular * (f0 + (1 - f0) * pow(2, (-5.55473 * LdotH - 6.98316) * LdotH));
}

//Get Fresnel
//specular  = the rgb specular color value of the pixel
//VdotH     = the dot product of the camera view direction and the half vector 
float3 Fresnel(float3 specular, float VdotH, float LdotH)
{
	//return SchlickFresnelCustom(specular, LdotH);
	return SchlickFresnel(specular, VdotH);
}

// Smith GGX corrected Visibility
// NdotL        = the dot product of the normal and direction to the light
// NdotV        = the dot product of the normal and the camera view direction
// roughness    = the roughness of the pixel
float SmithGGXSchlickVisibility(float NdotL, float NdotV, float roughness)
{
	float rough2 = roughness * roughness;
	float lambdaV = NdotL * sqrt((-NdotV * rough2 + NdotV) * NdotV + rough2);
	float lambdaL = NdotV * sqrt((-NdotL * rough2 + NdotL) * NdotL + rough2);

	return 0.5 / (lambdaV + lambdaL);
}

float NeumannVisibility(float NdotV, float NdotL)
{
	return NdotL * NdotV / max(1e-7, max(NdotL, NdotV));
}

// Get Visibility
// NdotL        = the dot product of the normal and direction to the light
// NdotV        = the dot product of the normal and the camera view direction
// roughness    = the roughness of the pixel
float Visibility(float NdotL, float NdotV, float roughness)
{
	return NeumannVisibility(NdotV, NdotL);
	//return SmithGGXSchlickVisibility(NdotL, NdotV, roughness);
}

// GGX Distribution
// NdotH        = the dot product of the normal and the half vector
// roughness    = the roughness of the pixel
float GGXDistribution(float NdotH, float roughness)
{
	float rough2 = roughness * roughness;
	float tmp = (NdotH * rough2 - NdotH) * NdotH + 1;
	return rough2 / (tmp * tmp);
}

// Get Distribution
// NdotH        = the dot product of the normal and the half vector
// roughness    = the roughness of the pixel
float Distribution(float NdotH, float roughness)
{
	return GGXDistribution(NdotH, roughness);
}

// Custom Lambertian Diffuse
// diffuseColor = the rgb color value of the pixel
// roughness    = the roughness of the pixel
// NdotV        = the normal dot with the camera view direction
// NdotL        = the normal dot with the light direction
// VdotH        = the camera view direction dot with the half vector
float3 CustomLambertianDiffuse(float3 diffuseColor, float NdotV, float roughness)
{
	return diffuseColor * (1.0 / M_PI) * pow(NdotV, 0.5 + 0.3 * roughness);
}

// Burley Diffuse
// diffuseColor = the rgb color value of the pixel
// roughness    = the roughness of the pixel
// NdotV        = the normal dot with the camera view direction
// NdotL        = the normal dot with the light direction
// VdotH        = the camera view direction dot with the half vector
float3 BurleyDiffuse(float3 diffuseColor, float roughness, float NdotV, float NdotL, float VdotH)
{
	const float energyBias = lerp(0, 0.5, roughness);
	const float energyFactor = lerp(1.0, 1.0 / 1.51, roughness);
	const float fd90 = energyBias + 2.0 * VdotH * VdotH * roughness;
	const float f0 = 1.0;
	const float lightScatter = f0 + (fd90 - f0) * pow(1.0f - NdotL, 5.0f);
	const float viewScatter = f0 + (fd90 - f0) * pow(1.0f - NdotV, 5.0f);

	return diffuseColor * lightScatter * viewScatter * energyFactor;
}

//Get Diffuse
// diffuseColor = the rgb color value of the pixel
// roughness    = the roughness of the pixel
// NdotV        = the normal dot with the camera view direction
// NdotL        = the normal dot with the light direction
// VdotH        = the camera view direction dot with the half vector
float3 Diffuse(float3 diffuseColor, float roughness, float NdotV, float NdotL, float VdotH)
{
	//return LambertianDiffuse(diffuseColor);
	//return CustomLambertianDiffuse(diffuseColor, NdotV, roughness);
	return BurleyDiffuse(diffuseColor, roughness, NdotV, NdotL, VdotH);
}


float3 GetSpecularDominantDir(float3 normal, float3 reflection, float roughness)
{
	const float smoothness = 1.0 - roughness;
	const float lerpFactor = smoothness * (sqrt(smoothness) + roughness);
	return lerp(normal, reflection, lerpFactor);
}

//Return the PBR BRDF value
// lightDir  = the vector to the light
// lightVec  = normalised lightDir
// toCamera  = vector to the camera
// normal    = surface normal of the pixel
// roughness = roughness of the pixel
// diffColor = the rgb color of the pixel
// specColor = the rgb specular color of the pixel
float3 GetBRDF(float3 worldPos, float3 lightDir, float3 lightVec, float3 toCamera, float3 normal, float roughness, float3 diffColor, float3 specColor)
{
	const float3 Hn = normalize(toCamera + lightDir);
	const float vdh = clamp((dot(toCamera, Hn)), M_EPSILON, 1.0);
	const float ndh = clamp((dot(normal, Hn)), M_EPSILON, 1.0);
	float ndl = clamp((dot(normal, lightVec)), M_EPSILON, 1.0);
	const float ndv = clamp((dot(normal, toCamera)), M_EPSILON, 1.0);
	const float ldh = clamp((dot(lightVec, Hn)), M_EPSILON, 1.0);

	const float3 diffuseFactor = Diffuse(diffColor, roughness, ndv, ndl, vdh) * ndl;
	float3 specularFactor = 0;

	const float3 fresnelTerm = Fresnel(specColor, vdh, ldh);
	const float distTerm = Distribution(ndh, roughness);
	const float visTerm = Visibility(ndl, ndv, roughness);
	specularFactor = distTerm * visTerm * fresnelTerm * ndl / M_PI;
	return diffuseFactor + specularFactor;
}


float3 get_environment_colourSpec(in float3 direction, in float lod)
{
	float3 value = AverageValue(saturate((tex_cube_specular.SampleLevel(SampleType, direction  /*texcoordEnvSwizzle(direction)*/, lod).rgb)));

	float powFactor = 2.2;	// 2.2  1 / 2.2
	return pow(value, powFactor);
}

float3 get_environment_colourDif(in float3 direction, in float lod)
{
	return saturate((tex_cube_diffuse.SampleLevel(SampleType, direction  /*texcoordEnvSwizzle(direction)*/, lod).rgb));
}

/// Calculate IBL contributation
///     reflectVec: reflection vector for cube sampling
///     wsNormal: surface normal in word space
///     toCamera: normalized direction from surface point to camera
///     roughness: surface roughness
///     ambientOcclusion: ambient occlusion
float3 ImageBasedLighting(in float3 reflectVec, in float3 wsNormal, in float3 toCamera, in float3 diffColor, in float3 specColor, in float roughness, inout float3 reflectionCubeColor)
{
	const float ndv = abs(dot(-toCamera, wsNormal)) + 0.001;

	const float mipSelect = GetMipFromRoughness(roughness);
	float3 cube = get_environment_colourSpec(FixCubeLookup(reflectVec), mipSelect);
	float3 cubeD = get_environment_colourSpec(FixCubeLookup(wsNormal), 7);
	//float3 cube = (texCUBElod(IBLSampler, float4(FixCubeLookup(reflectVec), )).rgb);
	//float3 cubeD = (texCUBElod(IBLSampler, float4(FixCubeLookup(wsNormal), 7.0)).rgb);

	const float3 Hn = normalize(toCamera + reflectVec);
	const float vdh = clamp((dot(toCamera, Hn)), M_EPSILON, 1.0);
	const float3 fresnelTerm = max(Fresnel(cube, vdh, vdh) * specColor, 0);

	const float2 brdf = GetIBLBrdf(ndv, roughness);
	const float3 environmentSpecular = (specColor * brdf.x + brdf.y) * cube;
	const float3 environmentDiffuse = cubeD * diffColor;

	return (environmentDiffuse * 1) + (environmentSpecular * 1) + (fresnelTerm * 1);
}





// --------------------- Pixel shader End



float4 PBRCommon(VertexShaderOutput input, float4 diffuse, float3 specColor, float roughness) : SV_TARGET0
{



	float3 normVal = NormalTexture.Sample(s_normal, input.SampleUV).xyz * 2.0 - 1.0;;

	//float3 normVal = tex2D(NormalMapSampler, input.SampleUV).xyz * 2.0 - 1.0;
	float3 eyeToPixel = normalize(input.WorldPos - CameraPosition);

	const float3x3 tbn = transpose(float3x3(input.Tangent, input.Bitangent * FlipCulling, input.Normal));
	float3 norm = normalize(mul(tbn, normVal).xyz);
	//float3 norm = perturb_normal(input.Normal, eyeToPixel, NormalMapSampler, input.SampleUV);

	float atten = GetAtten(norm, input.WorldPos);
	float4 oColor = float4(GetBRDF(input.WorldPos, -LightDir, normalize(-LightDir), -eyeToPixel, norm, roughness, diffuse.rgb, specColor), diffuse.a);

	float3 reflectVec = normalize(reflect(eyeToPixel, norm));
	float3 reflectionCubeColor = float3(0.25, 0.25, 0.25);
	float3 ibl = ImageBasedLighting(reflectVec, norm, -eyeToPixel, diffuse.rgb, specColor, roughness, reflectionCubeColor);

	float AOValue = 1;
	AOValue = AOValue < 0.001 ? 1.0 : AOValue;
	float emissive = 1;




	/// ambient term + direct-light + ibl
	float4 valu =
		diffuse +
		//float4(
		//(AmbientBrightness + emissive) * AOValue,
		//(AmbientBrightness + emissive) * AOValue,
		//(AmbientBrightness + emissive) * AOValue,
		//1) + 
		(oColor + float4(ibl, 1));

	float3 finalCol = valu.xyz;
	//finalCol = lumaBasedReinhardToneMapping(finalCol);
	finalCol = pow(valu,  1 / 2.2);

	return float4(finalCol,1);
}


float4 RoughSpecularPS(VertexShaderOutput input) : COLOR
{
		float powFactor = 2.2;	// 2.2  1 / 2.2

	float4 diffuse = DiffuseTexture.Sample(SampleType, input.SampleUV);
	float roughness = GlossTexture.Sample(SampleType, input.SampleUV).r;
	//roughness = 0;
	float3 specColor = SpecularTexture.Sample(SampleType, input.SampleUV).rgb;


	diffuse = pow(diffuse, powFactor);
	roughness = 1 - pow(roughness, powFactor);// *debugVal;
	specColor = pow(specColor, powFactor);



	//float4 diffuse = tex2D(DiffuseSampler, input.SampleUV);
	//float roughness = tex2D(RoughnessSampler, input.SampleUV).r;
	//roughness *= roughness;
	//roughness = max(roughness, 0.01);
	//float3 specColor = tex2D(MetalnessSampler, input.SampleUV).rgb;

	return PBRCommon(input, diffuse, specColor, roughness);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile vs_5_0 MainVS();
		PixelShader = compile ps_5_0 RoughSpecularPS();
	}
};