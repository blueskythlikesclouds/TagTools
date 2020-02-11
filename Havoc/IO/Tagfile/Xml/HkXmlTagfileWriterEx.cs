using System.Xml;
using Havoc.Objects;

namespace Havoc.IO.Tagfile.Xml
{
    public static class HkXmlTagfileWriterEx
    {
        public static void Write( this IHkXmlTagfileWriter tagWriter, string destinationFilePath, IHkObject rootObject )
        {
            using ( var writer = XmlWriter.Create( destinationFilePath,
                new XmlWriterSettings { Indent = true, IndentChars = "  " } ) )
            {
                tagWriter.Write( writer, rootObject );
            }
        }
    }
}