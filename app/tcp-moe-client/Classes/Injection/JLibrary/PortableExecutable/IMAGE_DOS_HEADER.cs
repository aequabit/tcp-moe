namespace JLibrary.PortableExecutable
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential, Pack=1)]
    public struct IMAGE_DOS_HEADER
    {
        public ushort e_magic;
        public ushort e_cblp;
        public ushort e_cp;
        public ushort e_crlc;
        public ushort e_cparhdr;
        public ushort e_minalloc;
        public ushort e_maxalloc;
        public ushort e_ss;
        public ushort e_sp;
        public ushort e_csum;
        public ushort e_ip;
        public ushort e_cs;
        public ushort e_lfarlc;
        public ushort e_ovno;
        public ushort e_res_0;
        public ushort e_res_1;
        public ushort e_res_2;
        public ushort e_res_3;
        public ushort e_oemid;
        public ushort e_oeminfo;
        public ushort e_res2_0;
        public ushort e_res2_1;
        public ushort e_res2_2;
        public ushort e_res2_3;
        public ushort e_res2_4;
        public ushort e_res2_5;
        public ushort e_res2_6;
        public ushort e_res2_7;
        public ushort e_res2_8;
        public ushort e_res2_9;
        public uint e_lfanew;
    }
}

