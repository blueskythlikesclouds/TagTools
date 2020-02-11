using System;
using System.Xml;
using Havoc.Extensions;

namespace Havoc.IO.Tagfile.Xml
{
    public static class HkXmlTypeWriterEx
    {
        public static void WriteTypeCompendium( this IHkXmlTypeWriter typeWriter, XmlWriter writer )
        {
            writer.WriteStartDocument();
            writer.WriteStartElement( "hktypecompendium", $"Exported from Havoc at {DateTime.Now}", ( "version", 3 ) );
            {
                typeWriter.WriteAllTypes( writer );
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        public static void WriteTypeCompendium( this IHkXmlTypeWriter typeWriter, string destinationFilePath )
        {
            using ( var writer = XmlWriter.Create( destinationFilePath,
                new XmlWriterSettings { Indent = true, IndentChars = "  " } ) )
            {
                typeWriter.WriteTypeCompendium( writer );
            }
        }
    }
}