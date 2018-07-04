using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using API.Socket.Exception;
using System.Net;
using System.Collections.Generic;
using API.Util;
using API.Socket.Data;

namespace API.Socket.ServerSocket
{
    public abstract class ServerBase
    {
        private bool _isRunning;
        private ManualResetEvent _allDone;
        private MemoryPool<SocketAsyncEventArgs> _socketArgPool;
        private SocketAsyncEventArgs _accept_Args;
        private Thread _thread;
        private IPEndPoint _iPEndPoint;
        private Dictionary<ulong, StateObject> _clientList;
        private SyncCount _acceptCount;
        private readonly object _readMutex;
        private readonly object _deleteMutex;

        #region abstract | virtual
        protected abstract void Accepted(StateObject state);
        protected abstract void Disconnected(ulong handerKey);
        protected abstract void Recieved(StateObject state);
        public virtual void BroadCast(Packet packet, StateObject state) { }
        #endregion
        protected ServerBase() : this(1000)
        {
        }
        protected ServerBase(int poolCount)
        {
            _acceptCount = new SyncCount();
            _clientList = new Dictionary<ulong, StateObject>();
            _readMutex = new object();
            _deleteMutex = new object();
            _isRunning = false;
            _allDone = new ManualResetEvent(false);
            _socketArgPool = new MemoryPool<SocketAsyncEventArgs>(poolCount, CreateSockEventArg);
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
#if DEBUG
                    Debug.WriteLine("handler {0}: Read {1}", state.Handle, e.BytesTransferred);
#endif
                    state.ReceiveBuffer.Append(e.Buffer.Take(e.BytesTransferred).ToArray());
                    Recieved(state);
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
#if DEBUG
                Debug.WriteLine("ReadCallback SocketException : Handler : " + state.Handle + " Close Message : " + ex.Message);
#endif
            }
            catch (System.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("ReadCallback System.Exception : Handler : " + state.Handle + " Close Message : " + ex.Message);
#endif
            }
        }
        public void Close()
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
            _socketArgPool.Dispose();
            Close();
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
                    catch (System.Exception)
                    {
                        continue;
                    }
                    if (!accept)
                    {
                        Accept_Completed(null, _accept_Args);
                    }
                    _allDone.WaitOne();
                }
            }
            catch (Exception.Exception ex)
            {
                throw ex;
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
#if DEBUG
                    Debug.WriteLine("Accept Exception : " + ex.Message);
#endif
                    ClosePeer(state);
                }
                catch (System.Exception ex)
                {
#if DEBUG
                    Debug.WriteLine("Accept Exception : " + ex.Message);
#endif
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
                Accepted(state);
            }
            catch (Exception.Exception ex)
            {
                throw new Exception.Exception(ex.Message);
            }
        }
        public void Reject(StateObject handler)
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
                    Disconnected(handle);
                }
            }
            catch (Exception.Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex.GetErrorMessage());
#endif
            }
        }
        private void Init(string ip, int port)
        {
            try
            {
                if (ip == "")
                    _iPEndPoint = new IPEndPoint(IPAddress.Any, port);
                else
                    _iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
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
        public void Send(StateObject handler, Packet packet)
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
            catch (Exception.Exception)
            {
                ClosePeer(handler);
            }
            catch (System.Exception)
            {
                ClosePeer(handler);
            }
        }
    }
}
