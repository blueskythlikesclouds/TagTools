using System.IO;
using System.Text;

namespace Havoc.Extensions
{
    public static class BinaryReaderEx
    {
        public static string ReadNullTerminatedString( this BinaryReader reader )
        {
            var stringBuilder = new StringBuilder();

            char c;
            while ( ( c = reader.ReadChar() ) != 0 )
                stringBuilder.Append( c );

            return stringBuilder.ToString();
        }

        public static string ReadString( this BinaryReader reader, int length )
        {
            var stringBuilder = new StringBuilder( length );
            for ( int i = 0; i < length; i++ )
            {
                char c = reader.ReadChar();
                if ( c != 0 )
                    stringBuilder.Append( c );
            }

            return stringBuilder.ToString();
        }

        // Yeah, don't ask about this method, please.
        public static long ReadPackedInt( this BinaryReader reader )
        {
            byte firstByte = reader.ReadByte();
            if ( ( firstByte & 0x80 ) == 0 )
                return firstByte;

            switch ( firstByte >> 3 )
            {
                case 0x10:
                case 0x11:
                case 0x12:
                case 0x13:
                case 0x14:
                case 0x15:
                case 0x16:
                case 0x17:
                    return ( ( firstByte << 8 ) | reader.ReadByte() ) & 0x3FFF;

                case 0x18:
                case 0x19:
                case 0x1A:
                case 0x1B:
                    return ( ( firstByte << 16 ) | ( reader.ReadByte() << 8 ) | reader.ReadByte() ) & 0x1FFFFF;

                case 0x1C:
                    return ( ( firstByte << 24 ) | ( reader.ReadByte() << 16 ) | ( reader.ReadByte() << 8 ) |
                             reader.ReadByte() ) &
                           0x7FFFFFF;

                case 0x1D:
                    return ( ( firstByte << 32 ) | ( reader.ReadByte() << 24 ) | ( reader.ReadByte() << 16 ) |
                             ( reader.ReadByte() << 8 ) | reader.ReadByte() ) & 0x7FFFFFFFFFFFFFF;

                case 0x1E:
                    return ( ( firstByte << 56 ) | ( reader.ReadByte() << 48 ) | ( reader.ReadByte() << 40 ) |
                             ( reader.ReadByte() << 32 ) | ( reader.ReadByte() << 24 ) | ( reader.ReadByte() << 16 ) |
                             ( reader.ReadByte() << 8 ) | reader.ReadByte() ) & 0x7FFFFFFFFFFFFFF;

                case 0x1F:
                    return ( firstByte & 7 ) == 0
                        ? ( ( firstByte << 40 ) | ( reader.ReadByte() << 32 ) | ( reader.ReadByte() << 24 ) |
                            ( reader.ReadByte() << 16 ) |
                            ( reader.ReadByte() << 8 ) | reader.ReadByte() ) & 0xFFFFFFFFFF
                        : ( firstByte & 7 ) == 1
                            ? ( reader.ReadByte() << 56 ) | ( reader.ReadByte() << 48 ) | ( reader.ReadByte() << 40 ) |
                              ( reader.ReadByte() << 32 ) | ( reader.ReadByte() << 24 ) | ( reader.ReadByte() << 16 ) |
                              ( reader.ReadByte() << 8 ) | reader.ReadByte()
                            : 0;

                default:
                    return 0;
            }
        }
    }
}