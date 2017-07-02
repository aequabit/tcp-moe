namespace JLibrary.PortableExecutable
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    [Serializable, StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct IMAGE_SECTION_HEADER
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
        public byte[] Name;
        public uint VirtualSize;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLineNumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLineNumbers;
        public uint Characteristics;
        public override string ToString()
        {
            string str = Encoding.UTF8.GetString(this.Name);
            if (str.Contains("\0"))
            {
                str = str.Substring(0, str.IndexOf("\0"));
            }
            return str;
        }
    }
}

