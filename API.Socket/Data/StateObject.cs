﻿
using API.Util;
using System;
using System.Diagnostics;
using System.Net.Sockets;
namespace API.Socket.Data
{
    public class StateObject : IDisposable
    {
        public const int BufferSize = 2048;

        protected ulong _handle;

        private System.Net.Sockets.Socket _workSocket = null;
        private SyncQueue<byte> _receiveBuffer = null;
        private SyncQueue<Packet.Packet> _packetBuffer = null;
        private SyncQueue<Packet.Packet> _sendQueue = null;

        public System.Net.Sockets.Socket WorkSocket { get => _workSocket; set => _workSocket = value; }
        public SyncQueue<byte> ReceiveBuffer { get => _receiveBuffer; }
        public SyncQueue<Packet.Packet> PacketBuffer { get => _packetBuffer; }
        public ulong Handle { get => _handle; set => _handle = value; }

        private SocketAsyncEventArgs _receiveAsync = null;
        public SocketAsyncEventArgs ReceiveAsync
        {
            get { return _receiveAsync; }
            set => _receiveAsync = value;
        }
        public StateObject()
        {
            _receiveBuffer = new SyncQueue<byte>();
            _packetBuffer = new SyncQueue<Packet.Packet>();
            _sendQueue = new SyncQueue<Packet.Packet>();

            IsDispose = false;
        }
        public void Init()
        {
            try
            {
                lock (this)
                {
                    _handle = 0;
                    _receiveAsync = null;
                    if (_workSocket != null)
                    {
                        lock (_workSocket)
                        {
                            try
                            {
                                _workSocket.Shutdown(SocketShutdown.Both);
                            }
                            catch (System.Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                            _workSocket.Close();
                            _workSocket = null;
                        }
                    }

                    if (!_receiveBuffer.IsDispose)
                        _receiveBuffer.Clear();
                    else
                        throw new ObjectDisposedException("RecieveQueue is Disposed");
                    if (!_packetBuffer.IsDispose)
                        _packetBuffer.Clear();
                    else
                        throw new ObjectDisposedException("PacketQueue is Disposed");
                    if (!_sendQueue.IsDispose)
                        _sendQueue.Clear();
                    else
                        throw new ObjectDisposedException("SendQueue is Disposed");
                }
            }
            catch (System.Exception ex)
            {
                throw ex;
            }
        }
        public bool IsConnect()
        {
            if (_workSocket != null)
            {
                return _workSocket.Connected;
            }
            else
            {
                return false;
            }
        }
        public void Send(Packet.Packet packet)
        {
            try
            {
                if (_sendQueue.Count() <= 0)
                {
                    _sendQueue.Append(packet);
                    BeginSend();
                    return;
                }
                _sendQueue.Append(packet);
            }
            catch (System.Exception ex)
            {
                throw new Exception.Exception(ex.Message);
            }
        }
        private void BeginSend()
        {
            try
            {
                if (WorkSocket == null || !WorkSocket.Connected)
                {
                    throw new Exception.Exception(Exception.ErrorCode.SocketDisConnect, "");
                }
                Packet.Packet packet = _sendQueue.Peek();
                WorkSocket.BeginSend(packet.GetBytes(), 0, packet.GetBytes().Length, 0, new AsyncCallback(SendCallback), this);
            }
            catch (System.Exception ex)
            {
                throw new Exception.Exception("State Send Exception :" + ex.Message);
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            StateObject handler = (StateObject)ar.AsyncState;
            try
            {
                if (handler.WorkSocket != null)
                {
                    int bytesSent = handler.WorkSocket.EndSend(ar);
                    Debug.WriteLine(string.Format("Sent {0} bytes to client.", bytesSent));

                    if (_sendQueue.Count() > 0)
                    {
                        var packet = _sendQueue.Read();
                        packet.Dispose();
                        packet = null;
                    }
                    if (_sendQueue.Count() > 0)
                    {
                        BeginSend();
                    }
                }
            }
            catch (SocketException ex)
            {
                Debug.WriteLine("Send SocketException : " + ex.Message);
            }
            catch (ObjectDisposedException ex)
            {
                Debug.WriteLine("Send ObjectDisposedException : " + ex.Message);
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine("Send Callback Exception : " + ex.GetErrorMessage());
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Send Callback System.Exception : " + ex.Message);
            }
        }
        public void Dispose()
        {
            if (IsDispose) return;
            try
            {
                lock (this)
                {
                    if (ReceiveAsync != null)
                    {
                        ReceiveAsync.SocketError = SocketError.Shutdown;
                        ReceiveAsync.SetBuffer(null, 0, 0);
                    }
                    if (WorkSocket != null)
                    {
                        WorkSocket.Shutdown(SocketShutdown.Both);
                        WorkSocket.Close();
                        WorkSocket = null;
                    }
                    _receiveBuffer.Clear();
                    _receiveBuffer.Dispose();

                    _packetBuffer.Clear();
                    _packetBuffer.Dispose();

                    _sendQueue.Clear();
                    _sendQueue.Dispose();

                    IsDispose = true;
                }
            }
            catch (Exception.Exception ex)
            {
                throw new Exception.Exception("State Dispose :" + ex.Message);
            }
        }
        public bool IsDispose { get; private set; }
    }
}
