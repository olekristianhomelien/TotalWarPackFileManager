
#include "pbr_lib.fx"
#include "tone_mapping.fx"

//#include "vs_const_buffers.hlsli"
//#include "ps_vs_structs.hlsli"
//#include "common_functions.hlsli"



float4x4 World;
float4x4 View;
float4x4 Projection;

float3 cameraPosition;
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

struct VertexInputType
{
	float4 position : POSITION;
	float3 normal : NORMAL0;
	float2 tex : TEXCOORD0;


	float3 tangent : TANGENT;
	float3 binormal : BINORMAL;
};



struct PixelInputType
{
	float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;

	float3 normal : NORMAL0;
	float3 normal2 : NORMAL1;
	float3 tangent : TANGENT;
	float3 binormal : BINORMAL;

	float3 viewDirection : TEXCOORD1;
	float3 worldPosition : TEXCOORD5;

};

float get_cube_env_scale_factor()
{
	return 1.0f;
}

float adjust_linear_smoothness(in const float linear_smoothness)
{
    //	return get_cubic_spline_adjusted_value(linear_smoothness, curve_y1_ctrl_pnt_env_smoothness, curve_y2_ctrl_pnt_env_smoothness, curve_y3_ctrl_pnt_env_smoothness);
    return linear_smoothness * linear_smoothness;
}

float3 texcoordEnvSwizzle(in float3 ref)
{
	//this should match the order of the basis
	return float3(-ref.x, ref.z, -ref.y);
}



float3 get_environment_colour(in float3 direction, in float lod)
{
	return (tex_cube_specular.SampleLevel(SampleType, direction  /*texcoordEnvSwizzle(direction)*/, lod).rgb);
}


float3 sample_environment_specular(in float roughness_in, in float3 reflected_view_vec)
{
#if 1
    //const float env_lod_pow = 1.8f;    
    const float env_lod_pow = 1.8f;
    const float env_map_lod_smoothness = adjust_linear_smoothness(1 - roughness_in);
    const float roughness = 1.0f - pow(env_map_lod_smoothness, env_lod_pow);

    float texture_num_lods = 5;
    float env_map_lod = roughness * (texture_num_lods - 1);//<------- LOWER = more reflective
    float3 environment_colour = get_environment_colour(reflected_view_vec, env_map_lod);
#else
    const float roughness = roughness_in;
    const float offset = 3;
    float texture_num_lods = 9.0f; // - offset;
    float env_map_lod = roughness * (texture_num_lods - 1);
    env_map_lod += offset;
    float3 environment_colour = get_environment_colour(reflected_view_vec, env_map_lod);
#endif

    float3 result = environment_colour * get_cube_env_scale_factor();

    return result;
}



// --------------------- Vertex shader
PixelInputType main(in VertexInputType input) // main is the default function name
{
	PixelInputType output;

	output.normal = input.normal;
	output.tangent = input.tangent;
	output.binormal = input.binormal;
	output.normal2 = input.normal;

	output.position = input.position;
	output.position = mul(output.position, rot_y);
	output.position = mul(output.position, rot_x);
	output.position = mul(output.position, World);

	float4 _V2 = output.position;
	float3 worldPosition = output.position.xyz;


	output.normal2 = float4(mul(input.normal, (float3x3) mRotEnv), 0).xyz;

	output.position = mul(output.position, View);
	output.position = mul(output.position, Projection);

	// Store the texture coordinates for the pixel shader.
	output.tex.x = input.tex.x;
	output.tex.y = input.tex.y;


	//-----------------------------------------------------------------------------
	//          Normal
	//-----------------------------------------------------------------------------                
	output.normal = mul(output.normal, (float3x3) rot_y);
	output.normal = normalize(output.normal);

	output.normal = mul(output.normal, (float3x3) rot_x);
	output.normal = normalize(output.normal);

	output.normal = mul(output.normal, (float3x3) World);
	output.normal = normalize(output.normal);


	output.normal2 = mul(output.normal2, (float3x3) rot_y);
	output.normal2 = normalize(output.normal2);

	output.normal2 = mul(output.normal2, (float3x3) rot_x);
	output.normal2 = normalize(output.normal2);

	output.normal2 = mul(output.normal2, (float3x3) World);
	output.normal2 = normalize(output.normal2);


	//-----------------------------------------------------------------------------
	//          Tangent
	//-----------------------------------------------------------------------------        
	output.tangent = mul(output.tangent, (float3x3) rot_y);
	output.tangent = normalize(output.tangent);

	output.tangent = mul(output.tangent, (float3x3) rot_x);
	output.tangent = normalize(output.tangent);

	output.tangent = mul(output.tangent, (float3x3) World);
	output.tangent = normalize(output.tangent);

	//-----------------------------------------------------------------------------
	//          Binormal
	//-----------------------------------------------------------------------------        
	output.binormal = mul(output.binormal, (float3x3) rot_y);
	output.binormal = normalize(output.binormal);

	output.binormal = mul(output.binormal, (float3x3) rot_x);
	output.binormal = normalize(output.binormal);

	output.binormal = mul(output.binormal, (float3x3) World);
	output.binormal = normalize(output.binormal);

	// Calculate the position of the vertex in the world.
	float3 camW = mul(float4(cameraPosition, 1), World).xyz;
	camW = cameraPosition.xyz;
	output.worldPosition = worldPosition;

	output.viewDirection = cameraLookAt - _V2.xyz;
	output.viewDirection.xyz = normalize((float3) ViewInverse[3] - worldPosition);
	//output.viewDirection = normalize(output.viewDirection).xyz;;
	//output.viewDirection = -normalize(worldPosition - float4(cameraPosition, 1).xyz).xyz;
	return output;

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

/*
float4 mainPs(in PixelInputType _input, bool bIsFrontFace : SV_IsFrontFace) : SV_TARGET0
{
	PixelInputType input;
	input = _input;


	if (bIsFrontFace)
	{
		input.normal *= -1;
		input.tangent *= -1;
		input.binormal *= -1;
	}

	float4 DiffuseTex = DiffuseTexture.Sample(SampleType, input.tex);


	float4 GlossTex = GlossTexture.Sample(SampleType, input.tex);


	float4 SpecTex = SpecularTexture.Sample(SampleType, input.tex);


	DiffuseTex = pow(DiffuseTex,  2.2);
	GlossTex = pow(GlossTex,  2.2);
	SpecTex = pow(SpecTex,  2.2);


	float alpha = DiffuseTex.a;


			float4 NormalTex = NormalTexture.Sample(s_normal, input.tex);


			float smoothness = GlossTex.r;
		   smoothness = substance_smoothness_get_our_smoothness(smoothness);
		   float roughness = saturate((1 - smoothness) * light0_roughnessFactor);

		   // Sample the pixel in the bump tex_cube_specular.
		   float3x3 basis = float3x3(normalize(input.tangent), normalize(input.normal), normalize(input.binormal)); // -- WOWRK2!pp§
		   float3x3 basis2 = float3x3(normalize(input.tangent), normalize(input.normal2), normalize(input.binormal)); // -- WOWRK2!pp§


		   // Deccode the TW nortex_cube_specular with orthogonal projection
		   float3 Np;

		   Np.x = NormalTex.r * NormalTex.a;
		   Np.y = NormalTex.g;
		   Np = (Np * 2.0f) - 1.0f;
		   Np.z = sqrt(1 - Np.x * Np.x - Np.y * Np.y);


		   float3 _N = Np.yzx; // Works

		   float3 bumpNormal = normalize(mul(normalize(_N), basis));
		   float3 bumpNormal2 = normalize(mul(normalize(_N), basis2));
		   // ************************************************************************
			   //bumpNormal = input.normal; // enable to DISABLE normal tex_cube_specular
			   //bumpNormal2 = input.normal2; // enable to DISABLE normal tex_cube_specular
		   // ************************************************************************	

			   float3 N = normalize(bumpNormal);

			   float3 N2 = normalize(bumpNormal2);
			   N2 = mul(N, (float3x3) mRotEnv);
			   N2 = normalize(N2);

			   float3 Lo = normalize(input.viewDirection);



			   // Angle between surface normal and outgoing light direction.
			   float cosLo = max(0.0, dot(N, Lo));

			   // Specular reflection vector.
			   float3 Lr = 2.0 * cosLo * N - Lo;
			   //float3 Lr = reflect(N, Lo); test code

			   Lr = mul(Lr, (float3x3) mRotEnv);

			   // Fresnel reflectance at normal incidence (for metals use albedo color).
			   float3 F0 = SpecTex.rgb;//lerp(Fdielectric, albedo, metalness);



			 //float3 irradiance = tex_cube_diffuse.Sample(SampleType, N2).rgb;
			 float3 irradiance = tex_cube_diffuse.Sample(SampleType, N2).rgb; // DEBUG CODE
			 float3 specularIrradiance = sample_environment_specular(roughness, normalize(Lr));


			 irradiance = pow(irradiance,  2.2);
			 specularIrradiance = pow(specularIrradiance,  2.2);
			 //
			  float3 F = fresnelSchlickRoughness(max(dot(N, Lo), 0.0), F0, roughness); // TEST CODE
			  float3 kS = F;
			  float3 kD = 1.0 - kS;
			  float3 diffuseIBL = kD * DiffuseTex.rgb * irradiance;




			  // specularIrradiance = AverageValue(specularIrradiance);

			   float2 brdf = specularBRDF_LUT.Sample(spBRDF_Sampler, float2(cosLo, 1.0 - roughness)).xy;
			   float3 specularIBL = F0 * (brdf.x + brdf.y) * specularIrradiance;



			   float3 ambientLighting = (specularIBL + diffuseIBL); // * light0_ambientFactor;		



		   float3 hdrColor = ((ambientLighting * exposure));

		   // if (debug)
		   //	  return float4(diffuseIBL, 1);
			 hdrColor = lumaBasedReinhardToneMapping(hdrColor);
			 // hdrColor = pow(hdrColor, 2.2);
			 hdrColor = pow(hdrColor, 1 / 2.2);
			  return float4(hdrColor, 1);
}

// --------------------- Pixel shader End
*/


float4 mainPs(in PixelInputType _input, bool bIsFrontFace : SV_IsFrontFace) : SV_TARGET0
{

    PixelInputType input;
    input = _input;

    if (bIsFrontFace)
    {
        input.normal *= -1;
        input.tangent *= -1;
        input.binormal *= -1;
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////// texture sample    
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////    


    float4 DiffuseTex;
    float4 FactionMaskTex;

    float4 NormalTex;
    float4 SpecTex;
    float4 GlossTex;

    float3 bumpNormal;
    float3 lightDir;
    float lightIntensity;
    float4 color;
    float4 specularIntensity;
    float3 reflection;
    float4 specular;
    float4 l_diffuse;
    float alpha;

    NormalTex = NormalTexture.Sample(s_normal, input.tex);
    DiffuseTex = DiffuseTexture.Sample(SampleType, input.tex);
   // FactionMaskTex = shaderTextures[3].Sample(SampleType, input.tex);
    SpecTex = SpecularTexture.Sample(SampleType, input.tex);
    GlossTex = GlossTexture.Sample(SampleType, input.tex);

    // store alpha, so it is no being change if any gamme stuff is applied to this sample    
    alpha = DiffuseTex.a;

    // needed for custom textures in mods that are not SRGB, and most are not, set flag outside shader based on .DDS texture format
   // if (is_diffuse_linear)
        DiffuseTex = pow(DiffuseTex, 2.2);

    //if (is_specular_linear)
        SpecTex = pow(SpecTex, 2.2);


    // uses CA own code, though edited quite a bit, to do faction coloring, set 3 example color here to try out     
    //float3 c1 = color1 / 255.0f;
    //float3 c2 = color2 / 255.0f;
    //float3 c3 = color3 / 255.0f;
    //
    //apply_faction_colours(DiffuseTex.rgb, shaderTextures[3], SampleType, input.tex, c1, c2, c3);


    DiffuseTex.rgb = DiffuseTex.rgb * (1 - max(SpecTex.b, max(SpecTex.r, SpecTex.g)));

    float spec_intensity = 1.0;

    // for setting rendering modes for different TW games
    float smoothness = GlossTex.r;

    // correct the smoothness curve to fit CA's engine (CA code)
    smoothness = substance_smoothness_get_our_smoothness(smoothness);

    // roughness, external render param "light[0].roughnessFactor" to scale up and down for this material property   
	float roughness = saturate((1 - smoothness));// *light[0].roughnessFactor);

    float3x3 basis = float3x3(normalize(input.tangent), normalize(input.normal), normalize(input.binormal)); // -- WOWRK2!pp§
    float3x3 basis2 = float3x3(normalize(input.tangent), normalize(input.normal2), normalize(input.binormal)); // -- WOWRK2!pp§


    // Deccode the TW nortex_cube_specular with orthogonal projection
    float3 Np;
    float4 n;

    Np.x = NormalTex.r * NormalTex.a;
    Np.y = NormalTex.g;
    Np = (Np * 2.0f) - 1.0f;

    Np.z = sqrt(1 - Np.x * Np.x - Np.y * Np.y);

    float3 _N = Np.yzx; // Works
    //float3 _N = Np.xzy; // rome2 ?

    bumpNormal = normalize(mul(normalize(_N), basis));
    float3 bumpNormal2 = normalize(mul(normalize(_N), basis));
    // ************************************************************************
    //bumpNormal = input.normal; // enable to DISABLE normal tex_cube_specular
    //bumpNormal2 = input.normal2; // enable to DISABLE normal tex_cube_specular
    // ************************************************************************	

    float3 N = normalize(bumpNormal);
    float3 N2 = normalize(bumpNormal2);

    float3 N_rotated = mul(N, (float3x3) mRotEnv);
    N_rotated = normalize(N_rotated);
    N_rotated = N;

    N2 = mul(N, (float3x3) mRotEnv);
    N2 = normalize(N2);


    float3 Lo = normalize(input.viewDirection);

    // Angle between surface normal and outgoing light direction.
    float cosLo = max(0.0, dot(N, Lo));

    // Specular reflection vector.
   // float3 Lr = 2.0 * cosLo * N- Lo;  // written out reflect formula
    float3 Lr = reflect(N, Lo); // HLSL intrisic reflection function  

    // rotate refletion map by rotating the reflect vector
    Lr = mul(Lr, (float3x3) mRotEnv);

    // specular    
    float3 F0 = SpecTex.rgb;

    // Direct light, non-ambient types
    float3 directLighting = 0.0;

    float3 ambientLighting;
    float3 specularIrradiance;
    float3 specularIBL;
    {
        // Sample diffuse irradiance at normal direction.        
        float3 irradiance = tex_cube_diffuse.Sample(SampleType, N2).rgb;
       // irradiance = pow(irradiance, 2.2);

        // Calculate Fresnel term for ambient lighting.
        // Since we use pre-filtered cubemap(s) and irradiance is coming from many directions
        // use cosLo instead of angle with light's half-vector (cosLh above).
        // See: https://seblagarde.wordpress.com/2011/08/17/hello-world/

        //float3 F = fresnelSchlick(F0, cosLo); // alternative code
        float3 F = fresnelSchlickRoughness(cosLo, F0, roughness);

        float3 kS = F;
        float3 kD = 1.0 - kS;

        // Irradiance map contains exitant radiance assuming Lambertian BRDF, no need to scale by 1/PI here either.
        float3 diffuseIBL = 0;
        //if (debug_flags.scale_by_one_over_pi)
        //    diffuseIBL = kD * DiffuseTex.rgb * irradiance;
        //else
        //    diffuseIBL = kD * DiffuseTex.rgb * irradiance * (1 / PI);

         diffuseIBL = kD * DiffuseTex.rgb * irradiance;


         // Sample pre-filtered specular reflection environment at correct mipmap level.     
         specularIrradiance = sample_environment_specular(roughness, normalize(Lr));

         // make the specular gradually become diffuse-like = softer images
         //specularIrradiance = lerp(specularIrradiance, irradiance, roughness * roughness); // -- alternative

         float2 brdf = specularBRDF_LUT.Sample(spBRDF_Sampler, float2(cosLo, (1.0 - roughness))).xy;

         // Edited to look more WH2 like
         specularIBL = F0 * (brdf.x + brdf.y) * specularIrradiance;

         // The "full pbr" (yes is it "F", not F0 :) )
         //specularIBL = (F*brdf.x + brdf.y) * specularIrradiance;

         // sum        
		 ambientLighting = (specularIBL + diffuseIBL);// * light[0].ambientFactor;


         //if (show_reflections)
         //   specularIBL = specularIrradiance;
		 //
         //if (debug_flags.irrandiace_only)
         //    specularIBL = irradiance;
     }

     float d_light_factor = 1.0;

    //f (debug_flags.other_flags.reflections_only == true)
    //   d_light_factor = 0.0;
	//
    //if (show_reflections == 1 || debug_flags.irrandiace_only)
    //    color = float4(specularIBL, 1);
    //else
     color = float4(directLighting + ambientLighting, 1.0);

     //if (has_alpha == 1)
     //{
     //    alpha_test(alpha);
     //}


     color.a = alpha;

     const float gamma_value = 2.2;

     float3 hdrColor = color.rgb * exposure * 0.9;
     float3 mapped = Uncharted2ToneMapping(hdrColor);

     // different types of tone mapping
     //float3 mapped = simpleReinhardToneMapping(hdrColor);       
     //float3 mapped = lumaBasedReinhardToneMapping(hdrColor);
     mapped = pow(mapped, 1.0 / gamma_value);
    // mapped = N + mapped * debugVal;
	 //
     //if (debug_flags.transparent == 1)
     //    color = float4(mapped, 0.7);
     //else
         color = float4(mapped, 1);

     return color;

}
float4 SimplePixel(in PixelInputType _input/*, bool bIsFrontFace : SV_IsFrontFace*/) : SV_TARGET0
{
	return float4(1,0,0,1);
}


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile vs_5_0 main();
		PixelShader = compile ps_5_0 mainPs();
	}
};