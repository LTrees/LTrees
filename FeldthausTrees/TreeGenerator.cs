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

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;

namespace Feldthaus.Xna
{
    /// <summary>
    /// Class for procedurally generating trees. One tree generator holds the profile of the kind of tree it generates.
    /// The tree profile can be loaded from an XML file using LoadFromFile.
    /// </summary>
    public sealed class TreeGenerator
    {
        #region Local types
        /// <summary>
        /// A value that is uniformly distributed between two values. Can also be a fixed value.
        /// </summary>
        /// <typeparam name="T">Type of the value, typically int or float.</typeparam>
        public struct ValueRange<T>
        {
            public bool isFixedValue;
            public T min;
            public T max;

            public ValueRange(T value)
            {
                isFixedValue = true;
                min = value;
                max = value;
            }

            public ValueRange(T pmin, T pmax)
            {
                isFixedValue = false;
                min = pmin;
                max = pmax;
            }

            public void Assign(T value)
            {
                isFixedValue = true;
                min = value;
                max = value;
            }

            public void Assign(T pmin, T pmax)
            {
                isFixedValue = false;
                min = pmin;
                max = pmax;
            }
        }

        /// <summary>
        /// The profile of a branch child. This does not represent a branch on any tree, but a blueprint for
        /// how to place and size child branches.
        /// </summary>
        public struct BranchChild
        {
            public int idRef;
            public ValueRange<int> relativeLevel;
            public ValueRange<int> levelRange;
            public ValueRange<float> position;
            public ValueRange<float> lengthScale;
            public ValueRange<float> radiusScale;
            public ValueRange<float> orientation;
            public ValueRange<float> pitch;
            public ValueRange<float> gravityInfluence;
            public ValueRange<int> count;

            /// <summary>
            /// Constructor that assigns defualt values to everything.
            /// The parameter is only there to separate it from the default constructor.
            /// </summary>
            /// <param name="dummy">Set it to anything. It has no effect.</param>
            public BranchChild(bool dummy)
            {
                idRef = -1;
                relativeLevel = new ValueRange<int>(-1);
                levelRange = new ValueRange<int>(0, 100);
                position = new ValueRange<float>(0f, 1f);
                lengthScale = new ValueRange<float>(0.6f);
                radiusScale = new ValueRange<float>(1.0f);
                orientation = new ValueRange<float>(0f, 360f);
                pitch = new ValueRange<float>(0f);
                gravityInfluence = new ValueRange<float>(0f);
                count = new ValueRange<int>(1);
            }

            public static BranchChild Create()
            {
                return new BranchChild(false);
            }
        }

        /// <summary>
        /// The profile of a leaf type. This does not represent a leaf on any tree or branch, but a blueprint for
        /// how to create new leaves.
        /// </summary>
        public struct Leaf
        {
            public ValueRange<int> levelRange;
            public ValueRange<float> width;
            public ValueRange<float> height;
            public ValueRange<float> scale;
            public ValueRange<float> position;
            public ValueRange<float> roll;
            public Vector3 axis;
            public bool hasAxis;
            public ValueRange<float> anchor;
            public ValueRange<int> count;
            public ValueRange<int> red;
            public ValueRange<int> green;
            public ValueRange<int> blue;
            public ValueRange<int> alpha;

            public Leaf(bool dummy)
            {
                levelRange = new ValueRange<int>(0, 100);
                width = new ValueRange<float>(10f);
                height = new ValueRange<float>(10f);
                scale = new ValueRange<float>(1f);
                position = new ValueRange<float>(1f);
                roll = new ValueRange<float>(0f, 360f);
                axis = new Vector3(0, 1, 0);
                hasAxis = false;
                anchor = new ValueRange<float>(0f);
                count = new ValueRange<int>(1);
                red = new ValueRange<int>(255);
                green = new ValueRange<int>(255);
                blue = new ValueRange<int>(255);
                alpha = new ValueRange<int>(255);
            }

            public static Leaf Create()
            {
                return new Leaf(false);
            }
        }

        /// <summary>
        /// The profile of a branch. Does not represent a branch on any tree, but rather a blueprint of how to create
        /// new branches.
        /// </summary>
        public struct Branch
        {
            public int id;
            public ValueRange<float> length;
            public ValueRange<float> radius;
            public ValueRange<float> radiusEnd;
            public List<BranchChild> children;
            public List<Leaf> leaves;

            /// <summary>
            /// Creates a new branch type with specified children and leaf lists.
            /// </summary>
            /// <param name="pChildren">list of children</param>
            /// <param name="pLeaves">list of leaves</param>
            public Branch(List<BranchChild> pChildren, List<Leaf> pLeaves)
            {
                id = -1;
                length = new ValueRange<float>(10f);
                radius = new ValueRange<float>(5f);
                radiusEnd = new ValueRange<float>(0.5f);
                children = pChildren;
                leaves = pLeaves;
            }

            /// <summary>
            /// Creates a new branch struct, initializing children and leaves for you.
            /// </summary>
            /// <returns></returns>
            public static Branch Create()
            {
                return new Branch(new List<BranchChild>(), new List<Leaf>());
            }
        }
        #endregion
        #region Private variables

        /* Configuration */
        GraphicsDevice graphicsDevice;
        ParticleCloudSystem cloudSystem;

        /* Tree Profile */
        List<Branch> branches = new List<Branch>();
        int rootIdRef;

        /* Parameter placeholders */
        bool addLeaves;
        int radialSegments;
        int levels;
        int cutoffLevel;
        int seed;
        int leafSeed;
        
        /* Resulting tree */
        BoundingBox boundingBox;
        List<VertexPositionNormalTexture> treeVertices = new List<VertexPositionNormalTexture>();
        List<int> treeIndices = new List<int>();
        ParticleCloud cloud;

        BasicEffect trunkEffect;
        Texture leafTexture;

        private static readonly NumberFormatInfo numberFormat = new NumberFormatInfo();

        #endregion
        #region Properties
        /// <summary>
        /// ID of the root branch. The root branch will make up the trunk of generated trees.
        /// </summary>
        /// <remarks>
        /// The root ID is part of the tree profile loaded from an XML file, so you usually don't have to set this.
        /// </remarks>
        public int RootIdRef
        {
            get { return rootIdRef; }
            set { rootIdRef = value; }
        }

        /// <summary>
        /// How many levels of branches will be generated. Branches at this level will be forced to be closed in the end,
        /// and branches beyond this level are not added at all.
        /// </summary>
        /// <remarks>
        /// The Levels property is part of the tree profile loaded from an XML file, so you usually don't have to set this.
        /// </remarks>
        public int Levels
        {
            get { return levels; }
            set { levels = value; }
        }

        /// <summary>
        /// Gets the basic effect applied to the trunk of every newly-generated tree.
        /// The trunk's effect can be changed after the tree has been created, if desired.
        /// </summary>
        public BasicEffect TrunkEffect
        {
            get { return trunkEffect; }
        }

        /// <summary>
        /// Gets or sets the texture applied to the leaves of newly-generated trees. Default is null.
        /// If the leaves have no texture, they will be invisible.
        /// </summary>
        public Texture LeafTexture
        {
            get { return leafTexture; }
            set { leafTexture = value; }
        }
        #endregion

        /// <summary>
        /// Constructor. The graphics device must be sent along, as it is needed to create new vertex buffers.
        /// The particle cloud system is also required, since the leaves are represented as a particle cloud.
        /// </summary>
        /// <param name="device">The graphics device.</param>
        /// <param name="cloudSystem">The particle cloud system. One system can be shared among many tree generators.</param>
        public TreeGenerator(GraphicsDevice device, ParticleCloudSystem cloudSystem)
        {
            graphicsDevice = device;
            this.cloudSystem = cloudSystem;
            trunkEffect = new BasicEffect(device, new EffectPool());
        }

        /// <summary>
        /// Adds a new branch type to the generator's tree profile. The branch type's index is returned.
        /// </summary>
        /// <remarks>
        /// This is here for flexibility, but you usually want to use LoadFromFile to load the tree profile
        /// instead of calling this manually.
        /// </remarks>
        /// <param name="branch">The branch type to add.</param>
        public int AddBranch(Branch branch)
        {
            branches.Add(branch);

            return branches.Count - 1;
        }

        #region XML Tree Profile Loading

        /// <summary>
        /// Loads all settings from an XML tree profile.
        /// </summary>
        /// <param name="filename">Filename of the XML file to load.</param>
        public void LoadFromFile(string filename)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);

            XmlElement tree = document.DocumentElement;
            for (int i=0; i<tree.ChildNodes.Count; i++)
            {
                XmlNode child = tree.ChildNodes[i];
                if (child.Name == "branch")
                {
                    LoadXmlBranch(child);
                }
                else if (child.Name == "root")
                {
                    rootIdRef = GetIntAttribute(child, "ref", -1);
                    levels = GetIntAttribute(child, "levels", 4);
                }
            }
        }
        
        // TODO: Avoid exceptions when using int.Parse and float.Parse. (Maybe use TryParse instead?)
        private bool AssignValueRange(XmlNode node, string attributeName, ref ValueRange<float> value)
        {
            if (node == null)
                return false;

            XmlNode attr = node.Attributes.GetNamedItem(attributeName);
            if (attr == null)
                return false;

            value.Assign(float.Parse(attr.Value, numberFormat));

            return true;
        }
        private bool AssignValueRange(XmlNode node, string attributeName, ref ValueRange<int> value)
        {
            if (node == null)
                return false;

            XmlNode attr = node.Attributes.GetNamedItem(attributeName);
            if (attr == null)
                return false;

            value.Assign(int.Parse(attr.Value, numberFormat));

            return true;
        }

        private bool AssignValueRange(XmlNode node, ref ValueRange<float> value)
        {
            if (node == null)
                return false;

            // Are we given a fixed value?
            XmlNode attr = node.Attributes.GetNamedItem("value");
            if (attr != null)
            {
                value.min = value.max = float.Parse(attr.Value, numberFormat);
                value.isFixedValue = true;
                return true;
            }
            
            // See if the minimum is specified.
            attr = node.Attributes.GetNamedItem("min");
            if (attr != null)
            {
                value.isFixedValue = false;
                value.min = float.Parse(attr.Value, numberFormat);
            }

            // See if the maximum is specified.
            attr = node.Attributes.GetNamedItem("max");
            if (attr != null)
            {
                value.isFixedValue = false;
                value.max = float.Parse(attr.Value, numberFormat);
            }

            return true;
        }
        private bool AssignValueRange(XmlNode node, ref ValueRange<int> value)
        {
            if (node == null)
                return false;

            // Are we given a fixed value?
            XmlNode attr = node.Attributes.GetNamedItem("value");
            if (attr != null)
            {
                value.min = value.max = int.Parse(attr.Value, numberFormat);
                value.isFixedValue = true;
                return true;
            }

            // See if the minimum is specified.
            attr = node.Attributes.GetNamedItem("min");
            if (attr != null)
            {
                value.isFixedValue = false;
                value.min = int.Parse(attr.Value, numberFormat);
            }

            // See if the maximum is specified.
            attr = node.Attributes.GetNamedItem("max");
            if (attr != null)
            {
                value.isFixedValue = false;
                value.max = int.Parse(attr.Value, numberFormat);
            }

            return true;
        }

        private float ParseFloat(string s, ref int i)
        {
            string buffer = "";
            
            // Skip commas and whitespace
            while (i < s.Length && (s[i] == ' ' || s[i] == ','))
            {
                i++;
            }

            // Parse until end of string, whitespace, or comma
            while (i < s.Length && s[i] != ' ' && s[i] != ',')
            {
                buffer += s[i];
                i++;
            }

            // Move past whitespace and commas again
            while (i < s.Length && (s[i] == ' ' || s[i] == ','))
            {
                i++;
            }

            // If we didn't read anything useful, just return 0f.
            if (buffer.Length == 0)
                return 0f;

            // Parse the string we read.
            return float.Parse(buffer, numberFormat);
        }

        private bool AssignVec3(XmlNode node, ref Vector3 value)
        {
            if (node == null)
                return false;

            XmlNode attr = node.Attributes.GetNamedItem("value");
            if (attr == null)
                return false;

            string s = attr.Value;

            if (s.Length == 0)
                return false;

            int index = 0;
            value.X = ParseFloat(s, ref index);
            value.Y = ParseFloat(s, ref index);
            value.Z = ParseFloat(s, ref index);

            return true;
        }

        private int GetIntAttribute(XmlNode node, string attributeName, int defaultValue)
        {
            if (node == null)
                return defaultValue;

            XmlNode attr = node.Attributes.GetNamedItem(attributeName);
            if (attr == null)
                return defaultValue;

            return int.Parse(attr.Value, numberFormat);
        }

        private void LoadXmlBranch(XmlNode node)
        {
            Branch branch = Branch.Create();
            branch.id = GetIntAttribute(node, "id", -1);

            // We must have a valid id
            if (branch.id < 0)
                return;

            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlNode childnode = node.ChildNodes[i];
                // Consider the node's name..
                switch (childnode.Name)
                {
                    case "length":
                        AssignValueRange(childnode, ref branch.length);
                        break;

                    case "radius":
                        AssignValueRange(childnode, ref branch.radius);
                        break;

                    case "radiusEnd":
                        AssignValueRange(childnode, ref branch.radiusEnd);
                        break;

                    case "child":
                        LoadXmlBranchChild(childnode, branch.children);
                        break;

                    case "leaf":
                        LoadXmlLeaf(childnode, branch.leaves);
                        break;

                    default:
                        break;
                }
            }

            AddBranch(branch);
        }

        private bool LoadXmlBranchChild(XmlNode node, List<BranchChild> children)
        {
            BranchChild child = BranchChild.Create();
            child.idRef = GetIntAttribute(node, "ref", -1);

            // Must have a valid branch reference id
            if (child.idRef < 0)
                return false;

            // Get the relative level short-hand from the attribute (if available)
            AssignValueRange(node, "level", ref child.relativeLevel);
            
            // Let all subelements apply to the new branch type
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlNode childnode = node.ChildNodes[i];
                switch (childnode.Name)
                {
                    case "relativeLevel":
                        AssignValueRange(childnode, ref child.relativeLevel);
                        break;

                    case "levelRange":
                        AssignValueRange(childnode, ref child.levelRange);
                        break;

                    case "position":
                        AssignValueRange(childnode, ref child.position);
                        break;

                    case "lengthScale":
                        AssignValueRange(childnode, ref child.lengthScale);
                        break;

                    case "radiusScale":
                        AssignValueRange(childnode, ref child.radiusScale);
                        break;

                    case "orientation":
                        AssignValueRange(childnode, ref child.orientation);
                        break;

                    case "pitch":
                        AssignValueRange(childnode, ref child.pitch);
                        break;

                    case "gravity":
                        AssignValueRange(childnode, ref child.gravityInfluence);
                        break;

                    case "count":
                        AssignValueRange(childnode, ref child.count);
                        break;
                }
            }

            // Add the branchchild to the array
            children.Add(child);

            return true;
        }

        private bool LoadXmlLeaf(XmlNode node, List<Leaf> leaves)
        {
            if (node == null)
                return false;

            Leaf leaf = Leaf.Create(); // TODO: Replace the .Create() methods with .Default constants.

            // Use the shorthand attribute "level" for levelRange, if available
            AssignValueRange(node, "level", ref leaf.levelRange);

            // Let all subelements apply to the new leaf
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlNode childnode = node.ChildNodes[i];
                switch (childnode.Name)
                {
                    case "levelRange":
                        AssignValueRange(childnode, ref leaf.levelRange);
                        break;

                    case "width":
                        AssignValueRange(childnode, ref leaf.width);
                        break;

                    case "height":
                        AssignValueRange(childnode, ref leaf.height);
                        break;

                    case "scale":
                        AssignValueRange(childnode, ref leaf.scale);
                        break;

                    case "position":
                        AssignValueRange(childnode, ref leaf.position);
                        break;

                    case "roll":
                        AssignValueRange(childnode, ref leaf.roll);
                        break;

                    case "axis":
                        leaf.hasAxis = AssignVec3(childnode, ref leaf.axis); // Set hasAxis to true only if AssignVec3 is succesful.
                        break;

                    case "anchor":
                        AssignValueRange(childnode, ref leaf.anchor);
                        break;

                    case "count":
                        AssignValueRange(childnode, ref leaf.count);
                        break;

                    case "red":
                        AssignValueRange(childnode, ref leaf.red);
                        break;

                    case "green":
                        AssignValueRange(childnode, ref leaf.green);
                        break;

                    case "blue":
                        AssignValueRange(childnode, ref leaf.blue);
                        break;

                    case "alpha":
                        AssignValueRange(childnode, ref leaf.alpha);
                        break;
                }
            }

            // Add the leaf to the list.
            leaves.Add(leaf);

            return true;
        }

        #endregion

        #region Tree Generation
        
        /// <summary>
        /// Generates a new tree using the loaded tree profile. This method is <i>not</i> thread-safe.
        /// </summary>
        /// <param name="seed">Random seed to generate the tree. Using the same seed twice will generate the same tree twice.</param>
        /// <param name="radialSegments">Number of radials segments at the tree's trunk. Typical values are between 8 and 12.</param>
        /// <param name="addLeaves">If true, leaf positions will be calculated and added to the tree mesh. Otherwise, leaves are ignored.</param>
        /// <param name="cutoffLevel">Branches lower than this level are abruptly removed from the mesh, but otherwise the tree is the same. Typically you want this to be 0, but for LOD you can increase it.</param>
        /// <returns>A TreeModel object containing the trunk's mesh, the leaves, and the bounding box. Null is returned if no tree profile is loaded.</returns>
        public TreeModel GenerateTreeMesh(int seed, int radialSegments, bool addLeaves, int cutoffLevel)
        {
            if (branches.Count == 0)
                return null;

            cloud = new ParticleCloud(cloudSystem);
            treeVertices.Clear();
            treeIndices.Clear();
            boundingBox.Min = boundingBox.Max = Vector3.Zero;

            this.seed = seed;
            leafSeed = seed;
            this.radialSegments = radialSegments;
            this.cutoffLevel = cutoffLevel;
            this.addLeaves = addLeaves;

            int branchId = GetBranchIndexFromId(rootIdRef);

            if (branchId != -1)
            {
                AppendBranch(Matrix.Identity, branchId, 1f, 1f, levels);
            }

            // Convert the result to a tree mesh
            TreeModel mesh = new TreeModel();
            mesh.Trunk = new Mesh(graphicsDevice, treeVertices.ToArray(), treeIndices.ToArray());
            mesh.Trunk.Effect = trunkEffect;
            mesh.BoundingBox = boundingBox;
            mesh.Leaves = cloud;
            mesh.Leaves.Texture = leafTexture;
            cloud = null;

            return mesh;
        }

        private static float GetRandomFloat( int seed, float min, float max )
        {
            seed = (seed<<13) ^ seed;
            float x = ( 1.0f - ( (seed * (seed * seed * 15731 + 789221) + 1376312589) & 0x7fffffff) / 1073741824.0f);
            return (max-min)*(x+1)/2.0f + min;
        }

        private static int GetRandomInt( int seed, int min, int max )
        {
            return (int)(GetRandomFloat(seed,min,max) + 0.50f);
        }

        private static float GetFloatFromValueRange(ValueRange<float> range, int seed)
        {
            if (range.isFixedValue)
                return range.min;
            return GetRandomFloat(seed, range.min, range.max);
        }

        private static int GetIntFromValueRange(ValueRange<int> range, int seed)
        {
            if (range.isFixedValue)
                return range.min;
            return GetRandomInt(seed, range.min, range.max);
        }

        private static bool IsValueInRange(ValueRange<int> range, int value)
        {
            return (range.min <= value && range.max >= value);
        }

        private void AppendBranch(Matrix transform, int parentId, float lengthScale, float radiusScale, int level)
        {
            if ( level <= 0 )
                return; // Do nothing, this branch is invisible.

            Branch branch = branches[parentId];
            
            // Add this branch
            float length = lengthScale * GetFloatFromValueRange( branch.length, seed++ );
            
            float radius = GetFloatFromValueRange( branch.radius, seed++ );
            
            float radiusEndScale = radiusScale * GetFloatFromValueRange( branch.radiusEnd, seed++ );
            
            float radiusEnd = radius * radiusEndScale;
            
            radius = radiusScale * radius;
            
            if ( level == 1 )
                radiusEnd = 0.0f;
            
            int radialSegments = (level*(this.radialSegments-3))/levels + 3;
            
            if ( level > cutoffLevel )
            {
                MeshUtil.BuildCylinder(
                    treeVertices,
                    treeIndices,
                    ref boundingBox,
                    
                    length,
                    radius,
                    radiusEnd,
                    radialSegments,
                    
                    transform,
                    0.0f,
                    1.0f);
            }
            
            // Add children
            if ( level > 1 )
            {
                for (int childN = 0; childN < branch.children.Count; childN++)
                {
                    BranchChild child = branch.children[childN];
                    
                    int childBranchId = GetBranchIndexFromId(child.idRef);

                    if ( childBranchId == -1 )
                        continue;
                    
                    Branch childBranch = branches[childBranchId];
                    
                    if ( !IsValueInRange( child.levelRange, level ) )
                        continue;
                    
                    int childLevel = level + GetIntFromValueRange( child.relativeLevel, seed++ ); // RelativeLevel is usually negative, -1 in particular.
                    
                    int numChildren = GetIntFromValueRange( child.count, seed++ );
                    
                    float positionRange = child.position.max - child.position.min;
                    
                    positionRange /= (float)numChildren;
                    
                    float orientation = GetRandomFloat( seed++, 0, 360.0f );
                    
                    for ( int i=0; i<numChildren; i++ )
                    {
                        float childLengthScale = lengthScale * GetFloatFromValueRange( child.lengthScale, seed++ );
                        
                        orientation += GetFloatFromValueRange( child.orientation, seed++ );
                        
                        // Clamp value between 0 and 360.
                        if ( orientation < 0.0f )
                            orientation += 360.0f;
                        else
                        if ( orientation > 360.0f )
                            orientation -= 360.0f;
                        
                        float childOrientation = orientation;
                        
                        float gravity = GetFloatFromValueRange( child.gravityInfluence, seed++ );
                        
                        float childPitch = GetFloatFromValueRange( child.pitch, seed++ );

                        float childPosition = GetFloatFromValueRange(child.position, seed++);
                        
                        float position;
                        
                        if ( child.position.isFixedValue )
                            position = child.position.min;
                        else
                            position = (child.position.min + positionRange * i + positionRange * GetRandomFloat( seed++, 0f, 1f ));
                        
                        float childRadiusScale = (radiusScale*(1.0f-position) + radiusEndScale*position) * GetFloatFromValueRange( child.radiusScale, seed++ );
                        
                        // Build transformation matrix
                        Matrix mat =
                              Matrix.CreateRotationX(MathHelper.ToRadians(childPitch))
                            * Matrix.CreateRotationY(MathHelper.ToRadians(childOrientation));
                        
                        // Set the Y translation
                        mat.M42 = length * position;
                        
                        // Transform by the branch transformation
                        mat = mat * transform;
                        
                        if ( gravity != 0.0f )
                        {
                            // Do some extra work
                            
                            // Get a vector pointing downwards (towards gravity's pull)
                            Vector3 vDown = -Vector3.UnitY;
                            
                            // Get a vector pointing in the branch's direction, by rotating the Y unit vector.
                            Vector3 vBranch = mat.Up;
                            
                            Vector3 vSide;
                            
                            if (Math.Abs(vBranch.Y) >= 0.9f)
                            {
                                // The X unit vector should suffice as side vector if rotated by the matrix.
                                vSide = mat.Right; //Vector3.TransformNormal(Vector3.UnitX, mat);
                            }
                            else
                            {
                                // Use the cross product to find a suitable sideways vector
                                vSide = Vector3.Cross(vBranch, vDown);
                                vSide.Normalize();
                            }
                            
                            vDown = Vector3.Cross(vSide, vBranch);
                            vDown.Normalize();
                            
                            mat.Right = vSide;
                            mat.Backward = vDown;
                            
                            float dot = -vBranch.Y;
                            
                            if ( gravity < 0.0f )
                                dot = -dot;
                            
                            float angle = (float)Math.Acos(dot);
                            
                            angle *= gravity;
                            
                            // Bend the branch around the X-axis.
                            Matrix mat2 = Matrix.CreateRotationX(angle);
                            
                            mat = mat2 * mat;
                        }
                        
                        // Add the branch
                        AppendBranch(mat, childBranchId, childLengthScale, childRadiusScale, childLevel);
                    }
                }
            }
            
            // Add leaves
            if ( addLeaves )
            {
                for (int leafN = 0; leafN < branch.leaves.Count; leafN++)
                {
                    Leaf leaf = branch.leaves[leafN];
                    
                    if ( !IsValueInRange( leaf.levelRange, level ) )
                        continue;

                    int count = GetIntFromValueRange(leaf.count, leafSeed++);
                    
                    for (int i = 0; i < count; i++)
                    {
                        float width = GetFloatFromValueRange(leaf.width, leafSeed++);
                        float height = GetFloatFromValueRange(leaf.height, leafSeed++);
                        float scale = GetFloatFromValueRange(leaf.scale, leafSeed++);
                        float position = GetFloatFromValueRange(leaf.position, leafSeed++);
                        float roll = GetFloatFromValueRange(leaf.roll, leafSeed++);
                        float anchor = GetFloatFromValueRange(leaf.anchor, leafSeed++);
                        
                        Matrix mat = Matrix.Identity;
                        
                        mat.M42 = length * position;
                        
                        mat = mat * transform;
                        
                        Vector3 leafPos = mat.Translation;

                        Color color = new Color(
                            (byte)GetIntFromValueRange(leaf.red, leafSeed++),
                            (byte)GetIntFromValueRange(leaf.green, leafSeed++),
                            (byte)GetIntFromValueRange(leaf.blue, leafSeed++),
                            (byte)GetIntFromValueRange(leaf.alpha, leafSeed++));

                        //Console.WriteLine("Color = " + color.R + ", " + color.G + ", " + color.B + ", " + color.A);
                        
                        if ( leaf.hasAxis )
                        {
                            cloud.AxisEnabled = true;
                            cloud.Axis = leaf.axis;
                            leafPos += leaf.axis * height * scale * anchor / 2.0f;
                        }

                        cloud.AddParticle(leafPos, color, roll, new Vector2(width, height));
                    }
                }
            }
        }

        private int GetBranchIndexFromId(int id)
        {
            for (int i = 0; i < branches.Count; i++)
            {
                if (branches[i].id == id)
                    return i;
            }
            return -1;
        }

        #endregion

        /// <summary>
        /// Prints the number of branches, children, and leaves in the current tree profile.
        /// Useful when debugging, to quickly check if the XML file was read correctly.
        /// </summary>
        public void PrintDebugInfo()
        {
            Console.WriteLine("Number of branches = " + branches.Count);
            for (int i = 0; i < branches.Count; i++)
            {
                Console.WriteLine("Branch #" + branches[i].id + ":");
                Console.WriteLine("\tNumber of leaves = " + branches[i].leaves.Count);
                Console.WriteLine("\tNumber of children = " + branches[i].children.Count);
            }
        }
    }
}
