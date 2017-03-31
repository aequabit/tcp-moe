namespace InjectionLibrary
{
    using JLibrary.Win32;
    using System;
    using System.Diagnostics;
    using System.Threading;

    internal class ThreadHijack : StandardInjectionMethod
    {
        private static readonly byte[] REDIRECT_STUB = new byte[] { 0x9c, 0x60, 0xe8, 0, 0, 0, 0, 0x61, 0x9d, 0xe9, 0, 0, 0, 0 };

        public override IntPtr Inject(string dllPath, IntPtr hProcess)
        {
            this.ClearErrors();
            IntPtr[] ptrArray = this.InjectAll(new string[] { dllPath }, hProcess);
            if (((ptrArray != null) && ptrArray[0].IsNull()) && (this.GetLastError() == null))
            {
                this.SetLastError(new Exception("Module's entry point function reported a failure"));
            }
            return (((ptrArray != null) && (ptrArray.Length > 0)) ? ptrArray[0] : IntPtr.Zero);
        }

        public override IntPtr[] InjectAll(string[] dllPaths, IntPtr hProcess)
        {
            Exception exception;
            this.ClearErrors();
            try
            {
                if (hProcess.IsNull() || hProcess.Compare(-1L))
                {
                    throw new ArgumentException("Invalid process handle.", "hProcess");
                }
                int processId = WinAPI.GetProcessId(hProcess);
                if (processId == 0)
                {
                    throw new ArgumentException("Provided handle doesn't have sufficient permissions to inject", "hProcess");
                }
                Process processById = Process.GetProcessById(processId);
                if (processById.Threads.Count == 0)
                {
                    throw new Exception("Target process has no targetable threads to hijack.");
                }
                ProcessThread thread = SelectOptimalThread(processById);
                IntPtr ptr = WinAPI.OpenThread(0x1a, false, thread.Id);
                if (ptr.IsNull() || ptr.Compare(-1L))
                {
                    throw new Exception("Unable to obtain a handle for the remote thread.");
                }
                IntPtr zero = IntPtr.Zero;
                IntPtr lpAddress = IntPtr.Zero;
                IntPtr ptr4 = this.CreateMultiLoadStub(dllPaths, hProcess, out zero, 1);
                IntPtr[] ptrArray = null;
                if (!ptr4.IsNull())
                {
                    if (WinAPI.SuspendThread(ptr) == uint.MaxValue)
                    {
                        throw new Exception("Unable to suspend the remote thread");
                    }
                    try
                    {
                        uint lpNumberOfBytesRead = 0;
                        WinAPI.CONTEXT pContext = new WinAPI.CONTEXT {
                            ContextFlags = 0x10001
                        };
                        if (!WinAPI.GetThreadContext(ptr, ref pContext))
                        {
                            throw new InvalidOperationException("Cannot get the remote thread's context");
                        }
                        byte[] array = REDIRECT_STUB;
                        IntPtr ptr5 = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint) array.Length, 0x3000, 0x40);
                        if (ptr5.IsNull())
                        {
                            throw new InvalidOperationException("Unable to allocate memory in the remote process.");
                        }
                        BitConverter.GetBytes(ptr4.Subtract(ptr5.Add(((long) 7L))).ToInt32()).CopyTo(array, 3);
                        BitConverter.GetBytes((uint) (pContext.Eip - ((uint) ptr5.Add(((long) array.Length)).ToInt32()))).CopyTo(array, (int) (array.Length - 4));
                        if (!(WinAPI.WriteProcessMemory(hProcess, ptr5, array, array.Length, out lpNumberOfBytesRead) && (lpNumberOfBytesRead == array.Length)))
                        {
                            throw new InvalidOperationException("Unable to write stub to the remote process.");
                        }
                        pContext.Eip = (uint) ptr5.ToInt32();
                        WinAPI.SetThreadContext(ptr, ref pContext);
                    }
                    catch (Exception exception1)
                    {
                        exception = exception1;
                        this.SetLastError(exception);
                        ptrArray = null;
                        WinAPI.VirtualFreeEx(hProcess, zero, 0, 0x8000);
                        WinAPI.VirtualFreeEx(hProcess, ptr4, 0, 0x8000);
                        WinAPI.VirtualFreeEx(hProcess, lpAddress, 0, 0x8000);
                    }
                    WinAPI.ResumeThread(ptr);
                    if (this.GetLastError() == null)
                    {
                        Thread.Sleep(100);
                        ptrArray = new IntPtr[dllPaths.Length];
                        byte[] buffer2 = WinAPI.ReadRemoteMemory(hProcess, zero, ((uint) dllPaths.Length) << 2);
                        if (buffer2 != null)
                        {
                            for (int i = 0; i < ptrArray.Length; i++)
                            {
                                ptrArray[i] = Win32Ptr.Create((long) BitConverter.ToInt32(buffer2, i << 2));
                            }
                        }
                    }
                    WinAPI.CloseHandle(ptr);
                }
                return ptrArray;
            }
            catch (Exception exception2)
            {
                exception = exception2;
                this.SetLastError(exception);
                return null;
            }
        }

        private static ProcessThread SelectOptimalThread(Process target)
        {
            return target.Threads[0];
        }
    }
}

