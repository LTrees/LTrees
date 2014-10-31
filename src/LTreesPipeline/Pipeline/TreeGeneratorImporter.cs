using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using LTreesLibrary.Trees;
using System.IO;
using System.Xml;

namespace LTreesLibrary.Pipeline
{
    /// <summary>
    /// Imports XML tree specifications.
    /// </summary>
    /// <remarks>
    /// It really just imports XML files as an XmlDocument.
    /// </remarks>
    [ContentImporter(".ltree", DisplayName = "LTree Specification", DefaultProcessor = "TreeProfileProcessor")]
    public class TreeGeneratorImporter : ContentImporter<XmlDocument>
    {
        public override XmlDocument Import(string filename, ContentImporterContext context)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            return doc;
        }
    }
}
