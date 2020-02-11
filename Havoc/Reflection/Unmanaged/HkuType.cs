using System;
using System.Runtime.InteropServices;

namespace Havoc.Reflection.Unmanaged
{
    public static unsafe class HkuType
    {
        public static void* GetParentType( void* type )
        {
            return HkuOptionalStruct.GetAssociatedType( type );
        }

        public static int GetFormatInfo( void* type )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( type, ( int ) HkuOptionalValues.FormatInfoOfType );
            return ptr != null ? ( int ) *ptr : 0;
        }

        public static void* GetSubType( void* type )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( type, ( int ) HkuOptionalValues.SubTypeOfType );
            return ptr != null ? *ptr : null;
        }

        public static string GetName( void* type )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( type, ( int ) HkuOptionalValues.NameOfType );
            return ptr != null && *ptr != null ? Marshal.PtrToStringAnsi( ( IntPtr ) ( *ptr ) ) : null;
        }

        public static int GetVersion( void* type )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( type, ( int ) HkuOptionalValues.VersionOfType );
            return ptr != null ? ( int ) *ptr : 0;
        }

        public static void* GetInterfaces( void* type )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( type, ( int ) HkuOptionalValues.InterfacesOfType );
            return ptr != null ? *ptr : null;
        }

        public static void* GetParameters( void* type )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( type, ( int ) HkuOptionalValues.ParametersOfType );
            if ( ptr == null || *ptr == null )
                return null;

            var value = *ptr;

            // 2016 2.0 and above adds 
            // the parent type before the array
            // for whatever reason
            if ( ( *value & ~0xFFFF ) != 0 )
                return &value[ 1 ];

            return value;
        }

        public static int GetByteSize( void* field )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( field, ( int ) HkuOptionalValues.ByteSizeOfType );
            return ptr != null ? *( short* ) ptr : 0;
        }

        public static int GetAlignment( void* field )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( field, ( int ) HkuOptionalValues.ByteSizeOfType );
            return ptr != null ? ( ( short* ) ptr )[ 1 ] : 0;
        }

        public static int GetUnknownFlags( void* type )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( type, ( int ) HkuOptionalValues.UnknownFlagsOfType );
            return ptr != null ? ( int ) *ptr : 0;
        }

        public static void* GetFields( void* type )
        {
            var ptr = HkuOptionalStruct.GetOptionalValue( type, ( int ) HkuOptionalValues.FieldsOfType );
            return ptr != null ? *ptr : null;
        }
    }
}