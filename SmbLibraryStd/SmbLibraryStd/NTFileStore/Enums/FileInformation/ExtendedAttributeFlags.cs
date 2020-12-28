using System;

namespace SmbLibraryStd
{
    [Flags]
    public enum ExtendedAttributeFlags : byte
    {
        FILE_NEED_EA = 0x80,
    }
}
