using System;
using System.Xml;
using Havoc.Extensions;
using Havoc.Objects;

namespace Havoc.IO.Tagfile.Xml.V3
{
    public class HkXmlTagfileWriterV3 : IHkXmlTagfileWriter
    {
        private static HkXmlTagfileWriterV3 sInstance;

        public static HkXmlTagfileWriterV3 Instance => sInstance ??= new HkXmlTagfileWriterV3();

        public void Write( XmlWriter writer, IHkObject rootObject )
        {
            var objectWriter = new HkXmlObjectWriterV3( rootObject );

            writer.WriteStartDocument();
            writer.WriteStartElement( "hktagfile", $"Exported from Havoc at {DateTime.Now}", ( "version", 3 ) );
            {
                objectWriter.TypeWriter.WriteAllTypes( writer );
                objectWriter.WriteAllObjects( writer );
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
    }
}