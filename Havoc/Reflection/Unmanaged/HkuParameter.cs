using System;
using System.Runtime.InteropServices;

namespace Havoc.Reflection.Unmanaged
{
    public unsafe struct HkuParameter
    {
        public readonly void* Value;
        public readonly byte* Name;

        public bool IsIntValue => Name[ 0 ] == 'v';
        public bool IsTypeValue => Name[ 0 ] == 't';

        public static int GetArrayCount( void* array )
        {
            return *( ushort* ) array;
        }

        public static HkuParameter* GetArrayItem( void* array, int index )
        {
            return ( HkuParameter* ) &( ( void** ) array )[ index * 2 + 1 ];
        }

        public string GetName()
        {
            return Marshal.PtrToStringAnsi( ( IntPtr ) Name );
        }
    }
}