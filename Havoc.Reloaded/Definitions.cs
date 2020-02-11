using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions.X64;

namespace Havoc.Reloaded
{
    public unsafe struct TypeHasher
    {
        public fixed byte Data[ 40 ];
    }

    [Function( CallingConventions.Microsoft )]
    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    public unsafe delegate void* TypeNodeInitializer( void* rcx, void* type );

    [Function( CallingConventions.Microsoft )]
    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    public unsafe delegate long TypeHashCalculator( TypeHasher* typeHasher, void* type );

    [Function( CallingConventions.Microsoft )]
    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    public unsafe delegate void* TypeHasherCtor( TypeHasher* typeHasher, int a2 );

    public static class Patterns
    {
        public const string TypeNodeInitializer =
            "48 8B 05 ?? ?? ?? ?? 48 83 E0 FC 48 89 51 08 48 89 01 33 C0 48 89 41 10 48 89 41 18 48 8B C1 48 89 0D ?? ?? ?? ?? C3";

        public const string TypeHashCalculator =
            "48 89 54 24 ?? 53 57 48 83 EC 28 48 8B DA 48 8B F9 48 85 D2 75 09 33 C0 48 83 C4 28 5F 5B C3 48 B8 ?? ?? ?? ?? ?? ?? " +
            "?? ?? 48 89 74 24 ?? 33 F6 48 89 44 24 ?? 4C 8D 44 24 ?? 48 89 74 24 ?? 48 8D 54 24 ?? E8 ?? ?? ?? ?? 48 8B 08 48 85 " +
            "C9 74 0E 8B C1 48 8B 74 24 ?? 48 83 C4 28 5F 5B C3 4C 8D 44 24 ?? 48 8B CF 48 8D 54 24 ?? E8 ?? ?? ?? ?? 48 8B 53 08 " +
            "48 85 D2 74 0A 48 8B CF E8 ?? ?? ?? ?? 8B F0 4C 8B C3 8B D6 48 8B CF E8 ?? ?? ?? ?? 8B D8 4C 8D 44 24 ?? 48 8D 54 24 " +
            "?? 48 89 5C 24 ?? 48 8B CF E8 ?? ?? ?? ?? 48 8B 74 24 ?? 8B C3 48 83 C4 28 5F 5B C3";

        // TODO: This is from Sonic Forces. Dark Souls Remastered seems to inline this function. Need to handle that, somehow.
        public const string TypeHasherCtor =
            "45 33 C0 C7 41 ?? ?? ?? ?? ?? 4C 89 01 48 8D 05 ?? ?? ?? ?? 44 89 41 08 48 89 41 10 48 8B C1 44 89 41 18 48 89 51 20 C3";
    }
}