using System;
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
