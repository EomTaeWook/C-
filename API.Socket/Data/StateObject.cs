
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
        private BufferQueue<byte> _recieveQueue = null;
        private BufferQueue<Packet.Packet> _packetQueue = null;
        private BufferQueue<Packet.Packet> _sendQueue = null;

        public System.Net.Sockets.Socket WorkSocket { get => _workSocket; set => _workSocket = value; }
        public BufferQueue<byte> Queue { get => _recieveQueue; }
        public BufferQueue<Packet.Packet> PacketQueue { get => _packetQueue; }
        public ulong Handle { get => _handle; set => _handle = value; }

        private SocketAsyncEventArgs _receiveAsync = null;
        public SocketAsyncEventArgs ReceiveAsync
        {
            get { return _receiveAsync; }
            set => _receiveAsync = value;
        }
        public StateObject()
        {
            _recieveQueue = new BufferQueue<byte>();
            _packetQueue = new BufferQueue<Packet.Packet>();
            _sendQueue = new BufferQueue<Packet.Packet>();
        }
        public void Init()
        {
            lock (this)
            {
                if (!_recieveQueue.IsDisposable)
                    _recieveQueue.Clear();
                if (!_packetQueue.IsDisposable)
                    _packetQueue.Clear();
                if (!_sendQueue.IsDisposable)
                    _sendQueue.Clear();

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
                    _recieveQueue.Dispose();
                    _packetQueue.Dispose();
                    _sendQueue.Dispose();
                }
            }
            catch (Exception.Exception ex)
            {
                throw new Exception.Exception("State Dispose :" + ex.Message);
            }
        }
    }
}
