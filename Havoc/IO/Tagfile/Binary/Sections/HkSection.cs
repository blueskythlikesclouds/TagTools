using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using Havoc.Extensions;

namespace Havoc.IO.Tagfile.Binary.Sections
{
    public class HkSection
    {
        public HkSection( BinaryReader reader )
        {
            uint flagsAndSize = BinaryPrimitives.ReverseEndianness( reader.ReadUInt32() );

            Signature = reader.ReadString( 4 );
            Position = reader.BaseStream.Position;
            Length = ( flagsAndSize & 0x3FFFFFFF ) - 8;

            if ( ( flagsAndSize & 0xC0000000 ) == 0 )
            {
                SubSections = new List<HkSection>();
                while ( reader.BaseStream.Position < Position + Length )
                    SubSections.Add( new HkSection( reader ) );
            }
            else
            {
                reader.BaseStream.Seek( Position + Length, SeekOrigin.Begin );
            }
        }

        public string Signature { get; }
        public long Position { get; }
        public long Length { get; }
        public List<HkSection> SubSections { get; }
    }
}