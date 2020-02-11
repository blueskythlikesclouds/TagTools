using System;
using System.Collections.Generic;
using Havoc.Reflection;

namespace Havoc.Objects
{
    public class HkClass : IHkObject
    {
        public HkClass( HkType type, IReadOnlyDictionary<HkField, IHkObject> value )
        {
            if ( type.Format != HkTypeFormat.Class )
                throw new ArgumentException( "Type must be of a class type.", nameof( type ) );

            // TODO: Check if member infos are valid?

            Type = type;
            Value = value;
        }

        public IReadOnlyDictionary<HkField, IHkObject> Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }
}