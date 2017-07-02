namespace JLibrary.PortableExecutable
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Explicit)]
    public struct IMAGE_RESOURCE_DIRECTORY_ENTRY
    {
        [FieldOffset(4)]
        public uint DataEntryRva;
        [FieldOffset(0)]
        public uint IntegerId;
        [FieldOffset(0)]
        public uint NameRva;
        [FieldOffset(4)]
        public uint SubdirectoryRva;
    }
}

