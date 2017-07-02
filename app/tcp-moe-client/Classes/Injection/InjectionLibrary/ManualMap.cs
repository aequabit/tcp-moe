namespace InjectionLibrary
{
    using JLibrary.PortableExecutable;
    using JLibrary.Tools;
    using JLibrary.Win32;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class ManualMap : InjectionMethod
    {
        private static readonly byte[] DLLMAIN_STUB = new byte[] { 
            0x68, 0, 0, 0, 0, 0x68, 1, 0, 0, 0, 0x68, 0, 0, 0, 0, 0xff, 
            0x54, 0x24, 0x10, 0xc3
         };
        private static readonly IntPtr FN_ACTIVATEACTCTX = WinAPI.GetProcAddress(H_KERNEL32, "ActivateActCtx");
        private static readonly IntPtr FN_CREATEACTCTXA = WinAPI.GetProcAddress(H_KERNEL32, "CreateActCtxA");
        private static readonly IntPtr FN_DEACTIVATEACTCTX = WinAPI.GetProcAddress(H_KERNEL32, "DeactivateActCtx");
        private static readonly IntPtr FN_GETMODULEHANDLEA = WinAPI.GetProcAddress(H_KERNEL32, "GetModuleHandleA");
        private static readonly IntPtr FN_LOADLIBRARYA = WinAPI.GetProcAddress(H_KERNEL32, "LoadLibraryA");
        private static readonly IntPtr FN_RELEASEACTCTX = WinAPI.GetProcAddress(H_KERNEL32, "ReleaseActCtx");
        private static readonly IntPtr H_KERNEL32 = WinAPI.GetModuleHandleA("KERNEL32.dll");
        private static readonly byte[] RESOLVER_STUB = new byte[] { 
            0x55, 0x8b, 0xec, 0x83, 0xec, 60, 0x8b, 0xcc, 0x8b, 0xd1, 0x83, 0xc2, 60, 0xc7, 1, 0, 
            0, 0, 0, 0x83, 0xc1, 4, 0x3b, 0xca, 0x7e, 0xf3, 0xc6, 4, 0x24, 0x20, 0xb9, 0, 
            0, 0, 0, 0x89, 0x4c, 0x24, 8, 0xb9, 0, 0, 0, 0, 0x89, 0x4c, 0x24, 40, 
            0xb9, 0, 0, 0, 0, 0x89, 0x4c, 0x24, 0x2c, 0x54, 0xe8, 0, 0, 0, 0, 0x83, 
            0x38, 0xff, 15, 0x84, 0x89, 0, 0, 0, 0x89, 0x44, 0x24, 0x30, 0x8b, 0xcc, 0x83, 0xc1, 
            0x20, 0x51, 80, 0xe8, 0, 0, 0, 0, 0x83, 0xf8, 0, 0x74, 0x6b, 0xc6, 0x44, 0x24, 
            0x24, 1, 0x8b, 0x4c, 0x24, 40, 0x83, 0xf9, 0, 0x7e, 0x3e, 0x83, 0xe9, 1, 0x89, 0x4c, 
            0x24, 40, 0x8b, 0x4c, 0x24, 0x24, 0x83, 0xf9, 0, 0x74, 0x2e, 0xff, 0x74, 0x24, 0x2c, 0xe8, 
            0, 0, 0, 0, 0x83, 0xf8, 0, 0x75, 9, 0xff, 0x74, 0x24, 0x2c, 0xe8, 0, 0, 
            0, 0, 0x89, 0x44, 0x24, 0x24, 0x8b, 0x4c, 0x24, 0x2c, 0x8a, 1, 0x83, 0xc1, 1, 60, 
            0, 0x75, 0xf7, 0x89, 0x4c, 0x24, 0x2c, 0xeb, 0xb9, 0x8b, 0x44, 0x24, 0x24, 0xb9, 1, 0, 
            0, 0, 0x23, 0xc1, 0x89, 0x4c, 0x24, 0x24, 0x83, 0xf9, 0, 0x75, 20, 0xff, 0x74, 0x24, 
            0x20, 0x6a, 0, 0xe8, 0, 0, 0, 0, 0xff, 0x74, 0x24, 0x30, 0xe8, 0, 0, 0, 
            0, 0x8b, 0x44, 0x24, 0x24, 0x8b, 0xe5, 0x5d, 0xc3
         };

        private static byte[] ExtractManifest(JLibrary.PortableExecutable.PortableExecutable image)
        {
            byte[] data = null;
            ResourceWalker walker = new ResourceWalker(image);
            ResourceWalker.ResourceDirectory directory = null;
            for (int i = 0; (i < walker.Root.Directories.Length) && (directory == null); i++)
            {
                if (walker.Root.Directories[i].Id == 0x18L)
                {
                    directory = walker.Root.Directories[i];
                }
            }
            if (((directory != null) && (directory.Directories.Length > 0)) && (IsManifestResource(directory.Directories[0].Id) && (directory.Directories[0].Files.Length == 1)))
            {
                data = directory.Directories[0].Files[0].GetData();
            }
            return data;
        }

        private static uint FindEntryPoint(IntPtr hProcess, IntPtr hModule)
        {
            if (hProcess.IsNull() || hProcess.Compare(-1L))
            {
                throw new ArgumentException("Invalid process handle.", "hProcess");
            }
            if (hModule.IsNull())
            {
                throw new ArgumentException("Invalid module handle.", "hModule");
            }
            byte[] buffer = WinAPI.ReadRemoteMemory(hProcess, hModule, (uint) Marshal.SizeOf(typeof(IMAGE_DOS_HEADER)));
            if (buffer != null)
            {
                ushort num = BitConverter.ToUInt16(buffer, 0);
                uint num2 = BitConverter.ToUInt32(buffer, 60);
                if (num == 0x5a4d)
                {
                    byte[] buffer2 = WinAPI.ReadRemoteMemory(hProcess, hModule.Add((long) num2), (uint) Marshal.SizeOf(typeof(IMAGE_NT_HEADER32)));
                    if ((buffer2 != null) && (BitConverter.ToUInt32(buffer2, 0) == 0x4550))
                    {
                        IMAGE_NT_HEADER32 result = new IMAGE_NT_HEADER32();
                        using (UnmanagedBuffer buffer3 = new UnmanagedBuffer(0x100))
                        {
                            if (buffer3.Translate<IMAGE_NT_HEADER32>(buffer2, out result))
                            {
                                return result.OptionalHeader.AddressOfEntryPoint;
                            }
                        }
                    }
                }
            }
            return 0;
        }

        private static IntPtr GetRemoteModuleHandle(string module, int processId)
        {
            IntPtr zero = IntPtr.Zero;
            Process processById = Process.GetProcessById(processId);
            for (int i = 0; (i < processById.Modules.Count) && zero.IsNull(); i++)
            {
                if (processById.Modules[i].ModuleName.ToLower() == module.ToLower())
                {
                    zero = processById.Modules[i].BaseAddress;
                }
            }
            return zero;
        }

        public override IntPtr Inject(JLibrary.PortableExecutable.PortableExecutable image, IntPtr hProcess)
        {
            this.ClearErrors();
            try
            {
                return MapModule(Utils.DeepClone<JLibrary.PortableExecutable.PortableExecutable>(image), hProcess, true);
            }
            catch (Exception exception)
            {
                this.SetLastError(exception);
                return IntPtr.Zero;
            }
        }

        public override IntPtr Inject(string dllPath, IntPtr hProcess)
        {
            IntPtr zero;
            this.ClearErrors();
            try
            {
                using (JLibrary.PortableExecutable.PortableExecutable executable = new JLibrary.PortableExecutable.PortableExecutable(dllPath))
                {
                    zero = this.Inject(executable, hProcess);
                }
            }
            catch (Exception exception)
            {
                this.SetLastError(exception);
                zero = IntPtr.Zero;
            }
            return zero;
        }

        public override IntPtr[] InjectAll(JLibrary.PortableExecutable.PortableExecutable[] images, IntPtr hProcess)
        {
            this.ClearErrors();
            return Array.ConvertAll<JLibrary.PortableExecutable.PortableExecutable, IntPtr>(images, pe => this.Inject(pe, hProcess));
        }

        public override IntPtr[] InjectAll(string[] dllPaths, IntPtr hProcess)
        {
            this.ClearErrors();
            return Array.ConvertAll<string, IntPtr>(dllPaths, dp => this.Inject(dp, hProcess));
        }

        private static bool IsManifestResource(int id)
        {
            switch (((uint) id))
            {
                case 1:
                case 2:
                case 3:
                    return true;
            }
            return false;
        }

        private static bool LoadDependencies(JLibrary.PortableExecutable.PortableExecutable image, IntPtr hProcess, int processId)
        {
            List<string> list = new List<string>();
            string lpBuffer = string.Empty;
            bool flag = false;
            foreach (IMAGE_IMPORT_DESCRIPTOR image_import_descriptor in image.EnumImports())
            {
                if ((image.ReadString((long) image.GetPtrFromRVA(image_import_descriptor.Name), SeekOrigin.Begin, out lpBuffer, -1, null) && !string.IsNullOrEmpty(lpBuffer)) && GetRemoteModuleHandle(lpBuffer, processId).IsNull())
                {
                    list.Add(lpBuffer);
                }
            }
            if (list.Count > 0)
            {
                byte[] data = ExtractManifest(image);
                string str2 = string.Empty;
                if (data == null)
                {
                    if (string.IsNullOrEmpty(image.FileLocation) || !File.Exists(Path.Combine(Path.GetDirectoryName(image.FileLocation), Path.GetFileName(image.FileLocation) + ".manifest")))
                    {
                        IntPtr[] ptrArray = InjectionMethod.Create(InjectionMethodType.Standard).InjectAll(list.ToArray(), hProcess);
                        foreach (IntPtr ptr in ptrArray)
                        {
                            if (ptr.IsNull())
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    str2 = Path.Combine(Path.GetDirectoryName(image.FileLocation), Path.GetFileName(image.FileLocation) + ".manifest");
                }
                else
                {
                    str2 = Utils.WriteTempData(data);
                }
                if (string.IsNullOrEmpty(str2))
                {
                    return false;
                }
                IntPtr ptr2 = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint) RESOLVER_STUB.Length, 0x3000, 0x40);
                IntPtr lpAddress = WinAPI.CreateRemotePointer(hProcess, Encoding.ASCII.GetBytes(str2 + "\0"), 4);
                IntPtr ptr4 = WinAPI.CreateRemotePointer(hProcess, Encoding.ASCII.GetBytes(string.Join("\0", list.ToArray()) + "\0"), 4);
                if (!ptr2.IsNull())
                {
                    byte[] array = (byte[]) RESOLVER_STUB.Clone();
                    uint lpNumberOfBytesRead = 0;
                    BitConverter.GetBytes(FN_CREATEACTCTXA.Subtract(ptr2.Add(((long) 0x3fL))).ToInt32()).CopyTo(array, 0x3b);
                    BitConverter.GetBytes(FN_ACTIVATEACTCTX.Subtract(ptr2.Add(((long) 0x58L))).ToInt32()).CopyTo(array, 0x54);
                    BitConverter.GetBytes(FN_GETMODULEHANDLEA.Subtract(ptr2.Add(((long) 0x84L))).ToInt32()).CopyTo(array, 0x80);
                    BitConverter.GetBytes(FN_LOADLIBRARYA.Subtract(ptr2.Add(((long) 0x92L))).ToInt32()).CopyTo(array, 0x8e);
                    BitConverter.GetBytes(FN_DEACTIVATEACTCTX.Subtract(ptr2.Add(((long) 200L))).ToInt32()).CopyTo(array, 0xc4);
                    BitConverter.GetBytes(FN_RELEASEACTCTX.Subtract(ptr2.Add(((long) 0xd1L))).ToInt32()).CopyTo(array, 0xcd);
                    BitConverter.GetBytes(lpAddress.ToInt32()).CopyTo(array, 0x1f);
                    BitConverter.GetBytes(list.Count).CopyTo(array, 40);
                    BitConverter.GetBytes(ptr4.ToInt32()).CopyTo(array, 0x31);
                    if (WinAPI.WriteProcessMemory(hProcess, ptr2, array, array.Length, out lpNumberOfBytesRead) && (lpNumberOfBytesRead == array.Length))
                    {
                        uint num2 = WinAPI.RunThread(hProcess, ptr2, 0, 0x1388);
                        flag = (num2 != uint.MaxValue) && (num2 != 0);
                    }
                    WinAPI.VirtualFreeEx(hProcess, ptr4, 0, 0x8000);
                    WinAPI.VirtualFreeEx(hProcess, lpAddress, 0, 0x8000);
                    WinAPI.VirtualFreeEx(hProcess, ptr2, 0, 0x8000);
                }
            }
            return flag;
        }

        private static IntPtr MapModule(JLibrary.PortableExecutable.PortableExecutable image, IntPtr hProcess, bool preserveHeaders = false)
        {
            if (hProcess.IsNull() || hProcess.Compare(-1L))
            {
                throw new ArgumentException("Invalid process handle.", "hProcess");
            }
            if (image == null)
            {
                throw new ArgumentException("Cannot map a non-existant PE Image.", "image");
            }
            int processId = WinAPI.GetProcessId(hProcess);
            if (processId == 0)
            {
                throw new ArgumentException("Provided handle doesn't have sufficient permissions to inject", "hProcess");
            }
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            uint lpNumberOfBytesRead = 0;
            try
            {
                zero = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, image.NTHeader.OptionalHeader.SizeOfImage, 0x3000, 4);
                if (zero.IsNull())
                {
                    throw new InvalidOperationException("Unable to allocate memory in the remote process.");
                }
                PatchRelocations(image, zero);
                LoadDependencies(image, hProcess, processId);
                PatchImports(image, hProcess, processId);
                if (preserveHeaders)
                {
                    long num3 = (long) (((image.DOSHeader.e_lfanew + Marshal.SizeOf(typeof(IMAGE_FILE_HEADER))) + ((long) 4L)) + image.NTHeader.FileHeader.SizeOfOptionalHeader);
                    byte[] buffer = new byte[num3];
                    if (image.Read(0L, SeekOrigin.Begin, buffer))
                    {
                        WinAPI.WriteProcessMemory(hProcess, zero, buffer, buffer.Length, out lpNumberOfBytesRead);
                    }
                }
                MapSections(image, hProcess, zero);
                if (image.NTHeader.OptionalHeader.AddressOfEntryPoint <= 0)
                {
                    return zero;
                }
                byte[] array = (byte[]) DLLMAIN_STUB.Clone();
                BitConverter.GetBytes(zero.ToInt32()).CopyTo(array, 11);
                ptr = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint) DLLMAIN_STUB.Length, 0x3000, 0x40);
                if (ptr.IsNull() || (!WinAPI.WriteProcessMemory(hProcess, ptr, array, array.Length, out lpNumberOfBytesRead) || (lpNumberOfBytesRead != array.Length)))
                {
                    throw new InvalidOperationException("Unable to write stub to the remote process.");
                }
                IntPtr hObject = WinAPI.CreateRemoteThread(hProcess, 0, 0, ptr, (uint) zero.Add(((long) image.NTHeader.OptionalHeader.AddressOfEntryPoint)).ToInt32(), 0, 0);
                if (WinAPI.WaitForSingleObject(hObject, 0x1388) != 0L)
                {
                    return zero;
                }
                WinAPI.GetExitCodeThread(hObject, out lpNumberOfBytesRead);
                if (lpNumberOfBytesRead == 0)
                {
                    WinAPI.VirtualFreeEx(hProcess, zero, 0, 0x8000);
                    throw new Exception("Entry method of module reported a failure " + Marshal.GetLastWin32Error().ToString());
                }
                WinAPI.VirtualFreeEx(hProcess, ptr, 0, 0x8000);
                WinAPI.CloseHandle(hObject);
            }
            catch (Exception exception)
            {
                if (!zero.IsNull())
                {
                    WinAPI.VirtualFreeEx(hProcess, zero, 0, 0x8000);
                }
                if (!ptr.IsNull())
                {
                    WinAPI.VirtualFreeEx(hProcess, zero, 0, 0x8000);
                }
                zero = IntPtr.Zero;
                throw exception;
            }
            return zero;
        }

        private static void MapSections(JLibrary.PortableExecutable.PortableExecutable image, IntPtr hProcess, IntPtr pModule)
        {
            foreach (IMAGE_SECTION_HEADER image_section_header in image.EnumSectionHeaders())
            {
                byte[] buffer = new byte[image_section_header.SizeOfRawData];
                if (!image.Read((long) image_section_header.PointerToRawData, SeekOrigin.Begin, buffer))
                {
                    throw image.GetLastError();
                }
                if ((image_section_header.Characteristics & 0x2000000) == 0)
                {
                    uint num;
                    WinAPI.WriteProcessMemory(hProcess, pModule.Add((long) image_section_header.VirtualAddress), buffer, buffer.Length, out num);
                    IntPtr lpAddress = pModule.Add((long) image_section_header.VirtualAddress);
                    WinAPI.VirtualProtectEx(hProcess, lpAddress, image_section_header.SizeOfRawData, image_section_header.Characteristics & 0xffffff, out num);
                }
            }
        }

        private static void PatchImports(JLibrary.PortableExecutable.PortableExecutable image, IntPtr hProcess, int processId)
        {
            string lpBuffer = string.Empty;
            string str2 = string.Empty;
            foreach (IMAGE_IMPORT_DESCRIPTOR image_import_descriptor in image.EnumImports())
            {
                if (image.ReadString((long) image.GetPtrFromRVA(image_import_descriptor.Name), SeekOrigin.Begin, out lpBuffer, -1, null))
                {
                    IMAGE_THUNK_DATA image_thunk_data;
                    IntPtr zero = IntPtr.Zero;
                    zero = GetRemoteModuleHandle(lpBuffer, processId);
                    if (zero.IsNull())
                    {
                        throw new FileNotFoundException(string.Format("Unable to load dependent module '{0}'.", lpBuffer));
                    }
                    uint ptrFromRVA = image.GetPtrFromRVA(image_import_descriptor.FirstThunkPtr);
                    uint num2 = (uint) Marshal.SizeOf(typeof(IMAGE_THUNK_DATA));
                    while (image.Read<IMAGE_THUNK_DATA>((long) ptrFromRVA, SeekOrigin.Begin, out image_thunk_data) && (image_thunk_data.u1.AddressOfData > 0))
                    {
                        IntPtr hModule = IntPtr.Zero;
                        object lpProcName = null;
                        if ((image_thunk_data.u1.Ordinal & 0x80000000) == 0)
                        {
                            if (!image.ReadString((long) (image.GetPtrFromRVA(image_thunk_data.u1.AddressOfData) + 2), SeekOrigin.Begin, out str2, -1, null))
                            {
                                throw image.GetLastError();
                            }
                            lpProcName = str2;
                        }
                        else
                        {
                            lpProcName = (ushort) (image_thunk_data.u1.Ordinal & 0xffff);
                        }
                        if (!(hModule = WinAPI.GetModuleHandleA(lpBuffer)).IsNull())
                        {
                            IntPtr ptr = lpProcName.GetType().Equals(typeof(string)) ? WinAPI.GetProcAddress(hModule, (string) lpProcName) : WinAPI.GetProcAddress(hModule, (uint) (((ushort) lpProcName) & 0xffff));
                            if (!ptr.IsNull())
                            {
                                hModule = zero.Add((long) ptr.Subtract(((long) hModule.ToInt32())).ToInt32());
                            }
                        }
                        else
                        {
                            hModule = WinAPI.GetProcAddressEx(hProcess, zero, lpProcName);
                        }
                        if (hModule.IsNull())
                        {
                            throw new EntryPointNotFoundException(string.Format("Unable to locate imported function '{0}' from module '{1}' in the remote process.", str2, lpBuffer));
                        }
                        image.Write<int>((long) ptrFromRVA, SeekOrigin.Begin, hModule.ToInt32());
                        ptrFromRVA += num2;
                    }
                }
            }
        }

        private static void PatchRelocations(JLibrary.PortableExecutable.PortableExecutable image, IntPtr pAlloc)
        {
            IMAGE_DATA_DIRECTORY image_data_directory = image.NTHeader.OptionalHeader.DataDirectory[5];
            if (image_data_directory.Size > 0)
            {
                IMAGE_BASE_RELOCATION image_base_relocation;
                uint num = 0;
                uint num2 = ((uint) pAlloc.ToInt32()) - image.NTHeader.OptionalHeader.ImageBase;
                uint ptrFromRVA = image.GetPtrFromRVA(image_data_directory.VirtualAddress);
                uint num4 = (uint) Marshal.SizeOf(typeof(IMAGE_BASE_RELOCATION));
                while ((num < image_data_directory.Size) && image.Read<IMAGE_BASE_RELOCATION>((long) ptrFromRVA, SeekOrigin.Begin, out image_base_relocation))
                {
                    int num5 = (int) ((image_base_relocation.SizeOfBlock - num4) / 2);
                    uint num6 = image.GetPtrFromRVA(image_base_relocation.VirtualAddress);
                    for (int i = 0; i < num5; i++)
                    {
                        ushort num7;
                        if (image.Read<ushort>((ptrFromRVA + num4) + (i << 1), SeekOrigin.Begin, out num7) && (((num7 >> 12) & 3) != 0))
                        {
                            uint num8;
                            uint num10 = num6 + ((uint) (num7 & 0xfff));
                            if (!image.Read<uint>((long) num10, SeekOrigin.Begin, out num8))
                            {
                                throw image.GetLastError();
                            }
                            image.Write<uint>(-4L, SeekOrigin.Current, num8 + num2);
                        }
                    }
                    num += image_base_relocation.SizeOfBlock;
                    ptrFromRVA += image_base_relocation.SizeOfBlock;
                }
            }
        }

        public override bool Unload(IntPtr hModule, IntPtr hProcess)
        {
            this.ClearErrors();
            if (hModule.IsNull())
            {
                throw new ArgumentNullException("hModule", "Invalid module handle");
            }
            if (hProcess.IsNull() || hProcess.Compare(-1L))
            {
                throw new ArgumentException("Invalid process handle.", "hProcess");
            }
            IntPtr zero = IntPtr.Zero;
            uint lpNumberOfBytesRead = 0;
            try
            {
                uint num2 = FindEntryPoint(hProcess, hModule);
                if (num2 != 0)
                {
                    byte[] array = (byte[]) DLLMAIN_STUB.Clone();
                    BitConverter.GetBytes(hModule.ToInt32()).CopyTo(array, 11);
                    BitConverter.GetBytes((uint) 0).CopyTo(array, 6);
                    BitConverter.GetBytes((uint) 0x3e8).CopyTo(array, 1);
                    zero = WinAPI.VirtualAllocEx(hProcess, IntPtr.Zero, (uint) DLLMAIN_STUB.Length, 0x3000, 0x40);
                    if (zero.IsNull() || (!WinAPI.WriteProcessMemory(hProcess, zero, array, array.Length, out lpNumberOfBytesRead) || (lpNumberOfBytesRead != array.Length)))
                    {
                        throw new InvalidOperationException("Unable to write stub to the remote process.");
                    }
                    IntPtr hObject = WinAPI.CreateRemoteThread(hProcess, 0, 0, zero, (uint) hModule.Add(((long) num2)).ToInt32(), 0, 0);
                    if (WinAPI.WaitForSingleObject(hObject, 0x1388) == 0L)
                    {
                        WinAPI.VirtualFreeEx(hProcess, zero, 0, 0x8000);
                        WinAPI.CloseHandle(hObject);
                        return WinAPI.VirtualFreeEx(hProcess, hModule, 0, 0x8000);
                    }
                    return false;
                }
                return WinAPI.VirtualFreeEx(hProcess, hModule, 0, 0x8000);
            }
            catch (Exception exception)
            {
                this.SetLastError(exception);
                return false;
            }
        }

        public override bool[] UnloadAll(IntPtr[] hModules, IntPtr hProcess)
        {
            this.ClearErrors();
            if (hModules == null)
            {
                throw new ArgumentNullException("hModules", "Parameter cannot be null.");
            }
            if (hProcess.IsNull() || hProcess.Compare(-1L))
            {
                throw new ArgumentOutOfRangeException("hProcess", "Invalid process handle specified.");
            }
            try
            {
                bool[] flagArray = new bool[hModules.Length];
                for (int i = 0; i < hModules.Length; i++)
                {
                    flagArray[i] = this.Unload(hModules[i], hProcess);
                }
                return flagArray;
            }
            catch (Exception exception)
            {
                this.SetLastError(exception);
                return null;
            }
        }
    }
}

