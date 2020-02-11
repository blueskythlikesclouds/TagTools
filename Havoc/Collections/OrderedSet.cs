using System;
using System.Collections;
using System.Collections.Generic;

namespace Havoc.Collections
{
    public class OrderedSet<T> : IList<T>, IReadOnlyList<T>
    {
        private readonly Dictionary<T, int> mIndexMap = new Dictionary<T, int>();
        private readonly List<T> mList = new List<T>();

        public IReadOnlyDictionary<T, int> IndexMap => mIndexMap;

        public int Count => mList.Count;
        public bool IsReadOnly => ( ( IList<T> ) mList ).IsReadOnly;

        public T this[ int index ]
        {
            get => mList[ index ];
            set
            {
                mIndexMap.Remove( mList[ index ] );
                mIndexMap.Add( value, index );
                mList[ index ] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        public void Add( T item )
        {
            if ( mIndexMap.ContainsKey( item ) )
                return;

            mIndexMap.Add( item, mList.Count );
            mList.Add( item );
        }

        public void Clear()
        {
            mIndexMap.Clear();
            mList.Clear();
        }

        public bool Contains( T item )
        {
            return mIndexMap.ContainsKey( item );
        }

        public void CopyTo( T[] array, int arrayIndex )
        {
            mList.CopyTo( array, arrayIndex );
        }

        public bool Remove( T item )
        {
            if ( !mIndexMap.TryGetValue( item, out int index ) )
                return false;

            mIndexMap.Remove( item );
            mList.RemoveAt( index );

            foreach ( var value in mList )
            {
                int refIndex = mIndexMap[ value ];
                if ( refIndex > index )
                    mIndexMap[ value ] = refIndex - 1;
            }

            return true;
        }

        public int IndexOf( T item )
        {
            if ( mIndexMap.TryGetValue( item, out int index ) )
                return index;

            return -1;
        }

        public void Insert( int index, T item )
        {
            if ( mIndexMap.ContainsKey( item ) )
                return;

            index = Math.Min( index, mList.Count );
            if ( index != mList.Count )
                foreach ( var value in mList )
                {
                    int refIndex = mIndexMap[ value ];
                    if ( refIndex >= index )
                        mIndexMap[ value ] = refIndex + 1;
                }

            mIndexMap.Add( item, index );
            mList.Insert( index, item );
        }

        public void RemoveAt( int index )
        {
            if ( index < 0 || index >= mList.Count )
                return;

            Remove( mList[ index ] );
        }

        public void Add( T item, out int index )
        {
            if ( mIndexMap.TryGetValue( item, out index ) )
                return;

            index = mList.Count;

            mIndexMap.Add( item, index );
            mList.Add( item );
        }
    }
}