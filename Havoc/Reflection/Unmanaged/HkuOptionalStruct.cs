namespace Havoc.Reflection.Unmanaged
{
    public static unsafe class HkuOptionalStruct
    {
        public static int GetSize( void* ptr )
        {
            return sizeof( void* ) * 2 + GetBitCount( *( int* ) ptr ) * sizeof( void* );
        }

        public static void* GetAssociatedType( void* ptr )
        {
            return ( ( long** ) ptr )[ 1 ];
        }

        public static bool HasOptionalValue( void* ptr, int member )
        {
            return ( *( int* ) ptr & member ) != 0;
        }

        public static long** GetOptionalValue( void* ptr, int member )
        {
            if ( !HasOptionalValue( ptr, member ) )
                return null;

            return &( ( long** ) ptr )[ GetBitCount( *( int* ) ptr & ( member - 1 ) ) + 2 ];
        }

        private static int GetBitCount( int value )
        {
            value -= ( value >> 1 ) & 0x55555555;
            value = ( value & 0x33333333 ) + ( ( value >> 2 ) & 0x33333333 );
            return ( ( ( value + ( value >> 4 ) ) & 0x0F0F0F0F ) * 0x01010101 ) >> 24;
        }
    }
}