float4x4 World;
//float4x4 View;
//float4x4 Projection;
float4x4 WVP;
float4x4 BoneTransform;

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

struct LightComponents {
	float4 ambient;
	float4 diffuse;
	float4 spec;
};

struct materialProperty {
	float4 a_material;
	float4 d_material;
	float4 s_material;
};

uniform extern float3 eyePosition;
int numLights = 4;
uniform extern lightProperty light[4];
uniform extern materialProperty material;

struct VertexShaderInput
{
    float4 Position : POSITION0;
	float3 Normal : NORMAL0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
	float3 Pos : TEXCOORD0;
	float3 Normal : TEXCOORD1;

    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

struct PSoutput
{
	float4 Color : COLOR;
};

struct PSInput
{
	float3 Position : TEXCOORD0;
	float3 Normal : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    /*float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);*/
	output.Position = mul(float4(input.Position.xyz, 1.0f), BoneTransform);
    output.Position = mul(output.Position, WVP);
	output.Pos = output.Position;
	output.Normal = input.Normal;

    // TODO: add your vertex shader code here.

    return output;
}

PSoutput PixelShaderFunction(PSInput input)
{

	PSoutput output;
	int i;

	float3 TransP = mul(float4(input.Position, 1.0f), World).xyz;
    float3 TransN = mul(float4(input.Normal, 0.0f), World).xyz;   
    LightComponents liteComponents[1]; // my shader constraint, won't compile if too big 

	float3 V = normalize(eyePosition - TransP);   

	output.Color = float4(0.0f, 0.0f, 0.0f, 0.0f); 
    //output.ColorDiff = float4(0.0f, 0.0f, 0.0f, 0.0f); 

    for (i=0; i<numLights; i++) 
    {
		liteComponents[0].ambient = 0.0f;
		liteComponents[0].diffuse = 0.0f;
		liteComponents[0].spec = 0.0f;

		if (light[i].on != 0) 
		{
			liteComponents[0].ambient = light[i].ambientLight * material.a_material;
			
			// trick to not use if-then to generate correct code on SM2.0
			float3 L =	normalize((0 - light[i].lightDir.xyz) * light[i].is_pointLight +
							(light[i].position.xyz - TransP) * (1 - light[i].is_pointLight));

			float4 intensity = max(dot(TransN, L), 0);
			liteComponents[0].diffuse = light[i].diffuseLight * intensity * material.d_material;
			
			// specular effect for just the light specified specular is on
			float3 H = normalize(L+V);
			float4 spec_intensity = pow(max(dot(TransN, H), 0.000001f), light[i].shininess);
			liteComponents[0].spec = light[i].specLight * spec_intensity * material.s_material;
			
			float dist  = distance(light[i].position.xyz, TransP);
			float atten = light[i].attenuation.x +  light[i].attenuation.y*dist + light[i].attenuation.y*dist*dist; 			
			output.Color = output.Color + liteComponents[0].ambient + ((liteComponents[0].diffuse + liteComponents[0].spec) / atten);
		}
		//output.ColorDiff = output.ColorDiff + (liteComponents[i].diffuse / atten);

	}

    return output;
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
