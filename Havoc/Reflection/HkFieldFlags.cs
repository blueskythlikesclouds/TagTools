using System;

namespace Havoc.Reflection
{
    [Flags]
    public enum HkFieldFlags
    {
        IsNotSerializable = 1 << 0,
        IsProtected = 1 << 1,
        IsPrivate = 1 << 2,
        UnknownFlag = 1 << 5
    }
}