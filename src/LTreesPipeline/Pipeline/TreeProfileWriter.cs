using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Xml;
using System.Text;

namespace LTreesLibrary.Pipeline
{
    [ContentTypeWriter]
    public class TreeProfileWriter : ContentTypeWriter<TreeProfileContent>
    {
        protected override void Write(ContentWriter output, TreeProfileContent value)
        {
            StringBuilder xmlSource = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(xmlSource);
            value.GeneratorXML.Save(writer);

            //output.Write(xmlSource.ToString());
            output.Write(xmlSource.ToString());
            output.Write(value.TrunkTexture);
            output.Write(value.LeafTexture);
            output.Write(value.TrunkEffect == null ? "" : value.TrunkEffect);
            output.Write(value.LeafEffect == null ? "" : value.LeafEffect);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(TreeProfileReader).AssemblyQualifiedName;
        }
    }

}
