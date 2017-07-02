namespace InjectionLibrary
{
    using JLibrary.Win32;
    using System;
    using System.Text;

    internal class CRTInjection : StandardInjectionMethod
    {
        public override IntPtr Inject(string dllPath, IntPtr hProcess)
        {
            this.ClearErrors();
            if (hProcess.IsNull() || hProcess.Compare(-1L))
            {
                throw new ArgumentOutOfRangeException("hProcess", "Invalid process handle specified.");
            }
            try
            {
                IntPtr zero = IntPtr.Zero;
                IntPtr procAddress = WinAPI.GetProcAddress(WinAPI.GetModuleHandleA("kernel32.dll"), "LoadLibraryW");
                if (procAddress.IsNull())
                {
                    throw new Exception("Unable to locate the LoadLibraryW entry point");
                }
                IntPtr ptr = WinAPI.CreateRemotePointer(hProcess, Encoding.Unicode.GetBytes(dllPath + "\0"), 4);
                if (ptr.IsNull())
                {
                    throw new InvalidOperationException("Failed to allocate memory in the remote process");
                }
                try
                {
                    uint num = WinAPI.RunThread(hProcess, procAddress, (uint) ptr.ToInt32(), 0x2710);
                    switch (num)
                    {
                        case uint.MaxValue:
                            throw new Exception("Error occurred when calling function in the remote process");

                        case 0:
                            throw new Exception("Failed to load module into remote process. Error code: " + WinAPI.GetLastErrorEx(hProcess).ToString());
                    }
                    zero = Win32Ptr.Create((long) num);
                }
                finally
                {
                    WinAPI.VirtualFreeEx(hProcess, ptr, 0, 0x8000);
                }
                return zero;
            }
            catch (Exception exception)
            {
                this.SetLastError(exception);
                return IntPtr.Zero;
            }
        }

        public override IntPtr[] InjectAll(string[] dllPaths, IntPtr hProcess)
        {
            this.ClearErrors();
            if (hProcess.IsNull() || hProcess.Compare(-1L))
            {
                throw new ArgumentOutOfRangeException("hProcess", "Invalid process handle specified.");
            }
            try
            {
                IntPtr zero = IntPtr.Zero;
                IntPtr ptr = this.CreateMultiLoadStub(dllPaths, hProcess, out zero, 0);
                IntPtr[] ptrArray = null;
                if (!ptr.IsNull())
                {
                    try
                    {
                        if (WinAPI.RunThread(hProcess, ptr, 0, 0x2710) == uint.MaxValue)
                        {
                            throw new Exception("Error occurred while executing remote thread.");
                        }
                        byte[] buffer = WinAPI.ReadRemoteMemory(hProcess, zero, ((uint) dllPaths.Length) << 2);
                        if (buffer == null)
                        {
                            throw new InvalidOperationException("Unable to read from the remote process.");
                        }
                        ptrArray = new IntPtr[dllPaths.Length];
                        for (int i = 0; i < ptrArray.Length; i++)
                        {
                            ptrArray[i] = new IntPtr(BitConverter.ToInt32(buffer, i << 2));
                        }
                    }
                    finally
                    {
                        WinAPI.VirtualFreeEx(hProcess, zero, 0, 0x8000);
                        WinAPI.VirtualFreeEx(hProcess, ptr, 0, 0x8000);
                    }
                }
                return ptrArray;
            }
            catch (Exception exception)
            {
                this.SetLastError(exception);
                return null;
            }
        }
    }
}

