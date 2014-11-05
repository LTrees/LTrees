/*
 * Copyright (c) 2007-2009 Asger Feldthaus
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using LTreesLibrary.Trees;
using System.ComponentModel;
using System.Xml;

namespace LTreesLibrary.Pipeline
{
    [ContentProcessor(DisplayName = "LTree Profile")]
    public class TreeProfileProcessor : ContentProcessor<XmlDocument, TreeProfileContent>
    {
        [DisplayName("Texture Path")]
        [DefaultValue("")]
        [Description("Prefix file's texture asset names.")]
        public String TexturePath { get; set; }

        [DisplayName("Trunk Effect")]
        [DefaultValue("")]
        [Description("Asset name of the effect used to render the branches. If left blank, the reference shader is used.")]
        public String TrunkEffect { get; set; }

        [DisplayName("Leaf Effect")]
        [DefaultValue("")]
        [Description("Asset name of the effect used to render the leaves. If left blank, the reference shader is used.")]
        public String LeafEffect { get; set; }

        private void ErrorInvalidFormat(String message)
        {
            throw new PipelineException("Invalid LTree specification. " + message);
        }

        private String GetChildContent(XmlNode node, String childName)
        {
            XmlNode child = node.SelectSingleNode(childName);
            if (child == null)
                ErrorInvalidFormat("Missing " + childName + " node.");
            return child.InnerText;
        }

        public override TreeProfileContent Process(XmlDocument input, ContentProcessorContext context)
        {
            // Build a tree generator just to validate the XML format
            try
            {
                TreeGenerator.CreateFromXml(input);
            }
            catch (ArgumentException ex)
            {
                ErrorInvalidFormat(ex.Message);
            }

            TreeProfileContent content = new TreeProfileContent();

            string path = "";
            if (TexturePath != null && TexturePath != "" && !(TexturePath.EndsWith("/") || TexturePath.EndsWith(@"\")))
                path = TexturePath + "/";
            else if (TexturePath == null)
                path = "";
            else
                path = TexturePath;

            XmlNode treeNode = input.SelectSingleNode("Tree");

            content.GeneratorXML = input;
            content.TrunkTexture = path + GetChildContent(treeNode, "TrunkTexture");
            content.LeafTexture = path + GetChildContent(treeNode, "LeafTexture");
            content.TrunkEffect = TrunkEffect;
            content.LeafEffect = LeafEffect;
            
            return content;
        }
    }
}
