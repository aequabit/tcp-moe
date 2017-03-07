namespace InjectionLibrary
{
    using JLibrary.PortableExecutable;
    using JLibrary.Tools;
    using JLibrary.Win32;
    using System;
    using System.Runtime.CompilerServices;

    public abstract class InjectionMethod : ErrorBase
    {
        protected InjectionMethod()
        {
        }

        public static InjectionMethod Create(InjectionMethodType type)
        {
            InjectionMethod method;
            switch (type)
            {
                case InjectionMethodType.Standard:
                    method = new CRTInjection();
                    break;

                case InjectionMethodType.ThreadHijack:
                    method = new ThreadHijack();
                    break;

                case InjectionMethodType.ManualMap:
                    method = new ManualMap();
                    break;

                default:
                    return null;
            }
            if (method != null)
            {
                method.Type = type;
            }
            return method;
        }

        public virtual IntPtr Inject(JLibrary.PortableExecutable.PortableExecutable image, int processId)
        {
            this.ClearErrors();
            IntPtr hProcess = WinAPI.OpenProcess(0x43a, false, processId);
            IntPtr ptr2 = this.Inject(image, hProcess);
            WinAPI.CloseHandle(hProcess);
            return ptr2;
        }

        public abstract IntPtr Inject(JLibrary.PortableExecutable.PortableExecutable image, IntPtr hProcess);
        public virtual IntPtr Inject(string dllPath, int processId)
        {
            this.ClearErrors();
            IntPtr hProcess = WinAPI.OpenProcess(0x43a, false, processId);
            IntPtr ptr2 = this.Inject(dllPath, hProcess);
            WinAPI.CloseHandle(hProcess);
            return ptr2;
        }

        public abstract IntPtr Inject(string dllPath, IntPtr hProcess);
        public virtual IntPtr[] InjectAll(JLibrary.PortableExecutable.PortableExecutable[] images, int processId)
        {
            this.ClearErrors();
            IntPtr hProcess = WinAPI.OpenProcess(0x43a, false, processId);
            IntPtr[] ptrArray = this.InjectAll(images, hProcess);
            WinAPI.CloseHandle(hProcess);
            return ptrArray;
        }

        public abstract IntPtr[] InjectAll(JLibrary.PortableExecutable.PortableExecutable[] images, IntPtr hProcess);
        public virtual IntPtr[] InjectAll(string[] dllPaths, int processId)
        {
            this.ClearErrors();
            IntPtr hProcess = WinAPI.OpenProcess(0x43a, false, processId);
            IntPtr[] ptrArray = this.InjectAll(dllPaths, hProcess);
            WinAPI.CloseHandle(hProcess);
            return ptrArray;
        }

        public abstract IntPtr[] InjectAll(string[] dllPaths, IntPtr hProcess);
        public virtual bool Unload(IntPtr hModule, int processId)
        {
            this.ClearErrors();
            IntPtr hProcess = WinAPI.OpenProcess(0x43a, false, processId);
            bool flag = this.Unload(hModule, hProcess);
            WinAPI.CloseHandle(hProcess);
            return flag;
        }

        public abstract bool Unload(IntPtr hModule, IntPtr hProcess);
        public virtual bool[] UnloadAll(IntPtr[] hModules, int processId)
        {
            this.ClearErrors();
            IntPtr hProcess = WinAPI.OpenProcess(0x43a, false, processId);
            bool[] flagArray = this.UnloadAll(hModules, hProcess);
            WinAPI.CloseHandle(hProcess);
            return flagArray;
        }

        public abstract bool[] UnloadAll(IntPtr[] hModules, IntPtr hProcess);

        public InjectionMethodType Type { get; protected set; }
    }
}

