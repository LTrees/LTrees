/*
 * Copyright (c) 2007-2009 Asger Feldthaus
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */


#define MAXBONES 20

float4x4 WorldView;
float4x4 View;
float4x4 Projection;

texture Texture;

float4x4 Bones[MAXBONES];

float4 DirLight0DiffuseColor = float4(1,1,1,0);
float3 DirLight0Direction = float3(0,-1,0);			// Unit vector from light source towards object
bool DirLight0Enabled = true;

float4 DirLight1DiffuseColor = float4(1,1,0.8,0);
float3 DirLight1Direction = float3(1,0,0);
bool DirLight1Enabled = true;

float4 AmbientLight = float4(0.05,0.05,0.05,0);
float LeafScale = 1.0f;

float3 BillboardRight = float3(1,0,0);	// The billboard's right direction in view space
float3 BillboardUp = float3(0,1,0);		// The billboard's up direction in view space

sampler TextureSampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
    float2 Offset : TEXCOORD1;
    float4 Color : COLOR0;
    int2 BoneIndex : TEXCOORD2;
    float3 Normal : NORMAL;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TextureCoordinate : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
	
	float4 localPosition = mul(input.Position, Bones[input.BoneIndex.x]);
    float4 viewPosition = mul(localPosition, WorldView);
    
    viewPosition.xyz += (input.Offset.x * BillboardRight + input.Offset.y * BillboardUp) * LeafScale;
    
    output.Position = mul(viewPosition, Projection);

	output.TextureCoordinate = input.TextureCoordinate;
	
	float3 normal = mul(input.Normal, Bones[input.BoneIndex.x]);
	normal = mul(normal, WorldView);
	
	output.Color = AmbientLight;
	
	if (DirLight0Enabled)
	{
		output.Color += DirLight0DiffuseColor * (0.75 * saturate(dot(normal, -mul(DirLight0Direction, View))) + 0.25);
	}
	if (DirLight1Enabled)
	{
		output.Color += DirLight1DiffuseColor * (0.75 * saturate(dot(normal, -mul(DirLight1Direction, View))) + 0.25);
	}
	
	output.Color = output.Color * input.Color;
	
	output.Color.a = 1.0;
	
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	// we use a larger mipmap for the alpha channel so the leaves don't look transparent
    return float4(input.Color * tex2D(TextureSampler, input.TextureCoordinate).rgb, tex2Dbias(TextureSampler, float4(input.TextureCoordinate.xy, 1, -1)).a);
}

float4 PixelShaderFunctionOpaque(VertexShaderOutput input) : COLOR0
{
	float4 result = PixelShaderFunction(input);

	// XNA 4.0 doesn't support AlphaTestEnable state, so need to replicate
	// that functionality with this.
	clip((result.a < 230.0 / 255.0) ? -1 : 1);

	return result;
}

float4 PixelShaderFunctionBlendedEdges(VertexShaderOutput input) : COLOR0
{
	float4 result = PixelShaderFunction(input);

	// XNA 4.0 doesn't support AlphaTestEnable state, so need to replicate
	// that functionality with this.
	clip((result.a > 230.0 / 255.0) ? -1 : 1);

	return result;
}

technique Standard
{
    pass Opaque
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionOpaque();
        
        AlphaBlendEnable = false;
        
        ZEnable = true;
        ZWriteEnable = true;
        
        CullMode = None;
    }
    pass BlendedEdges
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunctionBlendedEdges();
        
        AlphaBlendEnable = true;
        SrcBlend = SrcAlpha;
        DestBlend = InvSrcAlpha;

        ZEnable = true;
        ZWriteEnable = false;

        CullMode = None;
    }
}
technique SetNoRenderStates
{
	pass Pass1
	{
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}