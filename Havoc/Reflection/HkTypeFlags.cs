using System;

namespace Havoc.Reflection
{
    [Flags]
    public enum HkTypeFlags
    {
        HasFormatInfo = 1 << 0,
        HasSubType = 1 << 1,
        HasVersion = 1 << 2,
        HasByteSize = 1 << 3,
        HasUnknownFlags = 1 << 4,
        HasFields = 1 << 5,
        HasInterfaces = 1 << 6,
        HasUnknownData = 1 << 7
    }
}