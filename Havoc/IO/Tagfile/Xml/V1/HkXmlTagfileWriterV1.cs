using System;
using System.Xml;
using Havoc.Extensions;
using Havoc.Objects;

namespace Havoc.IO.Tagfile.Xml.V1
{
    public class HkXmlTagfileWriterV1 : IHkXmlTagfileWriter
    {
        private static HkXmlTagfileWriterV1 sInstance;

        public static HkXmlTagfileWriterV1 Instance => sInstance ??= new HkXmlTagfileWriterV1();

        public void Write( XmlWriter writer, IHkObject rootObject )
        {
            var objectWriter = new HkXmlObjectWriterV1( rootObject );

            writer.WriteStartDocument();
            writer.WriteStartElement( "hktagfile", $"Exported from Havoc at {DateTime.Now}", ( "version", 1 ) );
            {
                objectWriter.TypeWriter.WriteAllTypes( writer );
                objectWriter.WriteAllObjects( writer );
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
    }
}