namespace JLibrary.Tools
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [Serializable]
    public class UnmanagedBuffer : ErrorBase, IDisposable
    {
        private bool _disposed = false;

        public UnmanagedBuffer(int cbneeded)
        {
            if (cbneeded > 0)
            {
                this.Pointer = Marshal.AllocHGlobal(cbneeded);
                this.Size = cbneeded;
            }
            else
            {
                this.Pointer = IntPtr.Zero;
                this.Size = 0;
            }
        }

        private bool Alloc(int cb)
        {
            try
            {
                if (cb > this.Size)
                {
                    this.Pointer = (this.Pointer == IntPtr.Zero) ? Marshal.AllocHGlobal(cb) : Marshal.ReAllocHGlobal(this.Pointer, new IntPtr(cb));
                    this.Size = cb;
                }
                return true;
            }
            catch (Exception exception)
            {
                return this.SetLastError(exception);
            }
        }

        public bool Commit<T>(T data) where T: struct
        {
            try
            {
                if (this.Alloc(Marshal.SizeOf(typeof(T))))
                {
                    Marshal.StructureToPtr(data, this.Pointer, false);
                    return true;
                }
                return false;
            }
            catch (Exception exception)
            {
                return this.SetLastError(exception);
            }
        }

        public bool Commit(byte[] data, int index, int count)
        {
            if ((data != null) && this.Alloc(count))
            {
                Marshal.Copy(data, index, this.Pointer, count);
                return true;
            }
            if (data == null)
            {
                this.SetLastError(new ArgumentException("Attempting to commit a null reference", "data"));
            }
            return false;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this.Resize(0);
                }
                this._disposed = true;
            }
        }

        public byte[] Read(int count)
        {
            try
            {
                if ((count > this.Size) || (count <= 0))
                {
                    throw new ArgumentException("There is either not enough memory allocated to read 'count' bytes, or 'count' is negative (" + count.ToString() + ")", "count");
                }
                byte[] destination = new byte[count];
                Marshal.Copy(this.Pointer, destination, 0, count);
                return destination;
            }
            catch (Exception exception)
            {
                this.SetLastError(exception);
                return null;
            }
        }

        public bool Read<TResult>(out TResult data) where TResult: struct
        {
            data = default(TResult);
            try
            {
                if (this.Size < Marshal.SizeOf(typeof(TResult)))
                {
                    throw new InvalidCastException("Not enough unmanaged memory is allocated to contain this structure type.");
                }
                data = (TResult) Marshal.PtrToStructure(this.Pointer, typeof(TResult));
                return true;
            }
            catch (Exception exception)
            {
                return this.SetLastError(exception);
            }
        }

        public bool Resize(int size)
        {
            if (size < 0)
            {
                return this.SetLastError(new ArgumentException("Attempting to resize to less than zero bytes of memory", "size"));
            }
            if (size == this.Size)
            {
                return true;
            }
            if (size > this.Size)
            {
                return this.Alloc(size);
            }
            try
            {
                if (size == 0)
                {
                    Marshal.FreeHGlobal(this.Pointer);
                    this.Pointer = IntPtr.Zero;
                }
                else if (size > 0)
                {
                    this.Pointer = Marshal.ReAllocHGlobal(this.Pointer, new IntPtr(size));
                }
                this.Size = size;
                return true;
            }
            catch (Exception exception)
            {
                return this.SetLastError(exception);
            }
        }

        public bool SafeDecommit<T>() where T: struct
        {
            try
            {
                if (this.Size < Marshal.SizeOf(typeof(T)))
                {
                    throw new InvalidCastException("Not enough unmanaged memory is allocated to contain this structure type.");
                }
                Marshal.DestroyStructure(this.Pointer, typeof(T));
                return true;
            }
            catch (Exception exception)
            {
                return this.SetLastError(exception);
            }
        }

        public bool Translate<TSource>(TSource data, out byte[] buffer) where TSource: struct
        {
            buffer = null;
            if (this.Commit<TSource>(data))
            {
                buffer = this.Read(Marshal.SizeOf(typeof(TSource)));
                this.SafeDecommit<TSource>();
            }
            return (buffer != null);
        }

        public bool Translate<TResult>(byte[] buffer, out TResult result) where TResult: struct
        {
            result = default(TResult);
            if (buffer == null)
            {
                return this.SetLastError(new ArgumentException("Attempted to translate a null reference to a structure.", "buffer"));
            }
            return (this.Commit(buffer, 0, buffer.Length) && this.Read<TResult>(out result));
        }

        public bool Translate<TSource, TResult>(TSource data, out TResult result) where TSource: struct where TResult: struct
        {
            result = default(TResult);
            return ((this.Commit<TSource>(data) && this.Read<TResult>(out result)) && this.SafeDecommit<TSource>());
        }

        public IntPtr Pointer { get; private set; }

        public int Size { get; private set; }
    }
}

