using System;
using Havoc.Reflection;

namespace Havoc.Objects
{
    public class HkString : IHkObject
    {
        public HkString( HkType type, string value )
        {
            if ( type.Format != HkTypeFormat.String )
                throw new ArgumentException( "Type must be of a string type." );

            if ( type.IsFixedSize && value.Length > type.FixedSize )
                throw new ArgumentOutOfRangeException( nameof( value ),
                    "Value length must be less than or equal to fixed size." );

            Type = type;
            Value = value;
        }

        public string Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }
}