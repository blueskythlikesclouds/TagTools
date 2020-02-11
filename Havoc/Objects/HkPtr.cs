using System;
using Havoc.Reflection;

namespace Havoc.Objects
{
    public class HkPtr : IHkObject
    {
        public HkPtr( HkType type, IHkObject value )
        {
            if ( type.Format != HkTypeFormat.Ptr )
                throw new ArgumentException( "Type must be of a pointer type." );

            Type = type;
            Value = value;
        }

        public IHkObject Value { get; }
        public HkType Type { get; }

        object IHkObject.Value => Value;
    }
}