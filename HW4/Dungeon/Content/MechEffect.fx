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

uniform extern int numLights;
uniform extern lightProperty light[4];

struct VertexShaderInput
{
    float4 Position : POSITION0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;

    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    /*float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);*/
	output.Position = mul(float4(input.Position.xyz, 1.0f), BoneTransform);
    output.Position = mul(output.Position, WVP);

    // TODO: add your vertex shader code here.

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // TODO: add your pixel shader code here.

    return float4(1, 0, 1, 1);
}

technique myTech
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
