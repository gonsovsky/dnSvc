using System;

namespace SmbLibraryStd.SMB2
{
    [Flags]
    public enum ReadFlags : byte
    {
        Unbuffered = 0x01, // SMB2_READFLAG_READ_UNBUFFERED;
    }
}
