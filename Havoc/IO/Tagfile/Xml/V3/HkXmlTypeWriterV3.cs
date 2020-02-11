using System.Xml;
using Havoc.Extensions;
using Havoc.Reflection;

namespace Havoc.IO.Tagfile.Xml.V3
{
    public class HkXmlTypeWriterV3 : IHkXmlTypeWriter
    {
        public HkXmlTypeWriterV3( HkTypeCompendium typeCompendium )
        {
            TypeCompendium = typeCompendium;
        }

        public HkTypeCompendium TypeCompendium { get; }

        public void WriteType( XmlWriter writer, HkType type )
        {
            writer.WriteStartElement( "type", ( "typeid", GetTypeIdString( type ) ) );
            {
                writer.WriteElement( "name", ( "value", type.Name ) );
                if ( type.ParentType != null )
                    writer.WriteElement( "parent", type.ParentType, ( "id", GetTypeIdString( type.ParentType ) ) );

                if ( ( type.Flags & HkTypeFlags.HasFormatInfo ) != 0 )
                    writer.WriteElement( "format", ( "value", type.mFormatInfo ) );

                if ( ( type.Flags & HkTypeFlags.HasSubType ) != 0 )
                    writer.WriteElement( "subtype", type.mSubType, ( "id", GetTypeIdString( type.mSubType ) ) );

                if ( ( type.Flags & HkTypeFlags.HasVersion ) != 0 )
                    writer.WriteElement( "version", ( "value", type.mVersion ) );

                if ( type.mParameters.Count != 0 )
                {
                    writer.WriteStartElement( "parameters", ( "count", type.mParameters.Count ) );
                    foreach ( var parameter in type.mParameters )
                        if ( parameter.IsType )
                            writer.WriteElement( "typeparam", parameter.TypeValue,
                                ( "id", GetTypeIdString( parameter.TypeValue ) ) );
                        else
                            writer.WriteElement( "valueparam", ( "value", parameter.IntValue ) );

                    writer.WriteEndElement();
                }

                if ( ( type.Flags & HkTypeFlags.HasUnknownFlags ) != 0 )
                    writer.WriteElement( "flags", ( "value", type.mUnknownFlags ) );

                if ( ( type.Flags & HkTypeFlags.HasFields ) != 0 )
                {
                    writer.WriteStartElement( "fields", ( "count", type.mFields.Count ) );

                    foreach ( var field in type.mFields )
                        writer.WriteElement( "field", field.Type, ( "name", field.Name ),
                            ( "typeid", GetTypeIdString( field.Type ) ), ( "flags", ( int ) field.Flags ) );

                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        public void WriteAllTypes( XmlWriter writer )
        {
            foreach ( var type in TypeCompendium )
                WriteType( writer, type );
        }

        public string GetTypeIdString( HkType type )
        {
            return
                $"type{( type != null && TypeCompendium.IndexMap.TryGetValue( type, out int index ) ? index + 1 : 0 )}";
        }
    }
}