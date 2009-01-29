/*
 * Copyright (c) 2007-2009 Asger Feldthaus
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

//----------------------------------------------------------------------------
// Shader for Tree trunks
//
// Hardware skinning with support for two directional lights (vertex-based).
//----------------------------------------------------------------------------

#define MAXBONES 20

float4x4 World;
float4x4 View;
float4x4 Projection;

// Should be:  InverseReferenceFrame * AbsoluteBoneTransform
float4x4 Bones[MAXBONES];

texture Texture;

float4 DirLight0DiffuseColor = float4(1,1,1,0);
float3 DirLight0Direction = float3(0,-1,0);			// Unit vector from light source towards object
bool DirLight0Enabled = true;

float4 DirLight1DiffuseColor = float4(1,1,0.8,0);
float3 DirLight1Direction = float3(1,0,0);
bool DirLight1Enabled = true;

float4 AmbientLight = float4(0.05,0.05,0.05,0);

sampler TextureSampler = sampler_state
{
	Texture = (Texture);
	AddressU = Wrap;
	AddressV = Wrap;
	AddressW = Wrap;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL;
    float2 TextureCoordinate : TEXCOORD0;
    int2 BoneIndex : TEXCOORD1;
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
    float4 worldPosition = mul(localPosition, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    
    output.TextureCoordinate = input.TextureCoordinate;
    
    float3 normal = mul(input.Normal, Bones[input.BoneIndex.x]);
	normal = normalize(mul(normal, World));
    
    output.Color = AmbientLight;
    if (DirLight0Enabled)
    {
		output.Color += DirLight0DiffuseColor * saturate(dot(normal, -DirLight0Direction));
    }
    if (DirLight1Enabled)
    {
		// We assume that DirLight1 is only enabled if DirLight0 is also enabled, so use += here
		output.Color += DirLight1DiffuseColor * saturate(dot(normal, -DirLight1Direction));
    }
		
	// Always set alpha to 1
	output.Color.a = 1.0;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	return tex2D(TextureSampler, input.TextureCoordinate) * input.Color;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 VertexShaderFunction();
		PixelShader = compile ps_1_1 PixelShaderFunction();
    }
}
