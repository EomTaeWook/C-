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
        private SocketAsyncEventArgs _receive_Args;
        private string ip;
        private int port;
        #region abstract
        protected abstract void DisconnectedEvent();
        protected abstract void ConnectCompleteEvent(StateObject state);
        protected abstract void Recieved(StateObject state);
        #endregion abstract
        protected ClientBase()
        {
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
                this.ip = ip;
                this.port = port;
                _remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);
                if (_stateObject.WorkSocket == null)
                {
                    System.Net.Sockets.Socket handler = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IAsyncResult asyncResult = handler.BeginConnect(_remoteEP, null, null);
                    if (asyncResult.AsyncWaitHandle.WaitOne(timeout, true))
                    {
                        handler.EndConnect(asyncResult);
                        StateObject state = (StateObject)_receive_Args.UserToken;
                        state.WorkSocket = handler;
                        BeginReceive(state);
                        ConnectCompleteEvent(state);
                    }
                    else
                    {
                        _stateObject.Init();
                        throw new SocketException(10060);
                    }
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
            _stateObject.ReceiveAsync = _receive_Args;
            if (state.WorkSocket != null)
            {
                bool pending = state.WorkSocket.ReceiveAsync(_receive_Args);
                if (!pending)
                {
                    Process_Receive(_receive_Args);
                }
            }
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
                Process_Receive(e);
        }
        private void Process_Receive(SocketAsyncEventArgs e)
        {
            try
            {
                StateObject state = e.UserToken as StateObject;
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
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
#if DEBUG
                    Debug.WriteLine(string.Format("error {0},  transferred {1}", e.SocketError, e.BytesTransferred));
#endif
                    ClosePeer();
                }
            }
            catch (System.Exception ex)
            {
                ClosePeer();
#if DEBUG
                Debug.WriteLine("Process_Receive : " + ex.Message);
#endif
            }
        }
        private void Init()
        {
            try
            {
                _stateObject = new StateObject();
                _receive_Args = new SocketAsyncEventArgs();
                _receive_Args.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
                _receive_Args.SetBuffer(new byte[StateObject.BufferSize], 0, StateObject.BufferSize);
                _receive_Args.UserToken = _stateObject;
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
                try
                {
                    Monitor.Enter(this);
                    if (_receive_Args != null)
                    {
                        _receive_Args.SocketError = SocketError.Shutdown;
                        _stateObject.Init();
                    }
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                DisconnectedEvent();
            }
        }
        public bool IsConnect()
        {
            if (_stateObject == null) return false;
            if (_stateObject.WorkSocket == null) return false;
            return _stateObject.WorkSocket.Connected;
        }
        public void Send(Packet packet)
        {
            try
            {
                if (_stateObject.WorkSocket == null)
                {
                    throw new Exception.Exception(ErrorCode.SocketDisConnect, "");
                }
                if (packet == null) return;
                _stateObject.Send(packet);
            }
            catch (Exception.Exception ex)
            {
#if DEBUG
                Debug.WriteLine("Send Exception : " + ex.GetErrorMessage());
#endif
                ClosePeer();
            }
            catch (System.Exception ex)
            {
#if DEBUG
                ClosePeer();
#endif
                Debug.WriteLine("Send Exception : " + ex.Message);
            }
        }
    }
}
