using System;

namespace Havoc.Reflection.Unmanaged
{
    [Flags]
    public enum HkuOptionalValues
    {
        FormatInfoOfType = 1 << 0,
        SubTypeOfType = 1 << 1,
        NameOfType = 1 << 3,
        VersionOfType = 1 << 4,
        InterfacesOfType = 1 << 17,
        ParametersOfType = 1 << 18,
        NameOfField = 1 << 19,
        ByteOffsetAndFlagsOfField = 1 << 20,
        ParentOfField = 1 << 21,
        ByteSizeOfType = 1 << 23,
        UnknownFlagsOfType = 1 << 24,
        FieldsOfType = 1 << 26
    }
}