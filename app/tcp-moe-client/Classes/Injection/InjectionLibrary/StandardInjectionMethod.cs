namespace InjectionLibrary
{
    using JLibrary.PortableExecutable;
    using JLibrary.Tools;
    using JLibrary.Win32;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal abstract class StandardInjectionMethod : InjectionMethod
    {
        protected static readonly byte[] MULTILOAD_STUB = new byte[] { 
            0x55, 0x8b, 0xec, 0x83, 0xec, 12, 0xb9, 0, 0, 0, 0, 0x89, 12, 0x24, 0xb9, 0, 
            0, 0, 0, 0x89, 0x4c, 0x24, 4, 0xb9, 0, 0, 0, 0, 0x89, 0x4c, 0x24, 8, 
            0x8b, 0x4c, 0x24, 4, 0x83, 0xf9, 0, 0x74, 0x3a, 0x83, 0xe9, 1, 0x89, 0x4c, 0x24, 4, 
            0xff, 0x34, 0x24, 0xe8, 0, 0, 0, 0, 0x83, 0xf8, 0, 0x75, 8, 0xff, 0x34, 0x24, 
            0xe8, 0, 0, 0, 0, 0x8b, 0x4c, 0x24, 8, 0x89, 1, 0x83, 0xc1, 4, 0x89, 0x4c, 
            0x24, 8, 0x8b, 12, 0x24, 0x8a, 1, 0x83, 0xc1, 1, 60, 0, 0x75, 0xf7, 0x89, 12, 
            0x24, 0xeb, 0xbd, 0x8b, 0xe5, 0x5d, 0xc3
         };
        protected static readonly byte[] MULTIUNLOAD_STUB = new byte[] { 
            0x55, 0x8b, 0xec, 0x83, 0xec, 12, 0xb9, 0, 0, 0, 0, 0x89, 12, 0x24, 0xb9, 0, 
            0, 0, 0, 0x89, 0x4c, 0x24, 4, 0x8b, 12, 0x24, 0x8b, 9, 0x83, 0xf9, 0, 0x74, 
            0x3a, 0x89, 0x4c, 0x24, 8, 0x8b, 0x4c, 0x24, 4, 0xc7, 1, 0, 0, 0, 0, 0xff, 
            0x74, 0x24, 8, 0xe8, 0, 0, 0, 0, 0x83, 0xf8, 0, 0x74, 8, 0x8b, 0x4c, 0x24, 
            4, 0x89, 1, 0xeb, 0xea, 0x8b, 12, 0x24, 0x83, 0xc1, 4, 0x89, 12, 0x24, 0x8b, 0x4c, 
            0x24, 4, 0x83, 0xc1, 4, 0x89, 0x4c, 0x24, 4, 0xeb, 0xbc, 0x8b, 0xe5, 0x5d, 0xc3
         };

        protected StandardInjectionMethod()
        {
        }

        protected virtual IntPtr CreateMultiLoadStub(string[] paths, IntPtr hProcess, out IntPtr pModuleBuffer, uint nullmodule = 0)
        {
            IntPtr ptr6;
            pModuleBuffer = IntPtr.Zero;
            IntPtr zero = IntPtr.Zero;
            try
            {
                IntPtr moduleHandleA = WinAPI.GetModuleHandleA("kernel32.dll");
                IntPtr procAddress = WinAPI.GetProcAddress(moduleHandleA, "LoadLibraryA");
                IntPtr ptr = WinAPI.GetProcAddress(moduleHandleA, "GetModuleHandleA");
                if (procAddress.IsNull() || ptr.IsNull())
                {
                    throw new Exception("Unable to find necessary function entry points in the remote process");
                }
                pModuleBuffer = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, ((uint) paths.Length) << 2, 0x3000, 4);
                IntPtr ptr5 = WinAPI.CreateRemotePointer(hProcess, Encoding.ASCII.GetBytes(string.Join("\0", paths) + "\0"), 4);
                if (pModuleBuffer.IsNull() || ptr5.IsNull())
                {
                    throw new InvalidOperationException("Unable to allocate memory in the remote process");
                }
                try
                {
                    uint lpNumberOfBytesRead = 0;
                    byte[] array = new byte[paths.Length << 2];
                    for (int i = 0; i < (array.Length >> 2); i++)
                    {
                        BitConverter.GetBytes(nullmodule).CopyTo(array, (int) (i << 2));
                    }
                    WinAPI.WriteProcessMemory(hProcess, pModuleBuffer, array, array.Length, out lpNumberOfBytesRead);
                    byte[] buffer2 = (byte[]) MULTILOAD_STUB.Clone();
                    zero = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint) buffer2.Length, 0x3000, 0x40);
                    if (zero.IsNull())
                    {
                        throw new InvalidOperationException("Unable to allocate memory in the remote process");
                    }
                    BitConverter.GetBytes(ptr5.ToInt32()).CopyTo(buffer2, 7);
                    BitConverter.GetBytes(paths.Length).CopyTo(buffer2, 15);
                    BitConverter.GetBytes(pModuleBuffer.ToInt32()).CopyTo(buffer2, 0x18);
                    BitConverter.GetBytes(ptr.Subtract(zero.Add(((long) 0x38L))).ToInt32()).CopyTo(buffer2, 0x34);
                    BitConverter.GetBytes(procAddress.Subtract(zero.Add(((long) 0x45L))).ToInt32()).CopyTo(buffer2, 0x41);
                    if (!(WinAPI.WriteProcessMemory(hProcess, zero, buffer2, buffer2.Length, out lpNumberOfBytesRead) && (lpNumberOfBytesRead == buffer2.Length)))
                    {
                        throw new Exception("Error creating the remote function stub.");
                    }
                    ptr6 = zero;
                }
                finally
                {
                    WinAPI.VirtualFreeEx(hProcess, pModuleBuffer, 0, 0x8000);
                    WinAPI.VirtualFreeEx(hProcess, ptr5, 0, 0x8000);
                    if (!zero.IsNull())
                    {
                        WinAPI.VirtualFreeEx(hProcess, zero, 0, 0x8000);
                    }
                    pModuleBuffer = IntPtr.Zero;
                }
            }
            catch (Exception exception)
            {
                this.SetLastError(exception);
                ptr6 = IntPtr.Zero;
            }
            return ptr6;
        }

        public override IntPtr Inject(JLibrary.PortableExecutable.PortableExecutable dll, IntPtr hProcess)
        {
            this.ClearErrors();
            string str = Utils.WriteTempData(dll.ToArray());
            IntPtr zero = IntPtr.Zero;
            if (!string.IsNullOrEmpty(str))
            {
                zero = this.Inject(str, hProcess);
                try
                {
                    File.Delete(str);
                }
                catch
                {
                }
            }
            return zero;
        }

        public override IntPtr[] InjectAll(JLibrary.PortableExecutable.PortableExecutable[] dlls, IntPtr hProcess)
        {
            this.ClearErrors();
            return this.InjectAll(Array.ConvertAll<JLibrary.PortableExecutable.PortableExecutable, string>(dlls, pe => Utils.WriteTempData(pe.ToArray())), hProcess);
        }

        public override bool Unload(IntPtr hModule, IntPtr hProcess)
        {
            this.ClearErrors();
            if (hProcess.IsNull() || hProcess.Compare(-1L))
            {
                throw new ArgumentOutOfRangeException("hProcess", "Invalid process handle specified.");
            }
            if (hModule.IsNull())
            {
                throw new ArgumentNullException("hModule", "Invalid module handle");
            }
            try
            {
                IntPtr[] hModules = new IntPtr[] { hModule };
                bool[] flagArray = this.UnloadAll(hModules, hProcess);
                return (((flagArray != null) && (flagArray.Length > 0)) ? flagArray[0] : false);
            }
            catch (Exception exception)
            {
                this.SetLastError(exception);
                return false;
            }
        }

        public override bool[] UnloadAll(IntPtr[] hModules, IntPtr hProcess)
        {
            bool[] flagArray2;
            this.ClearErrors();
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            IntPtr ptr3 = IntPtr.Zero;
            try
            {
                int num2;
                uint lpNumberOfBytesRead = 0;
                IntPtr procAddress = WinAPI.GetProcAddress(WinAPI.GetModuleHandleA("kernel32.dll"), "FreeLibrary");
                if (procAddress.IsNull())
                {
                    throw new Exception("Unable to find necessary function entry points in the remote process");
                }
                zero = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, ((uint) hModules.Length) << 2, 0x3000, 4);
                ptr = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint) ((hModules.Length + 1) << 2), 0x3000, 4);
                ptr3 = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint) MULTIUNLOAD_STUB.Length, 0x3000, 0x40);
                if ((zero.IsNull() || ptr.IsNull()) || ptr3.IsNull())
                {
                    throw new InvalidOperationException("Unable to allocate memory in the remote process");
                }
                byte[] array = new byte[(hModules.Length + 1) << 2];
                for (num2 = 0; num2 < hModules.Length; num2++)
                {
                    BitConverter.GetBytes(hModules[num2].ToInt32()).CopyTo(array, (int) (num2 << 2));
                }
                WinAPI.WriteProcessMemory(hProcess, ptr, array, array.Length, out lpNumberOfBytesRead);
                byte[] buffer2 = (byte[]) MULTIUNLOAD_STUB.Clone();
                BitConverter.GetBytes(ptr.ToInt32()).CopyTo(buffer2, 7);
                BitConverter.GetBytes(zero.ToInt32()).CopyTo(buffer2, 15);
                BitConverter.GetBytes(procAddress.Subtract(ptr3.Add(((long) 0x38L))).ToInt32()).CopyTo(buffer2, 0x34);
                if (!(WinAPI.WriteProcessMemory(hProcess, ptr3, buffer2, buffer2.Length, out lpNumberOfBytesRead) && (lpNumberOfBytesRead == buffer2.Length)))
                {
                    throw new InvalidOperationException("Unable to write the function stub to the remote process.");
                }
                if (WinAPI.RunThread(hProcess, ptr3, 0, 0x3e8) == uint.MaxValue)
                {
                    throw new InvalidOperationException("Error occurred when running remote function stub.");
                }
                byte[] buffer3 = WinAPI.ReadRemoteMemory(hProcess, zero, ((uint) hModules.Length) << 2);
                if (buffer3 == null)
                {
                    throw new Exception("Unable to read results from the remote process.");
                }
                bool[] flagArray = new bool[hModules.Length];
                for (num2 = 0; num2 < flagArray.Length; num2++)
                {
                    flagArray[num2] = BitConverter.ToInt32(buffer3, num2 << 2) != 0;
                }
                flagArray2 = flagArray;
            }
            catch (Exception exception)
            {
                this.SetLastError(exception);
                flagArray2 = null;
            }
            finally
            {
                WinAPI.VirtualFreeEx(hProcess, ptr3, 0, 0x8000);
                WinAPI.VirtualFreeEx(hProcess, zero, 0, 0x8000);
                WinAPI.VirtualFreeEx(hProcess, ptr, 0, 0x8000);
            }
            return flagArray2;
        }
    }
}

