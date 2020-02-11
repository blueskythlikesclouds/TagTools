using System;
using System.Collections.Generic;
using Havoc.Reflection;

namespace Havoc.Objects
{
    public class HkArray : IHkObject
    {
        public HkArray( HkType type, IReadOnlyList<IHkObject> value )
        {
            if ( type.Format != HkTypeFormat.Array )
                throw new ArgumentException( "Type must be of an array type.", nameof( type ) );

            if ( type.IsFixedSize && value.Count != type.FixedSize )
                throw new ArgumentOutOfRangeException( nameof( value ), "Array size must be equal to fixed size." );

            Type = type;
            Value = value;
        }

        public IReadOnlyList<IHkObject> Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }
}