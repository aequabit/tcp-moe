namespace JLibrary.PortableExecutable
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct IMAGE_THUNK_DATA
    {
        public U1 u1;
    }
}

