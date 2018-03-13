using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using API.Socket.Data.Packet;
using API.Socket.Data;
using System.Diagnostics;
using API.Socket.Exception;
using System.Net;
using System.Collections.Generic;
using API.Util;

namespace API.Socket
{
    public abstract class AsyncServerSocket : ServerBase
    {
        private bool _isRunning;
        private ManualResetEvent _allDone;
        private MemoryPool<Packet> _packetPool;
        private MemoryPool<SocketAsyncEventArgs> _socketArgPool;
        private SocketAsyncEventArgs _accept_Args;
        private Thread _thread;
        private IPEndPoint _iPEndPoint;
        private Dictionary<ulong, StateObject> _clientList;
        private readonly object _readMutex;
        private readonly object _deleteMutex;
        public AsyncServerSocket() : this(1000)
        {
        }
        public AsyncServerSocket(int poolCount)
        {
            _clientList = new Dictionary<ulong, StateObject>();
            _readMutex = new object();
            _deleteMutex = new object();
            _isRunning = false;
            _allDone = new ManualResetEvent(false);
            _packetPool = new MemoryPool<Packet>(poolCount * 2);
            _socketArgPool = new MemoryPool<SocketAsyncEventArgs>(poolCount, CreateSockEventArg);
        }
        protected void SetPacketMemoryPool(int count)
        {
            if (count > _packetPool.Count)
            {
                _packetPool.Init(count - _socketArgPool.Count, null);
            }
        }
        protected void SetSocketMemoryPool(int count)
        {
            if (count > _socketArgPool.Count)
            {
                _socketArgPool.Init(count - _socketArgPool.Count, CreateSockEventArg);
            }
        }
        private SocketAsyncEventArgs CreateSockEventArg()
        {
            StateObject state = new StateObject();
            SocketAsyncEventArgs arg = new SocketAsyncEventArgs();
            arg.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
            arg.UserToken = state;
            arg.SetBuffer(new byte[StateObject.BufferSize], 0, StateObject.BufferSize);
            return arg;
        }
        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                Process_Receive(e);
            }
        }
        private void Process_Receive(SocketAsyncEventArgs e)
        {
            StateObject state = e.UserToken as StateObject;
            try
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    Debug.WriteLine("handler {0}: Read {1}", state.Handle, e.BytesTransferred);
                    state.Queue.Append(e.Buffer.Take(e.BytesTransferred).ToArray());
                    if (state.Queue.Count() >= Packet.HeaderSize)
                    {
                        var b = state.Queue.Peek(0, Packet.HeaderSize);
                        SHeader header = ObjectConverter.BytesToObject<SHeader>(b);
                        if (state.Queue.Count() >= header.DataSize)
                        {
                            b = state.Queue.Read(Convert.ToUInt32(header.DataSize));
                            Packet packet = _packetPool.Pop();
                            packet.SetHeader(header);
                            packet.Data = b.Skip(Packet.HeaderSize).ToArray();
                            if (state.PacketQueue.Count() <= 0)
                            {
                                state.PacketQueue.Append(packet);
                                ThreadPool.QueueUserWorkItem(new WaitCallback(Work), state);
                            }
                            else
                            {
                                state.PacketQueue.Append(packet);
                            }
                        }
                    }
                    bool pending = state.WorkSocket.ReceiveAsync(e);
                    if (!pending)
                    {
                        Process_Receive(e);
                    }
                }
                else
                {
                    ClosePeer(state);
                }
            }
            catch (SocketException ex)
            {
                ClosePeer(state);
                Debug.WriteLine("ReadCallback SocketException : Handler : " + state.Handle + " Close Message : " + ex.Message);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("ReadCallback System.Exception : Handler : " + state.Handle + " Close Message : " + ex.Message);
            }
        }
        public sealed override void Close()
        {
            _isRunning = false;
            _allDone.WaitOne();
            if (_clientList != null)
            {
                try
                {
                    Monitor.Enter(_clientList);
                    foreach (var c in _clientList)
                    {
                        var arg = c.Value.ReceiveAsync;
                        arg.SocketError = SocketError.Shutdown;
                        _socketArgPool.Push(arg);
                        c.Value.Init();
                        c.Value.Dispose();
                    }
                }
                finally
                {
                    Monitor.Exit(_clientList);
                }
            }
            _packetPool.Dispose();
            _socketArgPool.Dispose();
            base.Close();
            _clientList = null;
        }
        private void StartListening()
        {
            _isRunning = true;
            System.Net.Sockets.Socket listener = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(_iPEndPoint);
                listener.Listen(200);
                _accept_Args = new SocketAsyncEventArgs();
                _accept_Args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
                bool accept = true;
                while (_isRunning)
                {
                    _accept_Args.AcceptSocket = null;
                    _allDone.Reset();
                    try
                    {
                        accept = listener.AcceptAsync(_accept_Args);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        continue;
                    }
                    if (!accept)
                    {
                        Accept_Completed(null, _accept_Args);
                    }
                    _allDone.WaitOne();
                }
                Debug.WriteLine("Server Start");
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine(ex.GetErrorMessage());
            }
        }
        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            StateObject state = null;
            if (e.SocketError == SocketError.Success)
            {
                var arg = _socketArgPool.Pop();
                state = (arg.UserToken as StateObject);
                try
                {
                    System.Net.Sockets.Socket handler = e.AcceptSocket;
                    state.Handle = _acceptCount.CountAdd();
                    state.WorkSocket = handler;
                    state.ReceiveAsync = arg;
                    bool pending = state.WorkSocket.ReceiveAsync(arg);
                    if (!pending)
                    {
                        Process_Receive(arg);
                    }
                }
                catch (SocketException ex)
                {
                    Debug.WriteLine("Accept Exception : " + ex.Message);
                    ClosePeer(state);
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine("Accept Exception : " + ex.Message);
                    ClosePeer(state);
                }
                finally
                {
                    _allDone.Set();
                }
                AddPeer(state);
            }
        }
        private void AddPeer(StateObject state)
        {
            try
            {
                if (_clientList != null)
                {
                    try
                    {
                        Monitor.Enter(_readMutex);
                        if (_clientList.ContainsKey(state.Handle))
                        {
                            return;
                        }
                        _clientList.Add(state.Handle, state);
                    }
                    finally
                    {
                        Monitor.Exit(_readMutex);
                    }
                }
                AcceptComplete(state);
            }
            catch (Exception.Exception ex)
            {
                throw new Exception.Exception(ex.Message);
            }
        }
        public override void Reject(StateObject handler)
        {
            ClosePeer(handler);
        }
        protected void ClosePeer(StateObject state)
        {
            try
            {
                if (_clientList != null)
                {
                    ulong handle = state.Handle;
                    try
                    {
                        Monitor.Enter(_deleteMutex);
                        if (_clientList.ContainsKey(handle))
                        {
                            _clientList.Remove(handle);
                        }
                        var arg = state.ReceiveAsync;
                        arg.SocketError = SocketError.Shutdown;
                        _socketArgPool.Push(arg);
                        state.Init();
                    }
                    finally
                    {
                        Monitor.Exit(_deleteMutex);
                    }
                    DisconnectedComplete(handle);
                }
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine(ex.GetErrorMessage());
            }
        }
        private void Work(object obj)
        {
            StateObject state = (StateObject)obj;
            try
            {
                while (state.PacketQueue.Count() > 0)
                {
                    Packet packet = state.PacketQueue.Read();
                    object[] arg = null;
                    if (!PacketConversionComplete(packet, state, out arg))
                    {
                        Debug.WriteLine("PacketConversionComplete False");
                        continue;
                    }
                    if (packet.GetHeader().Tag != '~')
                    {
                        Debug.WriteLine("Header Tag Wrong!");
                        continue;
                    }
                    switch (VerifyPacket(packet, state))
                    {
                        case Data.Enum.VertifyResult.Vertify_Reject:
                            {
                                Debug.WriteLine("VertifyResult.Vertify_Reject");
                                Reject(state);
                                return;
                            }
                        case Data.Enum.VertifyResult.Vertify_Ignore:
                            {
                                Debug.WriteLine("VertifyResult.Vertify_Ignore");
                                continue;
                            }
                        case Data.Enum.VertifyResult.Vertify_Forward:
                            {
                                ForwardFunc(packet, state);
                                continue;
                            }
                    }
                    if (RunCallbackFunc(packet.GetHeader().Protocol, packet, state, arg))
                    {
                        CallbackComplete(packet, state);
                    }
                    _packetPool.Push(packet);
                }
            }
            catch (SocketException ex)
            {
                ClosePeer(state);
                Debug.WriteLine(ex.Message);
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine(ex.GetErrorMessage());
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void Init(string ip, int port)
        {
            try
            {
                if (ip == "") _iPEndPoint = new IPEndPoint(IPAddress.Any, port);
                else _iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        protected void Start(string ip, int port)
        {
            try
            {
                Init(ip, port);
                if (_thread == null)
                {
                    _thread = new Thread(StartListening);
                    _thread.Start();
                }
            }
            catch (System.Exception ex)
            {
                Debug.Write(ex.Message);
                throw new Exception.Exception(ex.Message);
            }
        }
        protected void Start(int port)
        {
            Start("", port);
        }
        public override void Send(StateObject handler, Packet packet)
        {
            try
            {
                if (handler.WorkSocket == null)
                {
                    throw new Exception.Exception(ErrorCode.SocketDisConnect, "");
                }
                if (packet == null) return;
                handler.Send(packet);
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine("Send Exception : " + ex.GetErrorMessage());
                ClosePeer(handler);
            }
            catch (System.Exception ex)
            {
                ClosePeer(handler);
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
