using API.Util;
using System;
using System.Net.Sockets;
using System.Threading;

namespace API.Socket.Data
{
    public class StateObject : IDisposable
    {
        public const int BufferSize = 2048;

        protected ulong _handle;
        private SyncQueue<Packet> _sendBuffer = null;

        public System.Net.Sockets.Socket WorkSocket { get; set; } = null;
        public SyncQueue<byte> ReceiveBuffer { get; } = null;
        public SyncQueue<Packet> ReceivePacketBuffer { get; } = null;
        public ulong Handle { get => _handle; set => _handle = value; }

        private SocketAsyncEventArgs _ioEvent;
        public StateObject()
        {
            ReceiveBuffer = new SyncQueue<byte>();
            ReceivePacketBuffer = new SyncQueue<Packet>();
            _sendBuffer = new SyncQueue<Packet>();
            _ioEvent = new SocketAsyncEventArgs();
            _ioEvent.Completed += Send_Completed;
        }

        public void Init()
        {
            if (Monitor.TryEnter(WorkSocket))
            {
                try
                {
                    _handle = 0;

                    _ioEvent.SocketError = SocketError.OperationAborted;
                    WorkSocket.Shutdown(SocketShutdown.Send);
                    WorkSocket.Shutdown(SocketShutdown.Receive);
                    WorkSocket.Shutdown(SocketShutdown.Both);
                    WorkSocket.Close();
                    WorkSocket = null;
                    ReceiveBuffer.Clear();
                    ReceivePacketBuffer.Clear();
                    _sendBuffer.Clear();
                }
                finally
                {
                    Monitor.Exit(WorkSocket);
                }
            }
        }
        public void Send(Packet packet)
        {
            try
            {
                if (_sendBuffer.Count() <= 0)
                {
                    _sendBuffer.Append(packet);
                    BeginSend();
                    return;
                }
                _sendBuffer.Append(packet);
            }
            catch (System.Exception)
            {
            }
        }
        private void BeginSend()
        {
            bool pending = false;
            try
            {
                if (WorkSocket == null || !WorkSocket.Connected)
                {
                    throw new Exception.Exception(Exception.ErrorCode.SocketDisConnect, "");
                }
                byte[] packet = _sendBuffer.Peek().GetBytes();
                _ioEvent.SetBuffer(packet,0, packet.Length);
                pending = WorkSocket.SendAsync(_ioEvent);
            }
            catch (System.Exception ex)
            {
                throw new Exception.Exception("State Send Exception :" + ex.Message);
            }
            if (!pending)
                ProcessSend(_ioEvent);
        }
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            try
            {
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    _sendBuffer.Read().Dispose();
                    if (_sendBuffer.Count() > 0)
                    {
                        BeginSend();
                    }
                }
            }
            catch (System.Exception)
            {
            }
        }

        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
                ProcessSend(e);
            }
        }
        private void Dispose(bool IsDispose)
        {
            if (IsDispose)
                return;
            if (Monitor.TryEnter(this))
            {
                try
                {
                    IsDispose = true;
                    Init();
                    ReceiveBuffer.Dispose();
                    ReceivePacketBuffer.Dispose();
                    _sendBuffer.Dispose();
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
