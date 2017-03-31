namespace JLibrary.Win32
{
    using System;
    using System.Runtime.CompilerServices;

    public static class Win32Ptr
    {
        public static IntPtr Add(this IntPtr ptr, long val)
        {
            return new IntPtr(ptr.ToInt32() + ((int) val));
        }

        public static IntPtr Add(this IntPtr ptr, IntPtr val)
        {
            return new IntPtr(ptr.ToInt32() + val.ToInt32());
        }

        public static bool Compare(this IntPtr ptr, long value)
        {
            return (ptr.ToInt64() == value);
        }

        public static IntPtr Create(long value)
        {
            return new IntPtr((int) value);
        }

        public static bool IsNull(this IntPtr ptr)
        {
            return (ptr == IntPtr.Zero);
        }

        public static bool IsNull(this UIntPtr ptr)
        {
            return (ptr == UIntPtr.Zero);
        }

        public static IntPtr Subtract(this IntPtr ptr, long val)
        {
            return new IntPtr((int) (ptr.ToInt64() - val));
        }

        public static IntPtr Subtract(this IntPtr ptr, IntPtr val)
        {
            return new IntPtr((int) (ptr.ToInt64() - val.ToInt64()));
        }
    }
}

