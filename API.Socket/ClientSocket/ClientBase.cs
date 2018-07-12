using API.Socket.Base;
using API.Socket.Data;
using API.Socket.Exception;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace API.Socket.ClientSocket
{
    public abstract class ClientBase
    {
        private StateObject _stateObject;
        private IPEndPoint _remoteEP;
        private SocketAsyncEventArgs _ioEvent;
        private string ip;
        private int port;
        private readonly object _closePeerObj;
        #region abstract
        protected abstract void OnDisconnected();
        protected abstract void OnConnected(StateObject state);
        protected abstract void OnRecieved(StateObject state);
        #endregion abstract
        protected ClientBase()
        {
            _closePeerObj = new object();
            Init();
        }
        public void Close()
        {
            ClosePeer();
            _stateObject.Dispose();
        }
        public void Connect(string ip, int port, int timeout = 5000)
        {
            try
            {
                if (IsConnect()) return;
                this.ip = ip;
                this.port = port;
                _remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);
                if (_stateObject.Socket == null)
                {
                    System.Net.Sockets.Socket handler = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IAsyncResult asyncResult = handler.BeginConnect(_remoteEP, null, null);
                    if (asyncResult.AsyncWaitHandle.WaitOne(timeout, true))
                    {
                        handler.EndConnect(asyncResult);
                        _stateObject.Socket = handler;
                        BeginReceive(_stateObject);
                        OnConnected(_stateObject);
                    }
                    else
                    {
                        _stateObject.Init();
                        throw new SocketException(10060);
                    }
                }
                else
                {
                    _stateObject.Init();
                }
            }
            catch (ArgumentNullException arg)
            {
                throw new Exception.Exception(arg.Message);
            }
            catch (SocketException se)
            {
                throw new Exception.Exception(se.Message);
            }
            catch (System.Exception e)
            {
                throw new Exception.Exception(e.Message);
            }
        }
        private void BeginReceive(StateObject state)
        {
            if (state.Socket != null)
            {
                bool pending = state.Socket.ReceiveAsync(_ioEvent);
                if (!pending)
                {
                    ProcessReceive(_ioEvent);
                }
            }
        }
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            var state = e.UserToken as StateObject;
            bool pending = false;
            try
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    state.ReceiveBuffer.Append(e.Buffer.Take(e.BytesTransferred).ToArray());
                    OnRecieved(state);
                }
                else
                {
                    ClosePeer();
                    return;
                }
                pending = state.Socket.ReceiveAsync(e);
            }
            catch (System.Exception)
            {
                state.ReceiveBuffer.Clear();
            }
            if (!pending)
                ProcessReceive(e);
        }
        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
                ProcessReceive(e);
        }

        private void Init()
        {
            try
            {
                _stateObject = new StateObject();
                _ioEvent = new SocketAsyncEventArgs();
                _ioEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
                _ioEvent.SetBuffer(new byte[StateObject.BufferSize], 0, StateObject.BufferSize);
                _ioEvent.UserToken = _stateObject;
            }
            catch (System.Exception ex)
            {
                throw new Exception.Exception(ex.Message);
            }
        }
        protected void ClosePeer()
        {
            try
            {
                if (Monitor.TryEnter(_closePeerObj))
                {
                    try
                    {
                        if (_ioEvent != null && _stateObject.Socket != null)
                        {
                            _ioEvent.SocketError = SocketError.Shutdown;
                            _stateObject.Init();
                            OnDisconnected();
                        }
                    }
                    finally
                    {
                        Monitor.Exit(_closePeerObj);
                    }
                }
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        public bool IsConnect()
        {
            if (_stateObject == null) return false;
            if (_stateObject.Socket == null) return false;
            return !(_stateObject.Socket.Poll(1000, SelectMode.SelectRead) && _stateObject.Socket.Available == 0);
        }
        public void Send(Packet packet)
        {
            try
            {
                if (_stateObject.Socket == null)
                {
                    throw new Exception.Exception(ErrorCode.SocketDisConnect, "");
                }
                if (packet == null) return;
                _stateObject.Send(packet);
            }
            catch (Exception.Exception ex)
            {
                Console.WriteLine(ex.Message);
                ClosePeer();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                ClosePeer();
            }
        }
    }
}
