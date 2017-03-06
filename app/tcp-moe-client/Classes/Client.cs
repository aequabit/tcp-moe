using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.ComponentModel;

//------------------
//Creator: aeonhack
//Site: nimoru.com
//Name: Buffered Client
//Created: 9/12/2012
//Changed: 6/13/2013
//Version: 1.2.0.3
//------------------

public sealed class UserClient : IDisposable
{

    //TODO: Lock objects where needed.
    //TODO: Create and handle ReadQueue?
    //TODO: Provide option to disable buffering.

    #region " Events "

    public event ExceptionThrownEventHandler ExceptionThrown;
    public delegate void ExceptionThrownEventHandler(UserClient sender, Exception ex);

    private void OnExceptionThrown(Exception ex)
    {
        if (ExceptionThrown != null)
        {
            ExceptionThrown(this, ex);
        }
    }

    public event StateChangedEventHandler StateChanged;
    public delegate void StateChangedEventHandler(UserClient sender, bool connected);

    private void OnStateChanged(bool connected)
    {
        if (StateChanged != null)
        {
            StateChanged(this, connected);
        }
    }

    public event ReadPacketEventHandler ReadPacket;
    public delegate void ReadPacketEventHandler(UserClient sender, byte[] data);

    private void OnReadPacket(byte[] data)
    {
        if (ReadPacket != null)
        {
            ReadPacket(this, data);
        }
    }

    public event ReadProgressChangedEventHandler ReadProgressChanged;
    public delegate void ReadProgressChangedEventHandler(UserClient sender, double progress, int bytesRead, int bytesToRead);

    private void OnReadProgressChanged(double progress, int bytesRead, int bytesToRead)
    {
        if (ReadProgressChanged != null)
        {
            ReadProgressChanged(this, progress, bytesRead, bytesToRead);
        }
    }

    public event WritePacketEventHandler WritePacket;
    public delegate void WritePacketEventHandler(UserClient sender, int size);

    private void OnWritePacket(int size)
    {
        if (WritePacket != null)
        {
            WritePacket(this, size);
        }
    }

    public event WriteProgressChangedEventHandler WriteProgressChanged;
    public delegate void WriteProgressChangedEventHandler(UserClient sender, double progress, int bytesWritten, int bytesToWrite);

    private void OnWriteProgressChanged(double progress, int bytesWritten, int bytesToWrite)
    {
        if (WriteProgressChanged != null)
        {
            WriteProgressChanged(this, progress, bytesWritten, bytesToWrite);
        }
    }

    #endregion

    #region " Properties "

    private ushort _BufferSize = 8192;
    public ushort BufferSize
    {
        get { return _BufferSize; }
        set
        {
            if (value < 1)
            {
                throw new Exception("Value must be greater than 0.");
            }
            else
            {
                _BufferSize = value;
            }
        }
    }

    private int _MaxPacketSize = 10485760;
    public int MaxPacketSize
    {
        get { return _MaxPacketSize; }
        set
        {
            if (value < 1)
            {
                throw new Exception("Value must be greater than 0.");
            }
            else
            {
                _MaxPacketSize = value;
            }
        }
    }

    private bool _KeepAlive = true;
    public bool KeepAlive
    {
        get { return _KeepAlive; }
        set
        {
            if (_Connected)
            {
                throw new Exception("Unable to change this option while connected.");
            }
            else
            {
                _KeepAlive = value;
            }
        }
    }

    private object _UserState;
    public object UserState
    {
        get { return _UserState; }
        set { _UserState = value; }
    }

    private IPEndPoint _EndPoint;
    public IPEndPoint EndPoint
    {
        get
        {
            if (_EndPoint != null)
            {
                return _EndPoint;
            }
            else
            {
                return new IPEndPoint(IPAddress.None, 0);
            }
        }
    }

    private bool _Connected;
    public bool Connected
    {
        get { return _Connected; }
    }

    #endregion

    private AsyncOperation O;

    private Socket Handle;
    private int SendIndex;

    private byte[] SendBuffer;
    private int ReadIndex;

    private byte[] ReadBuffer;

    private Queue<byte[]> SendQueue;
    private SocketAsyncEventArgs[] Items;

    private bool[] Processing = new bool[2];
    public UserClient()
    {
        O = AsyncOperationManager.CreateOperation(null);
    }

    public void Connect(string host, ushort port)
    {
        try
        {
            Disconnect();
            Initialize();

            IPAddress IP = IPAddress.None;
            if (IPAddress.TryParse(host, out IP))
            {
                DoConnect(IP, port);
            }
            else
            {
                Dns.BeginGetHostEntry(host, EndGetHostEntry, port);
            }
        }
        catch (Exception ex)
        {
            O.Post(x => OnExceptionThrown((Exception)x), ex);
            Disconnect();
        }
    }

    private void EndGetHostEntry(IAsyncResult r)
    {
        try
        {
            DoConnect(Dns.EndGetHostEntry(r).AddressList[0], (ushort)r.AsyncState);
        }
        catch (Exception ex)
        {
            O.Post(x => OnExceptionThrown((Exception)x), ex);
            Disconnect();
        }
    }

    private void DoConnect(IPAddress ip, ushort port)
    {
        try
        {
            Handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Handle.NoDelay = true;

            if (_KeepAlive)
            {
                Handle.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 20000);
            }

            Items[0].RemoteEndPoint = new IPEndPoint(ip, port);
            if (!Handle.ConnectAsync(Items[0]))
            {
                Process(null, Items[0]);
            }
        }
        catch (Exception ex)
        {
            O.Post(x => OnExceptionThrown((Exception)x), ex);
            Disconnect();
        }
    }

    private void Initialize()
    {
        Processing = new bool[2];

        SendIndex = 0;
        ReadIndex = 0;

        SendBuffer = new byte[-1 + 1];
        ReadBuffer = new byte[-1 + 1];

        SendQueue = new Queue<byte[]>();

        Items = new SocketAsyncEventArgs[2];

        Items[0] = new SocketAsyncEventArgs();
        Items[1] = new SocketAsyncEventArgs();
        Items[0].Completed += Process;
        Items[1].Completed += Process;
    }

    private void Process(object s, SocketAsyncEventArgs e)
    {
        try
        {
            if (e.SocketError == SocketError.Success)
            {
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Connect:
                        _EndPoint = (IPEndPoint)Handle.RemoteEndPoint;
                        _Connected = true;
                        Items[0].SetBuffer(new byte[_BufferSize], 0, _BufferSize);

                        O.Post(state => OnStateChanged(true), null);
                        if (!Handle.ReceiveAsync(e))
                        {
                            Process(null, e);
                        }
                        break;
                    case SocketAsyncOperation.Receive:
                        if (!_Connected)
                            return;

                        if (!(e.BytesTransferred == 0))
                        {
                            HandleRead(e.Buffer, 0, e.BytesTransferred);

                            if (!Handle.ReceiveAsync(e))
                            {
                                Process(null, e);
                            }
                        }
                        else
                        {
                            Disconnect();
                        }
                        break;
                    case SocketAsyncOperation.Send:
                        if (!_Connected)
                            return;

                        bool EOS = false;
                        SendIndex += e.BytesTransferred;

                        O.Post(WriteProgressChangedCallback, new object[] {
                            (SendIndex / SendBuffer.Length) * 100,
                            SendIndex,
                            SendBuffer.Length
                        });

                        if ((SendIndex >= SendBuffer.Length))
                        {
                            EOS = true;
                            O.Post(x => OnWritePacket((int)x), SendBuffer.Length - 4);
                        }

                        if (SendQueue.Count == 0 && EOS)
                        {
                            Processing[1] = false;
                        }
                        else
                        {
                            HandleSendQueue();
                        }
                        break;
                }
            }
            else
            {
                O.Post(x => OnExceptionThrown((SocketException)x), new SocketException((int)e.SocketError));
                Disconnect();
            }
        }
        catch (Exception ex)
        {
            O.Post(x => OnExceptionThrown((Exception)x), ex);
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (Processing[0])
        {
            return;
        }
        else
        {
            Processing[0] = true;
        }

        bool Raise = _Connected;
        _Connected = false;

        if (Handle != null)
        {
            Handle.Close();
        }

        if (SendQueue != null)
        {
            SendQueue.Clear();
        }

        SendBuffer = new byte[-1 + 1];
        ReadBuffer = new byte[-1 + 1];

        if (Raise)
        {
            O.Post(state => OnStateChanged(false), null);
        }

        if (Items != null)
        {
            Items[0].Dispose();
            Items[1].Dispose();
        }

        _UserState = null;
        _EndPoint = null;
    }

    public void Send(byte[] data)
    {
        if (!_Connected)
            return;

        SendQueue.Enqueue(data);

        if (!Processing[1])
        {
            Processing[1] = true;
            HandleSendQueue();
        }
    }

    private void HandleSendQueue()
    {
        try
        {
            if (SendIndex >= SendBuffer.Length)
            {
                SendIndex = 0;
                SendBuffer = Header(SendQueue.Dequeue());
            }

            int Write = Math.Min(SendBuffer.Length - SendIndex, _BufferSize);
            Items[1].SetBuffer(SendBuffer, SendIndex, Write);

            if (!Handle.SendAsync(Items[1]))
            {
                Process(null, Items[1]);
            }
        }
        catch (Exception ex)
        {
            O.Post(x => OnExceptionThrown((Exception)x), ex);
            Disconnect();
        }
    }

    private static byte[] Header(byte[] data)
    {
        byte[] T = new byte[data.Length + 4];
        Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, T, 0, 4);
        Buffer.BlockCopy(data, 0, T, 4, data.Length);
        return T;
    }

    private void HandleRead(byte[] data, int index, int length)
    {
        if (ReadIndex >= ReadBuffer.Length)
        {
            ReadIndex = 0;
            if (data.Length < 4)
            {
                O.Post(state => OnExceptionThrown(new Exception("Missing or corrupt packet header.")), null);
                Disconnect();
                return;
            }

            int PacketSize = BitConverter.ToInt32(data, index);
            if (PacketSize > _MaxPacketSize)
            {
                O.Post(state => OnExceptionThrown(new Exception("Packet size exceeds MaxPacketSize.")), null);
                Disconnect();
                return;
            }

            Array.Resize(ref ReadBuffer, PacketSize);
            index += 4;
        }

        int Read = Math.Min(ReadBuffer.Length - ReadIndex, length - index);
        Buffer.BlockCopy(data, index, ReadBuffer, ReadIndex, Read);
        ReadIndex += Read;

        O.Post(ReadProgressChangedCallback, new object[] {
            (ReadIndex / ReadBuffer.Length) * 100,
            ReadIndex,
            ReadBuffer.Length
        });

        if (ReadIndex >= ReadBuffer.Length)
        {
            byte[] BufferClone = new byte[ReadBuffer.Length];
            //Race condition fail-safe.
            Buffer.BlockCopy(ReadBuffer, 0, BufferClone, 0, ReadBuffer.Length);

            O.Post(x => OnReadPacket((byte[])x), BufferClone);
        }

        if (Read < (length - index))
        {
            HandleRead(data, index + Read, length);
        }
    }

    private void ReadProgressChangedCallback(object arg)
    {
        object[] Params = (object[])arg;
        OnReadProgressChanged((int)Params[0], (int)Params[1], (int)Params[2]);
    }

    private void WriteProgressChangedCallback(object arg)
    {
        object[] Params = (object[])arg;
        OnWriteProgressChanged((int)Params[0], (int)Params[1], (int)Params[2]);
    }

    #region " IDisposable Support "


    private bool DisposedValue;
    private void Dispose(bool disposing)
    {
        if (!DisposedValue && disposing)
            Disconnect();
        DisposedValue = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

}