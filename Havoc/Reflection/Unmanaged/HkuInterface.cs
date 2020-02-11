namespace Havoc.Reflection.Unmanaged
{
    public unsafe struct HkuInterface
    {
        public readonly void* Type;
        public readonly long Flags;

        public static int GetArrayCount( void* array )
        {
            return *( ushort* ) array;
        }

        public static HkuInterface* GetArrayItem( void* array, int index )
        {
            return ( HkuInterface* ) &( ( void** ) array )[ index * 2 + 1 ];
        }
    }
}