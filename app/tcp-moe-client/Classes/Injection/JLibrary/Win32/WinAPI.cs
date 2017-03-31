namespace JLibrary.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class WinAPI
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool CloseHandle(IntPtr handle);
        public static IntPtr CreateRemotePointer(IntPtr hProcess, byte[] pData, int flProtect)
        {
            IntPtr zero = IntPtr.Zero;
            if ((pData != null) && (hProcess != IntPtr.Zero))
            {
                zero = VirtualAllocEx(hProcess, IntPtr.Zero, (uint) pData.Length, 0x3000, flProtect);
                uint lpNumberOfBytesRead = 0;
                if (((zero != IntPtr.Zero) && WriteProcessMemory(hProcess, zero, pData, pData.Length, out lpNumberOfBytesRead)) && (lpNumberOfBytesRead == pData.Length))
                {
                    return zero;
                }
                if (zero != IntPtr.Zero)
                {
                    VirtualFreeEx(hProcess, zero, 0, 0x8000);
                    zero = IntPtr.Zero;
                }
            }
            return zero;
        }

        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, int lpThreadAttributes, int dwStackSize, IntPtr lpStartAddress, uint lpParameter, int dwCreationFlags, int lpThreadId);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool GetExitCodeThread(IntPtr hThread, out uint lpExitCode);
        public static uint GetLastErrorEx(IntPtr hProcess)
        {
            IntPtr procAddress = GetProcAddress(GetModuleHandleA("kernel32.dll"), "GetLastError");
            return RunThread(hProcess, procAddress, 0, 0x3e8);
        }

        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern IntPtr GetModuleHandleA(string lpModuleName);
        public static IntPtr GetModuleHandleEx(IntPtr hProcess, string lpModuleName)
        {
            IntPtr procAddress = GetProcAddress(GetModuleHandleA("kernel32.dll"), "GetModuleHandleW");
            IntPtr zero = IntPtr.Zero;
            if (!procAddress.IsNull())
            {
                IntPtr ptr = CreateRemotePointer(hProcess, Encoding.Unicode.GetBytes(lpModuleName + "\0"), 4);
                if (!ptr.IsNull())
                {
                    zero = Win32Ptr.Create((long) RunThread(hProcess, procAddress, (uint) ptr.ToInt32(), 0x3e8));
                    VirtualFreeEx(hProcess, ptr, 0, 0x8000);
                }
            }
            return zero;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, uint lpProcName);
        public static IntPtr GetProcAddressEx(IntPtr hProc, IntPtr hModule, object lpProcName)
        {
            IntPtr zero = IntPtr.Zero;
            byte[] buffer = ReadRemoteMemory(hProc, hModule, 0x40);
            if ((buffer == null) || (BitConverter.ToUInt16(buffer, 0) != 0x5a4d))
            {
                return zero;
            }
            uint num = BitConverter.ToUInt32(buffer, 60);
            if (num <= 0)
            {
                return zero;
            }
            byte[] buffer2 = ReadRemoteMemory(hProc, hModule.Add((long) num), 0x108);
            if ((buffer2 == null) || (BitConverter.ToUInt32(buffer2, 0) != 0x4550))
            {
                return zero;
            }
            uint num2 = BitConverter.ToUInt32(buffer2, 120);
            uint num3 = BitConverter.ToUInt32(buffer2, 0x7c);
            if ((num2 <= 0) || (num3 <= 0))
            {
                return zero;
            }
            byte[] buffer3 = ReadRemoteMemory(hProc, hModule.Add((long) num2), 40);
            uint num4 = BitConverter.ToUInt32(buffer3, 0x1c);
            uint num5 = BitConverter.ToUInt32(buffer3, 0x24);
            uint num6 = BitConverter.ToUInt32(buffer3, 20);
            int num7 = -1;
            if ((num4 <= 0) || (num5 <= 0))
            {
                return zero;
            }
            if (lpProcName.GetType().Equals(typeof(string)))
            {
                int num8 = SearchExports(hProc, hModule, buffer3, (string) lpProcName);
                if (num8 > -1)
                {
                    byte[] buffer4 = ReadRemoteMemory(hProc, hModule.Add((long) (num5 + (num8 << 1))), 2);
                    num7 = (buffer4 == null) ? -1 : BitConverter.ToUInt16(buffer4, 0);
                }
            }
            else if (lpProcName.GetType().Equals(typeof(short)) || lpProcName.GetType().Equals(typeof(ushort)))
            {
                num7 = int.Parse(lpProcName.ToString());
            }
            if ((num7 <= -1) || (num7 >= num6))
            {
                return zero;
            }
            byte[] buffer5 = ReadRemoteMemory(hProc, hModule.Add((long) (num4 + (num7 << 2))), 4);
            if (buffer5 == null)
            {
                return zero;
            }
            uint num9 = BitConverter.ToUInt32(buffer5, 0);
            if ((num9 >= num2) && (num9 < (num2 + num3)))
            {
                string str = ReadRemoteString(hProc, hModule.Add((long) num9), null);
                if (!(string.IsNullOrEmpty(str) || !str.Contains(".")))
                {
                    zero = GetProcAddressEx(hProc, GetModuleHandleEx(hProc, str.Split(new char[] { '.' })[0]), str.Split(new char[] { '.' })[1]);
                }
                return zero;
            }
            return hModule.Add(((long) num9));
        }

        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern int GetProcessId(IntPtr hProcess);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT pContext);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern IntPtr OpenThread(uint dwDesiredAccess, bool bInheritHandle, int dwThreadId);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out uint lpNumberOfBytesRead);
        public static byte[] ReadRemoteMemory(IntPtr hProc, IntPtr address, uint len)
        {
            byte[] lpBuffer = new byte[len];
            uint lpNumberOfBytesRead = 0;
            if (!(ReadProcessMemory(hProc, address, lpBuffer, lpBuffer.Length, out lpNumberOfBytesRead) && (lpNumberOfBytesRead == len)))
            {
                lpBuffer = null;
            }
            return lpBuffer;
        }

        public static IntPtr ReadRemotePointer(IntPtr hProcess, IntPtr pData)
        {
            IntPtr zero = IntPtr.Zero;
            if (!hProcess.IsNull() && !pData.IsNull())
            {
                byte[] buffer = null;
                buffer = ReadRemoteMemory(hProcess, pData, (uint) IntPtr.Size);
                if (buffer != null)
                {
                    zero = new IntPtr(BitConverter.ToInt32(buffer, 0));
                }
            }
            return zero;
        }

        public static string ReadRemoteString(IntPtr hProcess, IntPtr lpAddress, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.ASCII;
            }
            StringBuilder builder = new StringBuilder();
            byte[] lpBuffer = new byte[0x100];
            uint lpNumberOfBytesRead = 0;
            int length = -1;
            int startIndex = 0;
            while (((length < 0) && ReadProcessMemory(hProcess, lpAddress, lpBuffer, lpBuffer.Length, out lpNumberOfBytesRead)) && (lpNumberOfBytesRead > 0))
            {
                lpAddress = lpAddress.Add((long) lpNumberOfBytesRead);
                startIndex = builder.Length;
                builder.Append(encoding.GetString(lpBuffer, 0, (int) lpNumberOfBytesRead));
                length = builder.ToString().IndexOf('\0', startIndex);
            }
            return builder.ToString().Substring(0, length);
        }

        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern uint ResumeThread(IntPtr hThread);
        public static uint RunThread(IntPtr hProcess, IntPtr lpStartAddress, uint lpParam, int timeout = 0x3e8)
        {
            uint maxValue = uint.MaxValue;
            IntPtr hObject = CreateRemoteThread(hProcess, 0, 0, lpStartAddress, lpParam, 0, 0);
            if ((hObject != IntPtr.Zero) && (WaitForSingleObject(hObject, timeout) == 0L))
            {
                GetExitCodeThread(hObject, out maxValue);
            }
            return maxValue;
        }

        private static int SearchExports(IntPtr hProcess, IntPtr hModule, byte[] exports, string name)
        {
            uint num = BitConverter.ToUInt32(exports, 0x18);
            uint num2 = BitConverter.ToUInt32(exports, 0x20);
            int num3 = -1;
            if ((num > 0) && (num2 > 0))
            {
                byte[] buffer = ReadRemoteMemory(hProcess, hModule.Add((long) num2), num << 2);
                if (buffer == null)
                {
                    return num3;
                }
                uint[] numArray = new uint[num];
                for (int i = 0; i < numArray.Length; i++)
                {
                    numArray[i] = BitConverter.ToUInt32(buffer, i << 2);
                }
                int num5 = 0;
                int num6 = numArray.Length - 1;
                int index = 0;
                string strA = string.Empty;
                while (((num5 >= 0) && (num5 <= num6)) && (num3 == -1))
                {
                    index = (num5 + num6) / 2;
                    strA = ReadRemoteString(hProcess, hModule.Add((long) numArray[index]), null);
                    if (strA.Equals(name))
                    {
                        num3 = index;
                    }
                    else if (string.CompareOrdinal(strA, name) < 0)
                    {
                        num5 = index - 1;
                    }
                    else
                    {
                        num6 = index + 1;
                    }
                }
            }
            return num3;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool SetThreadContext(IntPtr hThread, ref CONTEXT pContext);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, int flAllocationType, int flProtect);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int dwFreeType);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint flOldProtect);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern uint WaitForSingleObject(IntPtr hObject, int dwTimeout);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpAddress, byte[] lpBuffer, int dwSize, out uint lpNumberOfBytesRead);

        [StructLayout(LayoutKind.Sequential)]
        public struct CONTEXT
        {
            public uint ContextFlags;
            public uint Dr0;
            public uint Dr1;
            public uint Dr2;
            public uint Dr3;
            public uint Dr6;
            public uint Dr7;
            public WinAPI.FLOATING_SAVE_AREA FloatSave;
            public uint SegGs;
            public uint SegFs;
            public uint SegEs;
            public uint SegDs;
            public uint Edi;
            public uint Esi;
            public uint Ebx;
            public uint Edx;
            public uint Ecx;
            public uint Eax;
            public uint Ebp;
            public uint Eip;
            public uint SegCs;
            public uint EFlags;
            public uint Esp;
            public uint SegSs;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x200)]
            public byte[] ExtendedRegisters;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct FLOATING_SAVE_AREA
        {
            public uint ControlWord;
            public uint StatusWord;
            public uint TagWord;
            public uint ErrorOffset;
            public uint ErrorSelector;
            public uint DataOffset;
            public uint DataSelector;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=80)]
            public byte[] RegisterArea;
            public uint Cr0NpxState;
        }
    }
}

