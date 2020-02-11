using System;
using System.IO;

namespace Havoc.Extensions
{
    public static class BinaryWriterEx
    {
        public static void WriteAlignmentPadding( this BinaryWriter writer, int alignment )
        {
            int amount = alignment - ( int ) ( writer.BaseStream.Position % alignment );

            if ( amount == alignment )
                return;

            for ( int i = 0; i < amount; i++ )
                writer.Write( ( byte ) 0 );
        }

        public static void Write( this BinaryWriter writer, string value, int length )
        {
            if ( value.Length > length )
                throw new ArgumentException( "Provided string is longer than fixed length.", nameof( value ) );

            for ( int i = 0; i < value.Length; i++ )
                writer.Write( value[ i ] );

            for ( int i = 0; i < length - value.Length; i++ )
                writer.Write( '\0' );
        }

        public static void WriteNullTerminatedString( this BinaryWriter writer, string value )
        {
            for ( int i = 0; i < value.Length; i++ )
                writer.Write( value[ i ] );

            writer.Write( '\0' );
        }

        public static void WritePackedInt( this BinaryWriter writer, long value )
        {
            if ( value < 0x80 )
            {
                writer.Write( ( byte ) value );
            }
            else if ( value < 0x4000 )
            {
                writer.Write( ( byte ) ( ( ( value >> 8 ) & 0xFF ) | 0x80 ) );
                writer.Write( ( byte ) ( value & 0xFF ) );
            }
            else if ( value < 0x200000 )
            {
                writer.Write( ( byte ) ( ( ( value >> 16 ) & 0xFF ) | 0xC0 ) );
                writer.Write( ( byte ) ( ( value >> 8 ) & 0xFF ) );
                writer.Write( ( byte ) ( value & 0xFF ) );
            }
            else if ( value < 0x8000000 )
            {
                writer.Write( ( byte ) ( ( ( value >> 24 ) & 0xFF ) | 0xE0 ) );
                writer.Write( ( byte ) ( ( value >> 16 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 8 ) & 0xFF ) );
                writer.Write( ( byte ) ( value & 0xFF ) );
            }
            else if ( value < 0x800000000 )
            {
                writer.Write( ( byte ) ( ( ( value >> 32 ) & 0xFF ) | 0xE8 ) );
                writer.Write( ( byte ) ( ( value >> 24 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 16 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 8 ) & 0xFF ) );
                writer.Write( ( byte ) ( value & 0xFF ) );
            }
            else if ( value < 0x10000000000 )
            {
                writer.Write( ( byte ) 0xF8 );
                writer.Write( ( byte ) ( ( value >> 32 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 24 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 16 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 8 ) & 0xFF ) );
                writer.Write( ( byte ) ( value & 0xFF ) );
            }
            else if ( value < 0x800000000000000 )
            {
                writer.Write( ( byte ) ( ( ( value >> 56 ) & 0xFF ) | 0xF0 ) );
                writer.Write( ( byte ) ( ( value >> 48 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 40 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 32 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 24 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 16 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 8 ) & 0xFF ) );
                writer.Write( ( byte ) ( value & 0xFF ) );
            }
            else
            {
                writer.Write( ( byte ) 0xF9 );
                writer.Write( ( byte ) ( ( value >> 56 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 48 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 40 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 32 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 24 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 16 ) & 0xFF ) );
                writer.Write( ( byte ) ( ( value >> 8 ) & 0xFF ) );
                writer.Write( ( byte ) ( value & 0xFF ) );
            }
        }

        public static void WriteNulls( this BinaryWriter writer, int count )
        {
            for ( int i = 0; i < count / 8; i++ )
                writer.Write( 0ul );

            for ( int i = 0; i < count % 8; i++ )
                writer.Write( ( byte ) 0 );
        }
    }
}