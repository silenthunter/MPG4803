struct LightComponents {
	float4 ambient;
	float4 diffuse;
	float4 spec;
};

struct lightProperty {
	float4 position;
	float4 diffuseLight;
	float4 ambientLight;
	float4 specLight;
	float  shininess;
	int    on;
	float4 attenuation;
	int    is_pointLight;
	float4 lightDir;
};

struct materialProperty {
	float4 a_material;
	float4 d_material;
	float4 s_material;
};

uniform extern lightProperty light[4];
uniform extern materialProperty material;

uniform extern float4x4 gWVP;
uniform extern float4x4 gWorld;

uniform extern texture map;
uniform extern texture NormalMap;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float2 Texcoord : TEXCOORD0;
	float3 Pos : TEXCOORD1;
};

struct PSinput
{
	float2 Tex : TEXCOORD0;
	float3 Position : TEXCOORD1;
};

sampler textureBase = sampler_state
{
  Texture = <map>;
  mipfilter = LINEAR;
};

sampler NormalSampler = sampler_state
{
  Texture = <NormalMap>;
  mipfilter = LINEAR;
};

float3 expand(float3 v) { return (v-0.5)*2; }

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	output.Position = mul(float4(input.Position.xyz, 1.0f), gWVP);
	output.Pos = output.Position;
	output.Texcoord = input.Texcoord;

    return output;
}

float4 PixelShaderFunction(PSinput input) : COLOR0
{
	int numLights = 4;

	float4 retn = float4(0, 0, 0, 0);

	for(int i = 0; i < numLights; i++)
	{
		float3 lightDirection = normalize(light[i].position - input.Position);
		float3 lightVec = expand(lightDirection);
		float3 normalTex = tex2D(NormalSampler, input.Tex).xyz;
		float3 normal = expand(normalTex);

		float f = 2.0;
		// Diffuse lighting
		float4 color = dot(normal,lightVec);
		color *= pow((max(dot(normal, normalize(light[i].position)), 0)), f);
		retn += color;
	}

	return retn;
}

technique myTech
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
