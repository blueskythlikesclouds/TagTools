using Havoc.IO.Tagfile.Binary;
using Havoc.IO.Tagfile.Xml;
using Havoc.IO.Tagfile.Xml.V3;

namespace Havoc.Cli
{
    internal class Program
    {
        private static void Main( string[] args )
        {
            var obj = HkBinaryTagfileReader.Read( args[ 0 ] );
            HkBinaryTagfileWriter.Write( args[ 0 ] + ".out", obj, HkSdkVersion.V20160100 );
            HkXmlTagfileWriterV3.Instance.Write( args[ 0 ] + ".xml", obj );
        }
    }
}