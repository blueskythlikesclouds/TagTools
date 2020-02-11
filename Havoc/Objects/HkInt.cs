using System;
using Havoc.Reflection;

namespace Havoc.Objects
{
    public class HkSByte : IHkObject
    {
        public HkSByte( HkType type, sbyte value )
        {
            if ( type.Format != HkTypeFormat.Int || type.BitCount != 8 || !type.IsSigned )
                throw new ArgumentException( "Type must be of an int8 type.", nameof( type ) );

            Type = type;
            Value = value;
        }

        public sbyte Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }

    public class HkByte : IHkObject
    {
        public HkByte( HkType type, byte value )
        {
            if ( type.Format != HkTypeFormat.Int || type.BitCount != 8 || type.IsSigned )
                throw new ArgumentException( "Type must be of an uint8 type.", nameof( type ) );

            Type = type;
            Value = value;
        }

        public byte Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }

    public class HkInt16 : IHkObject
    {
        public HkInt16( HkType type, short value )
        {
            if ( type.Format != HkTypeFormat.Int || type.BitCount != 16 || !type.IsSigned )
                throw new ArgumentException( "Type must be of an int16 type.", nameof( type ) );

            Type = type;
            Value = value;
        }

        public short Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }

    public class HkUInt16 : IHkObject
    {
        public HkUInt16( HkType type, ushort value )
        {
            if ( type.Format != HkTypeFormat.Int || type.BitCount != 16 || type.IsSigned )
                throw new ArgumentException( "Type must be of an uint16 type.", nameof( type ) );

            Type = type;
            Value = value;
        }

        public ushort Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }

    public class HkInt32 : IHkObject
    {
        public HkInt32( HkType type, int value )
        {
            if ( type.Format != HkTypeFormat.Int || type.BitCount != 32 || !type.IsSigned )
                throw new ArgumentException( "Type must be of an int32 type.", nameof( type ) );

            Type = type;
            Value = value;
        }

        public int Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }

    public class HkUInt32 : IHkObject
    {
        public HkUInt32( HkType type, uint value )
        {
            if ( type.Format != HkTypeFormat.Int || type.BitCount != 32 || type.IsSigned )
                throw new ArgumentException( "Type must be of an uint32 type.", nameof( type ) );

            Type = type;
            Value = value;
        }

        public uint Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }

    public class HkInt64 : IHkObject
    {
        public HkInt64( HkType type, long value )
        {
            if ( type.Format != HkTypeFormat.Int || type.BitCount != 64 || !type.IsSigned )
                throw new ArgumentException( "Type must be of an int64 type.", nameof( type ) );

            Type = type;
            Value = value;
        }

        public long Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }

    public class HkUInt64 : IHkObject
    {
        public HkUInt64( HkType type, ulong value )
        {
            if ( type.Format != HkTypeFormat.Int || type.BitCount != 64 || type.IsSigned )
                throw new ArgumentException( "Type must be of an uint64 type.", nameof( type ) );

            Type = type;
            Value = value;
        }

        public ulong Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }
}