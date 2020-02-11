using System;
using Havoc.Reflection;

namespace Havoc.Objects
{
    public class HkBool : IHkObject
    {
        public HkBool( HkType type, bool value )
        {
            if ( type.Format != HkTypeFormat.Bool )
                throw new ArgumentException( "Type must be of a bool type.", nameof( type ) );

            Type = type;
            Value = value;
        }

        public bool Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }
}