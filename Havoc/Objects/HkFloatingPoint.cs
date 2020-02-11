using System;
using Havoc.Reflection;

namespace Havoc.Objects
{
    public class HkSingle : IHkObject
    {
        public HkSingle( HkType type, float value )
        {
            if ( !type.IsSingle )
                throw new ArgumentException( "Type must be of a float type.", nameof( type ) );

            Type = type;
            Value = value;
        }

        public float Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }

    public class HkDouble : IHkObject
    {
        public HkDouble( HkType type, double value )
        {
            if ( !type.IsDouble )
                throw new ArgumentException( "Type must be of a double type.", nameof( type ) );

            Type = type;
            Value = value;
        }

        public double Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }
}