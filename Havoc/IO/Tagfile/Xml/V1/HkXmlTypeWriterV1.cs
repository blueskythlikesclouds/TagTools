using System;
using System.Xml;
using Havoc.Reflection;

namespace Havoc.IO.Tagfile.Xml.V1
{
    public class HkXmlTypeWriterV1 : IHkXmlTypeWriter
    {
        public HkXmlTypeWriterV1( HkTypeCompendium typeCompendium )
        {
            TypeCompendium = typeCompendium;
        }

        public HkTypeCompendium TypeCompendium { get; }

        public void WriteType( XmlWriter writer, HkType type )
        {
            // TODO
        }

        public void WriteAllTypes( XmlWriter writer )
        {
            // TODO
        }

        public string GetTypeIdString( HkType type )
        {
            // Return just name (for now)
            return type.Name;
        }

        public static string GetFormatName( HkType type )
        {
            // ew
            switch ( type.Format )
            {
                case HkTypeFormat.Void:
                    return "void";
                case HkTypeFormat.Opaque:
                    return "incomplete";
                case HkTypeFormat.Bool:
                case HkTypeFormat.Int:
                    return type.BitCount == 8 ? "byte" : "int";
                case HkTypeFormat.String:
                    return "string";
                case HkTypeFormat.FloatingPoint:
                    return "real";
                case HkTypeFormat.Ptr:
                    return "ref";
                case HkTypeFormat.Class:
                    return type.Name.StartsWith( "hkQsTransform" ) ? "vec12" : "struct";
                case HkTypeFormat.Array:
                    if ( !type.IsFixedSize )
                        return "array";

                    if ( type.SubType.Format != HkTypeFormat.FloatingPoint )
                        return "tuple";

                    return type.FixedSize switch
                    {
                        4 => "vec4",
                        12 => "vec12",
                        16 => "vec16",
                        _ => "tuple"
                    };

                default:
                    throw new ArgumentOutOfRangeException( nameof( type.Format ) );
            }
        }
    }
}