namespace JLibrary.Tools
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    [Serializable]
    public class MemoryIterator : ErrorBase, IDisposable
    {
        private MemoryStream _base;
        private bool _disposed;
        private UnmanagedBuffer _ubuffer;

        public MemoryIterator(byte[] iterable)
        {
            if (iterable == null)
            {
                throw new ArgumentException("Unable to iterate a null reference", "iterable");
            }
            this._base = new MemoryStream(iterable, 0, iterable.Length, true);
            this._ubuffer = new UnmanagedBuffer(0x100);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this._ubuffer.Dispose();
                    this._base.Dispose();
                }
                this._disposed = true;
            }
        }

        protected byte[] GetUnderlyingData()
        {
            return this._base.ToArray();
        }

        public bool Read<TResult>(out TResult result) where TResult: struct
        {
            return this.Read<TResult>(0L, SeekOrigin.Current, out result);
        }

        public bool Read(long offset, SeekOrigin origin, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", "Parameter cannot be null");
            }
            try
            {
                this._base.Seek(offset, origin);
                this._base.Read(buffer, 0, buffer.Length);
            }
            catch (Exception exception)
            {
                this.SetLastError(exception);
                buffer = null;
            }
            return (buffer != null);
        }

        public bool Read<TResult>(long offset, SeekOrigin origin, out TResult result) where TResult: struct
        {
            result = default(TResult);
            try
            {
                this._base.Seek(offset, origin);
                byte[] buffer = new byte[Marshal.SizeOf(typeof(TResult))];
                this._base.Read(buffer, 0, buffer.Length);
                if (!this._ubuffer.Translate<TResult>(buffer, out result))
                {
                    throw this._ubuffer.GetLastError();
                }
                return true;
            }
            catch (Exception exception)
            {
                return base.SetLastError(exception);
            }
        }

        public bool ReadString(long offset, SeekOrigin origin, out string lpBuffer, int len = -1, Encoding stringEncoding = null)
        {
            lpBuffer = null;
            byte[] buffer = new byte[(len > 0) ? len : 0x40];
            if (stringEncoding == null)
            {
                stringEncoding = Encoding.ASCII;
            }
            try
            {
                this._base.Seek(offset, origin);
                StringBuilder builder = new StringBuilder((len > 0) ? len : 260);
                int length = -1;
                int num2 = 0;
                int startIndex = 0;
                while ((length == -1) && ((num2 = this._base.Read(buffer, 0, buffer.Length)) > 0))
                {
                    builder.Append(stringEncoding.GetString(buffer));
                    length = builder.ToString().IndexOf('\0', startIndex);
                    startIndex += num2;
                    if ((len > 0) && (startIndex >= len))
                    {
                        break;
                    }
                }
                if (length > -1)
                {
                    lpBuffer = builder.ToString().Substring(0, length);
                }
                else if ((startIndex >= len) && (len > 0))
                {
                    lpBuffer = builder.ToString().Substring(0, len);
                }
                return (lpBuffer != null);
            }
            catch (Exception exception)
            {
                return this.SetLastError(exception);
            }
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return this._base.Seek(offset, origin);
        }

        public bool Write(long offset, SeekOrigin origin, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("Parameter 'data' cannot be null");
            }
            try
            {
                this._base.Seek(offset, origin);
                this._base.Write(data, 0, data.Length);
                return true;
            }
            catch (Exception exception)
            {
                return this.SetLastError(exception);
            }
        }

        public bool Write<TSource>(long offset, SeekOrigin origin, TSource data) where TSource: struct
        {
            try
            {
                this._base.Seek(offset, origin);
                byte[] buffer = null;
                if (!this._ubuffer.Translate<TSource>(data, out buffer))
                {
                    throw this._ubuffer.GetLastError();
                }
                this._base.Write(buffer, 0, buffer.Length);
                return true;
            }
            catch (Exception exception)
            {
                return this.SetLastError(exception);
            }
        }
    }
}

