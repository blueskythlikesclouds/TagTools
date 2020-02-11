using System.Xml;
using Havoc.Objects;

namespace Havoc.IO.Tagfile.Xml
{
    public interface IHkXmlTagfileWriter
    {
        void Write( XmlWriter writer, IHkObject rootObject );
    }
}