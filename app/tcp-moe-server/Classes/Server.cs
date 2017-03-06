using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;

//------------------
//Creator: aeonhack
//Site: nimoru.com
//Name: Buffered Server
//Created: 9/12/2012
//Changed: 6/13/2013
//Version: 1.2.0.3
//------------------

public sealed class ServerClient : IDisposable
{

    //TODO: Lock objects where needed.
    //TODO: Create and handle ReadQueue?
    //TODO: Provide option to disable buffering.

    #region " Events "

    public event ExceptionThrownEventHandler ExceptionThrown;
    public delegate void ExceptionThrownEventHandler(ServerClient sender, Exception ex);

    private void OnExceptionThrown(Exception ex)
    {
        if (ExceptionThrown != null)
        {
            ExceptionThrown(this, ex);
        }
    }

    public event StateChangedEventHandler StateChanged;
    public delegate void StateChangedEventHandler(ServerClient sender, bool connected);

    private void OnStateChanged(bool connected)
    {
        if (StateChanged != null)
        {
            StateChanged(this, connected);
        }
    }

    public event ReadPacketEventHandler ReadPacket;
    public delegate void ReadPacketEventHandler(ServerClient sender, byte[] data);

    private void OnReadPacket(byte[] data)
    {
        if (ReadPacket != null)
        {
            ReadPacket(this, data);
        }
    }

    public event ReadProgressChangedEventHandler ReadProgressChanged;
    public delegate void ReadProgressChangedEventHandler(ServerClient sender, double progress, int bytesRead, int bytesToRead);

    private void OnReadProgressChanged(double progress, int bytesRead, int bytesToRead)
    {
        if (ReadProgressChanged != null)
        {
            ReadProgressChanged(this, progress, bytesRead, bytesToRead);
        }
    }

    public event WritePacketEventHandler WritePacket;
    public delegate void WritePacketEventHandler(ServerClient sender, int size);

    private void OnWritePacket(int size)
    {
        if (WritePacket != null)
        {
            WritePacket(this, size);
        }
    }

    public event WriteProgressChangedEventHandler WriteProgressChanged;
    public delegate void WriteProgressChangedEventHandler(ServerClient sender, double progress, int bytesWritten, int bytesToWrite);

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


    private Socket Handle;
    private int SendIndex;

    private byte[] SendBuffer;
    private int ReadIndex;

    private byte[] ReadBuffer;

    private Queue<byte[]> SendQueue;
    private bool[] Processing = new bool[2];

    private SocketAsyncEventArgs[] Items = new SocketAsyncEventArgs[2];
    public ServerClient(Socket sock, ushort bufferSize, int maxPacketSize)
    {
        try
        {
            Initialize();
            Items[0].SetBuffer(new byte[bufferSize], 0, bufferSize);

            Handle = sock;

            _BufferSize = bufferSize;
            _MaxPacketSize = maxPacketSize;
            _EndPoint = (IPEndPoint)Handle.RemoteEndPoint;
            _Connected = true;

            if (!Handle.ReceiveAsync(Items[0]))
            {
                Process(null, Items[0]);
            }
        }
        catch (Exception ex)
        {
            OnExceptionThrown(ex);
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

                        OnWriteProgressChanged((SendIndex / SendBuffer.Length) * 100, SendIndex, SendBuffer.Length);

                        if ((SendIndex >= SendBuffer.Length))
                        {
                            EOS = true;
                            OnWritePacket(SendBuffer.Length - 4);
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
                OnExceptionThrown(new SocketException((int)e.SocketError));
                Disconnect();
            }
        }
        catch (Exception ex)
        {
            OnExceptionThrown(ex);
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
            OnStateChanged(false);
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
            OnExceptionThrown(ex);
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
                OnExceptionThrown(new Exception("Missing or corrupt packet header."));
                Disconnect();
                return;
            }

            int PacketSize = BitConverter.ToInt32(data, index);
            if (PacketSize > _MaxPacketSize)
            {
                OnExceptionThrown(new Exception("Packet size exceeds MaxPacketSize."));
                Disconnect();
                return;
            }

            Array.Resize(ref ReadBuffer, PacketSize);
            index += 4;
        }

        int Read = Math.Min(ReadBuffer.Length - ReadIndex, length - index);
        Buffer.BlockCopy(data, index, ReadBuffer, ReadIndex, Read);
        ReadIndex += Read;

        OnReadProgressChanged((ReadIndex / ReadBuffer.Length) * 100, ReadIndex, ReadBuffer.Length);

        if (ReadIndex >= ReadBuffer.Length)
        {
            OnReadPacket(ReadBuffer);
        }

        if (Read < (length - index))
        {
            HandleRead(data, index + Read, length);
        }
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

public sealed class ServerListener : IDisposable
{

    //TODO: Remove some redundant code (e.g. Listen and Process socket preparation.)
    //TODO: Stop listening when at connection capacity, as opposed to disconnecting.
    //TODO: Implement option to limit simultaneous connections per source.

    #region " Events "

    public event StateChangedEventHandler StateChanged;
    public delegate void StateChangedEventHandler(ServerListener sender, bool listening);

    private void OnStateChanged(bool listening)
    {
        if (StateChanged != null)
        {
            StateChanged(this, listening);
        }
    }

    public event ExceptionThrownEventHandler ExceptionThrown;
    public delegate void ExceptionThrownEventHandler(ServerListener sender, Exception ex);

    private void OnExceptionThrown(Exception ex)
    {
        if (ExceptionThrown != null)
        {
            ExceptionThrown(this, ex);
        }
    }

    public event ClientExceptionThrownEventHandler ClientExceptionThrown;
    public delegate void ClientExceptionThrownEventHandler(ServerListener sender, ServerClient client, Exception ex);

    private void OnClientExceptionThrown(ServerClient client, Exception ex)
    {
        if (ClientExceptionThrown != null)
        {
            ClientExceptionThrown(this, client, ex);
        }
    }

    public event ClientStateChangedEventHandler ClientStateChanged;
    public delegate void ClientStateChangedEventHandler(ServerListener sender, ServerClient client, bool connected);

    private void OnClientStateChanged(ServerClient client, bool connected)
    {
        if (ClientStateChanged != null)
        {
            ClientStateChanged(this, client, connected);
        }
    }

    public event ClientReadPacketEventHandler ClientReadPacket;
    public delegate void ClientReadPacketEventHandler(ServerListener sender, ServerClient client, byte[] data);

    private void OnClientReadPacket(ServerClient client, byte[] data)
    {
        if (ClientReadPacket != null)
        {
            ClientReadPacket(this, client, data);
        }
    }

    public event ClientReadProgressChangedEventHandler ClientReadProgressChanged;
    public delegate void ClientReadProgressChangedEventHandler(ServerListener sender, ServerClient client, double progress, int bytesRead, int bytesToRead);

    private void OnClientReadProgressChanged(ServerClient client, double progress, int bytesRead, int bytesToRead)
    {
        if (ClientReadProgressChanged != null)
        {
            ClientReadProgressChanged(this, client, progress, bytesRead, bytesToRead);
        }
    }

    public event ClientWritePacketEventHandler ClientWritePacket;
    public delegate void ClientWritePacketEventHandler(ServerListener sender, ServerClient client, int size);

    private void OnClientWritePacket(ServerClient client, int size)
    {
        if (ClientWritePacket != null)
        {
            ClientWritePacket(this, client, size);
        }
    }

    public event ClientWriteProgressChangedEventHandler ClientWriteProgressChanged;
    public delegate void ClientWriteProgressChangedEventHandler(ServerListener sender, ServerClient client, double progress, int bytesWritten, int bytesToWrite);

    private void OnClientWriteProgressChanged(ServerClient client, double progress, int bytesWritten, int bytesToWrite)
    {
        if (ClientWriteProgressChanged != null)
        {
            ClientWriteProgressChanged(this, client, progress, bytesWritten, bytesToWrite);
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
            if (_Listening)
            {
                throw new Exception("Unable to change this option while listening.");
            }
            else
            {
                _KeepAlive = value;
            }
        }
    }

    private ushort _MaxConnections = 20;
    public ushort MaxConnections
    {
        get { return _MaxConnections; }
        set { _MaxConnections = value; }
    }

    private bool _Listening;
    public bool Listening
    {
        get { return _Listening; }
    }

    private List<ServerClient> _Clients;
    public ServerClient[] Clients
    {
        get
        {
            if (_Listening)
            {
                return _Clients.ToArray();
            }
            else
            {
                return new ServerClient[] { };
            }
        }
    }

    #endregion


    private Socket Handle;
    private bool Processing;

    private SocketAsyncEventArgs Item;
    public void Listen(ushort port)
    {
        try
        {
            if (!_Listening)
            {
                _Clients = new List<ServerClient>();

                Item = new SocketAsyncEventArgs();
                Item.Completed += Process;
                Item.AcceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Item.AcceptSocket.NoDelay = true;

                if (_KeepAlive)
                {
                    Item.AcceptSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 20000);
                }

                Handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Handle.Bind(new IPEndPoint(IPAddress.Any, port));
                Handle.Listen(10);

                Processing = false;
                _Listening = true;

                OnStateChanged(true);
                if (!Handle.AcceptAsync(Item))
                {
                    Process(null, Item);
                }
            }
        }
        catch (Exception ex)
        {
            OnExceptionThrown(ex);
            Disconnect();
        }
    }

    private void Process(object s, SocketAsyncEventArgs e)
    {
        try
        {
            if (e.SocketError == SocketError.Success)
            {
                ServerClient T = new ServerClient(e.AcceptSocket, _BufferSize, _MaxPacketSize);

                lock (_Clients)
                {
                    if (_Clients.Count < _MaxConnections)
                    {
                        _Clients.Add(T);
                        T.StateChanged += HandleStateChanged;
                        T.ExceptionThrown += OnClientExceptionThrown;
                        T.ReadPacket += OnClientReadPacket;
                        T.ReadProgressChanged += OnClientReadProgressChanged;
                        T.WritePacket += OnClientWritePacket;
                        T.WriteProgressChanged += OnClientWriteProgressChanged;

                        OnClientStateChanged(T, true);
                    }
                    else
                    {
                        T.Disconnect();
                    }
                }

                e.AcceptSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                e.AcceptSocket.NoDelay = true;

                if (_KeepAlive)
                {
                    e.AcceptSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 20000);
                }

                if (!Handle.AcceptAsync(e))
                {
                    Process(null, e);
                }
            }
            else
            {
                OnExceptionThrown(new SocketException((int)e.SocketError));
                Disconnect();
            }
        }
        catch (Exception ex)
        {
            OnExceptionThrown(ex);
            Disconnect();
        }
    }

    public void Disconnect()
    {
        if (Processing)
        {
            return;
        }
        else
        {
            Processing = true;
        }

        if (Handle != null)
        {
            Handle.Close();
        }

        lock (_Clients)
        {
            while (_Clients.Count > 0)
            {
                _Clients[0].Disconnect();
                _Clients.RemoveAt(0);
            }
        }

        if (Item != null)
        {
            Item.Dispose();
        }

        _Listening = false;
        OnStateChanged(false);
    }

    private void HandleStateChanged(ServerClient client, bool connected)
    {
        lock (_Clients)
        {
            _Clients.Remove(client);
            OnClientStateChanged(client, false);
        }
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