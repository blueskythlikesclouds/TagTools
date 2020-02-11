using System.Collections.Generic;
using System.Xml;
using Havoc.Objects;

namespace Havoc.IO.Tagfile.Xml
{
    public interface IHkXmlObjectWriter
    {
        IHkXmlTypeWriter TypeWriter { get; }

        IHkObject RootObject { get; }
        IReadOnlyList<IHkObject> Objects { get; }

        void WriteObject( XmlWriter writer, IHkObject obj, bool writeObjectDefinition = false );
        void WriteAllObjects( XmlWriter writer );

        string GetObjectIdString( IHkObject obj );
    }
}