using System.Collections;
using System.Collections.Generic;
using Havoc.Collections;
using Havoc.Objects;

namespace Havoc.Reflection
{
    public class HkTypeCompendium : IReadOnlyList<HkType>
    {
        private readonly OrderedSet<HkType> mTypes = new OrderedSet<HkType>();

        public HkTypeCompendium( IHkObject obj )
        {
            AddRecursively( obj );
        }

        public HkTypeCompendium( IEnumerable<HkType> types )
        {
            foreach ( var type in types )
                Add( type );
        }

        public IReadOnlyDictionary<HkType, int> IndexMap => mTypes.IndexMap;

        public IEnumerator<HkType> GetEnumerator()
        {
            return mTypes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ( ( IEnumerable ) mTypes ).GetEnumerator();
        }

        public int Count => mTypes.Count;

        public HkType this[ int index ] => mTypes[ index ];

        internal void Add( HkType type )
        {
            // TODO: Figure out how Havok SDK adds types to the compendium
            if ( type == null )
                return;

            AddIfNotNull( type );

            foreach ( var item in type.mParameters )
                AddIfNotNull( item.Value as HkType );

            AddIfNotNull( type.ParentType );

            AddIfNotNull( type.SubType );

            foreach ( var item in type.mFields )
                AddIfNotNull( item.Type );

            foreach ( var item in type.mInterfaces )
                AddIfNotNull( item.Type );

            void AddIfNotNull( HkType t )
            {
                if ( t != null && !mTypes.Contains( t ) )
                {
                    mTypes.Add( t );
                    Add( t );
                }
            }
        }

        internal void AddRecursively( IHkObject obj )
        {
            while ( true )
            {
                Add( obj.Type );

                if ( obj.Value == null )
                    break;

                switch ( obj.Type.Format )
                {
                    case HkTypeFormat.Ptr:
                        obj = ( IHkObject ) obj.Value;
                        continue;

                    case HkTypeFormat.Class:
                    {
                        foreach ( var item in ( IReadOnlyDictionary<HkField, IHkObject> ) obj.Value )
                            AddRecursively( item.Value );
                        break;
                    }

                    case HkTypeFormat.Array:
                    {
                        foreach ( var item in ( IEnumerable<IHkObject> ) obj.Value ) AddRecursively( item );
                        break;
                    }
                }

                break;
            }
        }
    }
}