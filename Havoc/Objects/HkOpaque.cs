using System;
using Havoc.Reflection;

namespace Havoc.Objects
{
    public class HkOpaque : IHkObject
    {
        public HkOpaque( HkType type )
        {
            if ( type.Format != HkTypeFormat.Opaque )
                throw new ArgumentException( "Type must be of an opaque type.", nameof( type ) );

            Type = type;
        }

        public HkType Type { get; }

        object IHkObject.Value => null;
    }
}