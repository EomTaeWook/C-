using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using API.Socket.Exception;
using System.Net;
using System.Collections.Generic;
using API.Util;
using API.Socket.InternalStructure;

namespace API.Socket.ServerSocket
{
    public abstract class ServerBase
    {
        private bool _isRunning;
        private ManualResetEvent _allDone;
        private MemoryPool<SocketAsyncEventArgs> _ioEventPool;
        private SocketAsyncEventArgs _acceptEvent;
        private Thread _thread;
        private IPEndPoint _iPEndPoint;

        private Dictionary<ulong, SocketAsyncEventArgs> _useIOEvents;
        private SyncCount _acceptCount;
        private readonly object _readMutex;
        private readonly object _deleteMutex;

        #region abstract | virtual
        protected abstract void OnAccepted(StateObject state);
        protected abstract void OnDisconnected(ulong handerKey);
        protected abstract void OnRecieved(StateObject state);
        public virtual void BroadCast(IPacket packet, StateObject state) { }
        #endregion
        protected ServerBase() : this(1000)
        {
        }
        protected ServerBase(int poolCount)
        {
            _acceptCount = new SyncCount();
            _readMutex = new object();
            _deleteMutex = new object();
            _isRunning = false;
            _allDone = new ManualResetEvent(false);
            _ioEventPool = new MemoryPool<SocketAsyncEventArgs>(poolCount, CreateSockEventArg);
            _acceptEvent = new SocketAsyncEventArgs();
            _useIOEvents = new Dictionary<ulong, SocketAsyncEventArgs>();

            _acceptEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
        }
        protected void SetSocketMemoryPool(int count)
        {
            if (count > _ioEventPool.Count)
            {
                _ioEventPool.Init(count - _ioEventPool.Count, CreateSockEventArg);
            }
        }
        private SocketAsyncEventArgs CreateSockEventArg()
        {
            StateObject state = new StateObject();
            SocketAsyncEventArgs ioEvent = new SocketAsyncEventArgs();
            ioEvent.Completed += IO_Completed;
            ioEvent.UserToken = state;
            ioEvent.SetBuffer(new byte[StateObject.BufferSize], 0, StateObject.BufferSize);
            return ioEvent;
        }
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                ProcessReceive(e);
            }
        }
        private void BeginReceive(SocketAsyncEventArgs e)
        {
            StateObject state = e.UserToken as StateObject;
            if (state.Socket != null)
            {
                bool pending = state.Socket.ReceiveAsync(e);
                if (!pending)
                {
                    ProcessReceive(e);
                }
            }
        }
        private void ProcessReceive(SocketAsyncEventArgs e)
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
                    OnRecieved(state);
                }
                else
                {
                    ClosePeer(state);
                    return;
                }
            }
            catch (System.Exception)
            {
                state.ReceiveBuffer.Clear();
            }
            BeginReceive(e);
        }
        public void Close()
        {
            _isRunning = false;
            _allDone.WaitOne();
            if (_useIOEvents != null)
            {
                try
                {
                    Monitor.Enter(_useIOEvents);
                    foreach (var c in _useIOEvents)
                    {
                        (c.Value.UserToken as StateObject).Dispose();
                        c.Value.Dispose();
                    }
                }
                finally
                {
                    Monitor.Exit(_useIOEvents);
                }
            }
            _ioEventPool.Dispose();
            Close();
            _useIOEvents = null;
        }
        private void StartListening()
        {
            _isRunning = true;
            System.Net.Sockets.Socket listener = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(_iPEndPoint);
                listener.Listen(200);

                while (_isRunning)
                {
                    _acceptEvent.AcceptSocket = null;
                    _allDone.Reset();
                    var pending = listener.AcceptAsync(_acceptEvent);

                    if (!pending)
                        Accept_Completed(null, _acceptEvent);
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
                var io = _ioEventPool.Pop();

                state = (io.UserToken as StateObject);
                try
                {
                    System.Net.Sockets.Socket sock = e.AcceptSocket;
                    state.Handle = _acceptCount.CountAdd();
                    state.Socket = sock;
                    AddPeer(io);
                    BeginReceive(io);
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
            }
        }
        private void AddPeer(SocketAsyncEventArgs ioEvent)
        {
            try
            {
                try
                {
                    Monitor.Enter(_readMutex);
                    var state = (ioEvent.UserToken as StateObject);
                    if (_useIOEvents.ContainsKey(state.Handle))
                    {
                        return;
                    }
                    _useIOEvents.Add(state.Handle, ioEvent);
                    OnAccepted(state);
                }
                finally
                {
                    Monitor.Exit(_readMutex);
                }
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
                Monitor.Enter(_deleteMutex);
                ulong handle = state.Handle;
                if (_useIOEvents.ContainsKey(state.Handle))
                {
                    _ioEventPool.Push(_useIOEvents[handle]);
                    _useIOEvents.Remove(handle);
                }
                state.Init();
                OnDisconnected(handle);
            }
            finally
            {
                Monitor.Exit(_deleteMutex);
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
        public void Send(StateObject stateObject, IPacket packet)
        {
            try
            {
                if (stateObject.Socket == null)
                {
                    throw new Exception.Exception(ErrorCode.SocketDisConnect, "");
                }
                if (packet == null) return;
                stateObject.Send(packet);
            }
            catch (Exception.Exception)
            {
                ClosePeer(stateObject);
            }
            catch (System.Exception)
            {
                ClosePeer(stateObject);
            }
        }
    }
}
