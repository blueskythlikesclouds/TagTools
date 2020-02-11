using System.Xml;
using Havoc.Reflection;

namespace Havoc.IO.Tagfile.Xml
{
    public interface IHkXmlTypeWriter
    {
        HkTypeCompendium TypeCompendium { get; }

        void WriteType( XmlWriter writer, HkType type );
        void WriteAllTypes( XmlWriter writer );

        string GetTypeIdString( HkType type );
    }
}