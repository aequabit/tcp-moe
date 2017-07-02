namespace JLibrary.PortableExecutable
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_RESOURCE_DATA_ENTRY
    {
        public uint OffsetToData;
        public uint Size;
        public uint CodePage;
        public uint Reserved;
    }
}

