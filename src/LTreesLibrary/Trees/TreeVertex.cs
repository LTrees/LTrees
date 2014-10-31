using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Vertex with position, normal, texture coordinates, and 2 bone indices.
    /// There are no bone weights, so it is assumed that the two bones have equal weight.
    /// </summary>
    /// <remarks>
    /// The second bone index is currently unused.
    /// </remarks>
    public struct TreeVertex
    {
        public struct BoneIndex
        {
            public short Bone1;
            public short Bone2;
        }

        /// <summary>
        /// Position of the vertex, in object space.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Vertex normal, in object space.
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Texture coordinates.
        /// </summary>
        public Vector2 TextureCoordinate;

        /// <summary>
        /// Index of the bones. Set Bone1=Bone2 if only one bone is effective.
        /// </summary>
        /// <remarks>
        /// For the reference shader, this should be passed as a short2 in TEXCOORD1.
        /// </remarks>
        public BoneIndex Bones;

        public TreeVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate, short bone1, short bone2)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            Bones.Bone1 = bone1;
            Bones.Bone2 = bone2;
        }

        public TreeVertex(Vector3 position, Vector3 normal, Vector2 textureCoordinate, int bone1, int bone2)
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
            Bones.Bone1 = (short) bone1;
            Bones.Bone2 = (short) bone2;
        }

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new[] {
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(32, VertexElementFormat.Short2, VertexElementUsage.TextureCoordinate, 1),
        });
    }
}
