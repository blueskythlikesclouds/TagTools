using System.Xml;

namespace Havoc.Extensions
{
    public static class XmlWriterEx
    {
        public static void WriteStartElement( this XmlWriter writer, string name, params (string, object)[] attributes )
        {
            writer.WriteStartElement( name );
            foreach ( ( string attributeName, var attributeValue ) in attributes )
                writer.WriteAttributeString( attributeName, attributeValue.ToString() );
        }

        public static void WriteStartElement( this XmlWriter writer, string name, object comment,
            params (string, object)[] attributes )
        {
            writer.WriteComment( $"{comment}" );
            writer.WriteStartElement( name, attributes );
        }

        public static void WriteElement( this XmlWriter writer, string name, params (string, object)[] attributes )
        {
            writer.WriteStartElement( name, attributes );
            writer.WriteEndElement();
        }

        public static void WriteElement( this XmlWriter writer, string name, object comment,
            params (string, object)[] attributes )
        {
            writer.WriteComment( $"{comment}" );
            writer.WriteElement( name, attributes );
        }
    }
}