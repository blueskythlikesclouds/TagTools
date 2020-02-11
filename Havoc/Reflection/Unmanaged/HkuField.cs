using System;
using System.Runtime.InteropServices;

namespace Havoc.Reflection.Unmanaged
{
    public static unsafe class HkuField
    {
        public static void* GetType( void* field )
        {
            return HkuOptionalStruct.GetAssociatedType( field );
        }

        public static int GetByteOffset( void* field )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( field, ( int ) HkuOptionalValues.ByteOffsetAndFlagsOfField );
            return ptr != null ? *( short* ) ptr : 0;
        }

        public static int GetFlags( void* field )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( field, ( int ) HkuOptionalValues.ByteOffsetAndFlagsOfField );
            return ptr != null ? ( ( short* ) ptr )[ 1 ] : 0;
        }

        public static string GetName( void* field )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( field, ( int ) HkuOptionalValues.NameOfField );
            return ptr != null && *ptr != null ? Marshal.PtrToStringAnsi( ( IntPtr ) ( *ptr ) ) : null;
        }

        public static void* GetParent( void* field )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( field, ( int ) HkuOptionalValues.ParentOfField );
            return ptr != null ? *ptr : null;
        }

        public static int GetArrayCount( void* array )
        {
            return ( ( ushort* ) array )[ 0 ] + ( ( ushort* ) array )[ 1 ];
        }

        public static void* GetArrayItem( void* array, int index )
        {
            return ( ( void** ) array )[ index + 1 ];
        }
    }
}