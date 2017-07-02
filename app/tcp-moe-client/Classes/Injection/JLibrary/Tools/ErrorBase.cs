namespace JLibrary.Tools
{
    using System;

    [Serializable]
    public abstract class ErrorBase
    {
        protected Exception _lasterror = null;

        protected ErrorBase()
        {
        }

        public virtual void ClearErrors()
        {
            this._lasterror = null;
        }

        public virtual Exception GetLastError()
        {
            return this._lasterror;
        }

        protected virtual bool SetLastError(Exception e)
        {
            this._lasterror = e;
            return false;
        }

        protected virtual bool SetLastError(string message)
        {
            return this.SetLastError(new Exception(message));
        }
    }
}

