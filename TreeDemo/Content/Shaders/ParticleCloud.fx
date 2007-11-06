/* 
 * Copyright (c) 2007 Asger Feldthaus
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
 * and associated documentation files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:  
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

//
// Particle Cloud Shader by Asger Feldthaus
//

// There is a limited number of registers, so don't set MAX_PARTICLES too high.
#define MAX_PARTICLES 80

float4x4 WorldView : WORLDVIEW;
float4x4 World : WORLD;
float4x4 Projection : PROJECTION;

float3 Positions[MAX_PARTICLES];
float4 Colors[MAX_PARTICLES];
float4 Orientations[MAX_PARTICLES];

float3 BillboardRight = float3(1,0,0);
float3 BillboardUp = float3(0,1,0);

struct VertexInput
{
	float2 offset : POSITION;
	int2 particleId : TEXCOORD0;
};

struct VertexOutput
{
	float4 position : POSITION;
	float2 texCoords : TEXCOORD0;
	float4 color : COLOR;
};

texture Texture;

sampler Sampler = sampler_state
{
	Texture = (Texture);
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
};

//
//  Vertex shader
//
VertexOutput VS_Main(VertexInput input)
{
	VertexOutput vert = (VertexOutput)0;
	
	// Get the particle's index
	int id = input.particleId.x;
	
	// Calculate vertex position
	float2 offset = float2(
		input.offset.x * Orientations[id].x + input.offset.y * Orientations[id].z,
		input.offset.x * Orientations[id].y + input.offset.y * Orientations[id].w);
	vert.position = mul(float4(Positions[id],1), WorldView);
	vert.position += float4(offset.x * BillboardRight + offset.y * BillboardUp, 0);
	vert.position = mul(vert.position, Projection);
	
	// Set the texture coordinates
	vert.texCoords = (0.5 * input.offset + float2(0.5,0.5));
	
	// Calculate vertex color
	//float3 normal = mul(input.normal, World);
	//normal = normalize(normal);
	vert.color = Colors[id];//LightColor0 * input.color * float4(float3(1,1,1) * max(0.0, dot(normal, -LightDirection0)), 1);
	
	return vert;
}

//
//  Fragment shader
//
float4 PS_Main(VertexOutput vert) : COLOR
{
	// This is a particle system so keep the pixel shader as fast as possible.
	return tex2D(Sampler, vert.texCoords) * vert.color;
}

technique UnsortedParticles
{
	pass p0
	{
		CullMode = None;
		AlphaTestEnable = true;
		AlphaRef = 0x00000088;
		AlphaFunc = Greater;
		
		VertexShader = compile vs_2_0 VS_Main();
		PixelShader = compile ps_2_0 PS_Main();
	}
}

technique SortedParticles
{
	pass p0
	{
		AlphaTestEnable = false;
		AlphaBlendEnable = true;
		BlendOp = Add;
		DestBlend = InvSrcAlpha;
		SrcBlend = SrcAlpha;
		CullMode = None;
		ZWriteEnable = false;
		
		VertexShader = compile vs_2_0 VS_Main();
		PixelShader = compile ps_2_0 PS_Main();
	}
}