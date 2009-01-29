/*
 * Copyright (c) 2007-2009 Asger Feldthaus
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using LTreesLibrary.Trees.Instructions;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Randomly generates tree skeletons using an L-system. It is recommended to load this from an XML file using <see cref="CreateFromXML"/>.
    /// </summary>
    /// <remarks>
    /// This class only produces tree skeletons, which by themselves cannot be rendered. Use the <see cref="TreeMesh"/> and <see cref="TreeLeafCloud"/>
    /// classes to generate meshes and particles that can be rendered. The <see cref="SimpleTree"/> class can do this for you that can be used in 
    /// most simple applications.
    /// </remarks>
    public class TreeGenerator
    {
        private Production root;
        private int maxLevel;
        private CompositeTreeConstraints constraints = new CompositeTreeConstraints();
        private float textureHeight = 512.0f;
        private float textureHeightVariation = 0.0f;

        /// <summary>
        /// Variation of the texture height. The range of possible values are <code>TextureHeight - var</code> to <code>TextureHeight + var</code>.
        /// </summary>
        public float TextureHeightVariation
        {
            get { return textureHeightVariation; }
            set { textureHeightVariation = value; }
        }

        /// <summary>
        /// Height of the texture before it is repeate on the Y-axis.
        /// </summary>
        public float TextureHeight
        {
            get { return textureHeight; }
            set { textureHeight = value; }
        }

        /// <summary>
        /// Used to stop nested production calls to run infinitely long. A typical value is 5.
        /// </summary>
        public int MaxLevel
        {
            get { return maxLevel; }
            set { maxLevel = value; }
        }
        
        /// <summary>
        /// The leaf axis to use for generated trees.
        /// If null, the leaves are free billboards. Otherwise, they are axis-aligned
        /// around this axis. Must be a normalized vector or null.
        /// </summary>
        public Vector3? LeafAxis { get; set; }
        
        /// <summary>
        /// How deep into the branches the bones should be allowed to grow. A typical value is 3.
        /// </summary>
        public int BoneLevels { get; set; }

        /// <summary>
        /// The top-most production used in the L-system generating the tree.
        /// </summary>
        public Production Root
        {
            get { return root; }
            set { root = value; }
        }

        /// <summary>
        /// The constraints imposed on the generated tree. An overload of GenerateTree
        /// will let you provide user-defined constraints.
        /// </summary>
        public CompositeTreeConstraints Constraints
        {
            get { return constraints; }
        }

        /// <summary>
        /// Generates the structure of a tree, but no vertex data.
        /// </summary>
        /// <param name="rnd">Object providing random numbers.</param>
        /// <param name="userConstraint">Object imposing user-defined constraints on the generated tree. See <see cref="TreeConstraints"/>.</param>
        /// <returns>A new tree skeleton.</returns>
        /// <seealso cref="TreeSkeleton"/>
        /// <seealso cref="TreeMesh"/>
        /// <seealso cref="TreeLeafCloud"/>
        /// <seealso cref="SimpleTree"/>
        public TreeSkeleton GenerateTree(Random rnd, TreeContraints userConstraint)
        {
            if (root == null || maxLevel == 0)
                throw new InvalidOperationException("TreeGenerator has not been initialized. Must set Root and MaxLevel before generating a tree.");

            TreeCrayon crayon = new TreeCrayon();
            crayon.Level = maxLevel;
            crayon.BoneLevels = BoneLevels;
            crayon.Constraints = constraints;
            constraints.UserConstraint = userConstraint;
            crayon.Skeleton.LeafAxis = LeafAxis;

            crayon.Skeleton.Bones.Add(new TreeBone(Quaternion.Identity, -1, Matrix.Identity, Matrix.Identity, 1, 1, -1));
            root.Execute(crayon, rnd);

            crayon.Skeleton.CloseEdgeBranches();

            crayon.Skeleton.TextureHeight = textureHeight + textureHeightVariation * (2.0f * (float)rnd.NextDouble() - 1.0f);

            return crayon.Skeleton;
        }

        /// <summary>
        /// Generates the structure of a tree, but no vertex data.
        /// </summary>
        /// <param name="rnd">Object providing random numbers.</param>
        /// <returns>A new tree skeleton.</returns>
        /// <seealso cref="TreeSkeleton"/>
        /// <seealso cref="TreeMesh"/>
        /// <seealso cref="TreeLeafCloud"/>
        /// <seealso cref="SimpleTree"/>
        public TreeSkeleton GenerateTree(Random rnd)
        {
            return GenerateTree(rnd, null);
        }


        #region XML
        /// <summary>
        /// Contains a production and the node that defined it.
        /// </summary>
        private struct ProductionNodePair
        {
            public Production Production;
            public XmlNode Node;

            public ProductionNodePair(Production p, XmlNode n)
            {
                Production = p;
                Node = n;
            }
        }

        /// <summary>
        /// Creates a tree generator from a specified XML document, loaded using <see cref="XmlDocument.Load"/>.
        /// </summary>
        /// <param name="filename">Filename of XML file to load (including path). Hint: Remember to include the <code>Content</code> folder in the path.</param>
        /// <returns>A new tree generator.</returns>
        /// <exception cref="IOException">If an IO error occurred while trying to load the file. See <see cref="XmlDocument.Load"/>.</exception>
        /// <exception cref="ArgumentException">If the XML document is wellformed but not a valid tree specification.</exception>
        public static TreeGenerator CreateFromXml(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            return CreateFromXml(doc);
        }

        /// <summary>
        /// Creates a tree generator from a specified XML document.
        /// </summary>
        /// <param name="document">The XML document to parse.</param>
        /// <returns>A new tree generator.</returns>
        /// <exception cref="ArgumentException">If the XML document is not a valid tree specification.</exception>
        public static TreeGenerator CreateFromXml(XmlDocument document)
        {
            TreeGenerator generator = new TreeGenerator();
            string rootName = null;
            int levels = -1;
            int boneLevels = 3;
            MultiMap<string, ProductionNodePair> productions = new MultiMap<string, ProductionNodePair>();

            XmlNode root = document.SelectSingleNode("Tree");

            foreach (XmlNode child in root.ChildNodes)
            {
                switch (child.Name)
                {
                    case "Root":
                        rootName = XmlUtil.GetString(child, "ref");
                        break;

                    case "Levels":
                        levels = XmlUtil.GetInt(child, "value");
                        break;

                    case "BoneLevels":
                        boneLevels = XmlUtil.GetInt(child, "value");
                        break;

                    case "LeafAxis":
                        generator.LeafAxis = XmlUtil.GetVector3(child, "value");
                        generator.LeafAxis.Value.Normalize();
                        break;

                    case "Production":
                        string name = XmlUtil.GetString(child, "id");
                        productions.Add(name, new ProductionNodePair(new Production(), child));
                        break;

                    case "ConstrainUnderground":
                        generator.Constraints.Constaints.Add(new ConstrainUndergroundBranches(XmlUtil.GetFloat(child, "lowerBound", 256.0f)));
                        break;

                    case "TextureHeight":
                        generator.TextureHeight = XmlUtil.GetFloat(child, "height");
                        generator.TextureHeightVariation = XmlUtil.GetFloat(child, "variation", 0.0f);
                        break;
                }
            }

            if (rootName == null)
                throw new ArgumentException("Root name must be specified.");

            // Now we have a map of names -> productions, so we can start parsing the the productions
            foreach (ProductionNodePair pn in productions.Values)
            {
                ParseInstructionsFromXml(pn.Node, pn.Production.Instructions, productions);
            }

            generator.Root = productions[rootName][0].Production;
            generator.MaxLevel = levels;
            generator.BoneLevels = boneLevels;
            
            return generator;
        }

        /// <summary>
        /// Returns the list of productions whose id matches the specified name. Several named may be OR'ed together
        /// with pipes (|) to match more than one id. Note that several productions may also have the same id.
        /// </summary>
        private static List<Production> GetProductionsByRef(String name, MultiMap<string, ProductionNodePair> map)
        {
            String[] names = name.Split('|');
            List<Production> list = new List<Production>();
            foreach (String n in names)
            {
                if (!map.ContainsKey(n))
                    throw new ArgumentException(String.Format("No production exists with the name '{0}'", n));
                List<ProductionNodePair> np = map[n];
                foreach (ProductionNodePair pair in np)
                {
                    list.Add(pair.Production);
                }
            }
            return list;
        }

        /// <summary>
        /// Parses each child of <code>node</code> as an instruction, adds them to the specified instruction list. Calls are resolved to production lists
        /// using the specified multimap.
        /// </summary>
        private static void ParseInstructionsFromXml(XmlNode node, List<TreeCrayonInstruction> instructions, MultiMap<string, ProductionNodePair> map)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.Name)
                {
                    case "Call":
                        String name = XmlUtil.GetString(child, "ref");
                        List<Production> productions = GetProductionsByRef(name, map);
                        instructions.Add(new Call(productions, XmlUtil.GetInt(child, "delta", -1)));
                        break;

                    case "Child":
                        Child ch = new Child();
                        ParseInstructionsFromXml(child, ch.Instructions, map);
                        instructions.Add(ch);
                        break;

                    case "Maybe":
                        Maybe maybe = new Maybe(XmlUtil.GetFloat(child, "chance", 0.50f));
                        ParseInstructionsFromXml(child, maybe.Instructions, map);
                        instructions.Add(maybe);
                        break;

                    case "Forward":
                        instructions.Add(new Forward(XmlUtil.GetFloat(child, "distance"), XmlUtil.GetFloat(child, "variation", 0.0f), XmlUtil.GetFloat(child, "radius", 0.86f)));
                        break;

                    case "Backward":
                        instructions.Add(new Backward(XmlUtil.GetFloat(child, "distance"), XmlUtil.GetFloat(child, "variation", 0.0f)));
                        break;

                    case "Pitch":
                        instructions.Add(new Pitch(XmlUtil.GetFloat(child, "angle"), XmlUtil.GetFloat(child, "variation", 0.0f)));
                        break;

                    case "Scale":
                        instructions.Add(new Scale(XmlUtil.GetFloat(child, "scale"), XmlUtil.GetFloat(child, "variation", 0.0f)));
                        break;

                    case "ScaleRadius":
                        instructions.Add(new ScaleRadius(XmlUtil.GetFloat(child, "scale"), XmlUtil.GetFloat(child, "variation", 0.0f)));
                        break;

                    case "Twist":
                        instructions.Add(new Twist(XmlUtil.GetFloat(child, "angle", 0), XmlUtil.GetFloat(child, "variation", 360.0f)));
                        break;

                    case "Level":
                        instructions.Add(new Level(XmlUtil.GetInt(child, "delta", -1)));
                        break;

                    case "Leaf":
                        instructions.Add(ParseLeafFromXml(child));
                        break;

                    case "Bone":
                        instructions.Add(new Bone(XmlUtil.GetInt(child, "delta", -1)));
                        break;

                    case "RequireLevel":
                        String type = XmlUtil.GetStringOrNull(child, "type");
                        CompareType ctype = type == "less" ? CompareType.Less : CompareType.Greater;
                        RequireLevel req = new RequireLevel(XmlUtil.GetInt(child, "level"), ctype);
                        ParseInstructionsFromXml(child, req.Instructions, map);
                        instructions.Add(req);
                        break;

                    case "Align":
                        instructions.Add(new Align());
                        break;
                }
            }
        }

        /// <summary>
        /// Parses the children of a Leaf node, and returns a corresponding <see cref="Leaf"/> object.
        /// </summary>
        private static Leaf ParseLeafFromXml(XmlNode node)
        {
            Leaf leaf = new Leaf();
            foreach (XmlNode child in node.ChildNodes)
            {
                switch (child.Name)
                {
                    case "Color":
                        leaf.Color = XmlUtil.GetColor(child, "value").ToVector4();
                        leaf.ColorVariation = XmlUtil.GetColor(child, "variation", new Color(0, 0, 0, 0)).ToVector4();
                        break;

                    case "Size":
                        leaf.Size = XmlUtil.GetVector2(child, "value");
                        leaf.SizeVariation = XmlUtil.GetVector2(child, "variation", new Vector2(0, 0));
                        break;

                    case "AxisOffset":
                        leaf.AxisOffset = XmlUtil.GetFloat(child, "value");
                        break;
                }
            }
            return leaf;
        }
        #endregion


    }
}
