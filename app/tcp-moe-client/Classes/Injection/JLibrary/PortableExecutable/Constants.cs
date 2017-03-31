namespace JLibrary.PortableExecutable
{
    using System;

    public static class Constants
    {
        public const uint CREATEPROCESS_MANIFEST_RESOURCE_ID = 1;
        public const ushort DOS_SIGNATURE = 0x5a4d;
        public const uint ISOLATIONAWARE_MANIFEST_RESOURCE_ID = 2;
        public const uint ISOLATIONAWARE_NOSTATICIMPORT_MANIFEST_RESOURCE_ID = 3;
        public const uint NT_SIGNATURE = 0x4550;
        public const ushort PE32_FORMAT = 0x10b;
        public const ushort PE32P_FORMAT = 0x20b;
        public const uint RT_MANIFEST = 0x18;
    }
}

