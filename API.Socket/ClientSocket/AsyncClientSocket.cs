using API.Socket.Data;
using API.Socket.Data.Packet;
using API.Socket.Exception;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace API.Socket
{
    public abstract class AsyncClientSocket : ClientBase
    {
        private StateObject _stateObject;
        private IPEndPoint _remoteEP;
        private SocketAsyncEventArgs _receive_Args;
        private string ip;
        private int port;
        public AsyncClientSocket()
        {
            Init();
        }
        public sealed override void Close()
        {
            ClosePeer();
            _stateObject.Dispose();
            base.Close();
        }
        protected override void ClosePeer()
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
        public void Send(int protocol, string data)
        {
            if (data == null) return;
            byte[] b = System.Text.Encoding.Default.GetBytes(data);
            Send(Convert.ToUInt16(protocol), b);
        }
        public override void Send(int protocol, byte[] data)
        {
            try
            {
                if (_stateObject.WorkSocket == null)
                {
                    throw new Exception.Exception(ErrorCode.SocketDisConnect, "");
                }

                if (data == null) return;
                Packet packet = new Packet();
                packet.GetHeader().Protocol = Convert.ToUInt16(protocol);
                packet.GetHeader().DataSize = (UInt32)data.Length;
                packet.Data = data;
                _stateObject.Send(packet);
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine("Send Exception : " + ex.GetErrorMessage());
                ClosePeer();
            }
            catch (System.Exception ex)
            {
                ClosePeer();
                Debug.WriteLine("Send Exception : " + ex.Message);
            }
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
                Debug.WriteLine("Send Exception : " + ex.GetErrorMessage());
                ClosePeer();
            }
            catch (System.Exception ex)
            {
                ClosePeer();
                Debug.WriteLine("Send Exception : " + ex.Message);
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
                Debug.Write("Init Exception : " + ex.Message);
                throw new Exception.Exception(ex.Message);
            }
        }
        protected void ReConnect(int timeout = 5000)
        {
            Connect(ip, port, timeout);
        }
        public override void Connect(string ip, int port, int timeout = 5000)
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
                Debug.WriteLine(string.Format("ArgumentNullException : {0}", arg.ToString()));
                throw new Exception.Exception(arg.Message);
            }
            catch (SocketException se)
            {
                Debug.WriteLine(string.Format("SocketException : {0}", se.ToString()));
                throw new Exception.Exception(se.Message);
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(string.Format("SystemException : {0}", e.ToString()));
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
            {
                Process_Receive(e);
            }
        }
        private void Process_Receive(SocketAsyncEventArgs e)
        {
            try
            {
                StateObject state = e.UserToken as StateObject;
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    state.ReceiveBuffer.Append(e.Buffer.Take(e.BytesTransferred).ToArray());
                    Debug.WriteLine("handler {0}: Read {1}", state.Handle, e.BytesTransferred);
                    if (state.ReceiveBuffer.Count() >= Packet.HeaderSize)
                    {
                        var b = state.ReceiveBuffer.Peek(0, Packet.HeaderSize);
                        var packet = new Packet(b);
                        if (state.ReceiveBuffer.Count() >= packet.GetHeader().DataSize)
                        {
                            b = state.ReceiveBuffer.Read(Convert.ToUInt32(packet.GetHeader().DataSize));
                            packet.Data = b.Skip(Packet.HeaderSize).ToArray();
                            if (state.PacketBuffer.Count() <= 0)
                            {
                                state.PacketBuffer.Append(packet);
                                BeginWork(state);
                            }
                            else
                            {
                                state.PacketBuffer.Append(packet);
                            }
                        }
                        else
                        {
                            packet.Dispose();
                            packet = null;
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
                    Debug.WriteLine(string.Format("error {0},  transferred {1}", e.SocketError, e.BytesTransferred));
                    ClosePeer();
                }
            }
            catch (System.Exception ex)
            {
                ClosePeer();
                Debug.WriteLine("Process_Receive : " + ex.Message);
            }
        }
        private void Work(object _packet)
        {
            var packet = (Packet)_packet;
            try
            {
                object[] arg = null;
                if (!PacketConversionComplete(packet, out arg))
                {
                    Debug.WriteLine("PacketConversionComplete False");
                    return;
                }
                if (packet.GetHeader().Tag != '~')
                {
                    Debug.WriteLine("Header Tag Wrong!");
                    return;
                }
                switch (VerifyPacket(packet))
                {
                    case Data.Enum.VertifyResult.Vertify_Ignore:
                        {
                            Debug.WriteLine("VertifyResult.Vertify_Ignore");
                            return;
                        }
                    case Data.Enum.VertifyResult.Vertify_Forward:
                        {
                            ForwardFunc(packet);
                            return;
                        }
                }
                RunCallbackFunc(packet.GetHeader().Protocol, packet);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                packet.Dispose();
                packet = null;
            }
        }
        private void BeginWork(StateObject state)
        {
            while (state.PacketBuffer.Count() > 0)
            {
                Packet packet = state.PacketBuffer.Read();
                ThreadPool.QueueUserWorkItem(new WaitCallback(Work), packet);
            }
        }
        public bool IsConnect()
        {
            if (_stateObject == null) return false;
            if (_stateObject.WorkSocket == null) return false;
            return _stateObject.WorkSocket.Connected;
        }
    }
}
