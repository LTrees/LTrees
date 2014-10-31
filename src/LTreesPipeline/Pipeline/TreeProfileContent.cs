using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LTreesLibrary.Trees;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.Xml;

namespace LTreesLibrary.Pipeline
{
    public class TreeProfileContent
    {
        public XmlDocument GeneratorXML { get; set; }
        public String TrunkTexture { get; set; }
        public String LeafTexture { get; set; }
        public String TrunkEffect { get; set; }
        public String LeafEffect { get; set; }
    }
}
