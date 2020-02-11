using System;
using Havoc.Reflection;

namespace Havoc.Objects
{
    public class HkVoid : IHkObject
    {
        public HkVoid( HkType type )
        {
            if ( type.Format != HkTypeFormat.Void )
                throw new ArgumentException( "Type must be of a void type.", nameof( type ) );

            Type = type;
        }

        public HkType Type { get; }

        object IHkObject.Value => null;
    }
}