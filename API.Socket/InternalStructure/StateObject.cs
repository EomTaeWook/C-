using API.Socket.InternalStructure;
using API.Util;
using API.Util.Collections;
using System;
using System.Net.Sockets;
using System.Threading;

namespace API.Socket.InternalStructure
{
    public class StateObject : IDisposable
    {
        public static readonly int BufferSize = 2048;

        protected ulong _handle;
        private SyncQueue<IPacket> _sendBuffer = null;

        public System.Net.Sockets.Socket Socket { get; set; } = null;
        public SyncQueue<byte> ReceiveBuffer { get; } = null;
        public SyncQueue<IPacket> ReceivePacketBuffer { get; } = null;
        public ulong Handle { get => _handle; set => _handle = value; }

        private SocketAsyncEventArgs _ioEvent;
        private bool _disposed;
        public StateObject()
        {
            ReceiveBuffer = new SyncQueue<byte>();
            ReceivePacketBuffer = new SyncQueue<IPacket>();
            _sendBuffer = new SyncQueue<IPacket>();
            _ioEvent = new SocketAsyncEventArgs();
            _ioEvent.Completed += Send_Completed;
        }

        public void Init()
        {
            if (Monitor.TryEnter(this))
            {
                try
                {
                    _handle = 0;
                    _ioEvent.SocketError = SocketError.OperationAborted;
                    if (Socket != null)
                    {
                        Socket.Shutdown(SocketShutdown.Send);
                        Socket.Shutdown(SocketShutdown.Receive);
                        Socket.Shutdown(SocketShutdown.Both);
                        Socket.Close();
                        Socket = null;
                    }
                    ReceiveBuffer.Clear();
                    ReceivePacketBuffer.Clear();
                    _sendBuffer.Clear();
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }
        public void Send(IPacket packet)
        {
            try
            {
                if (_sendBuffer.Count <= 0)
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
                if (Socket == null || !Socket.Connected)
                    throw new Exception.Exception(Exception.ErrorCode.SocketDisConnect, "");

                byte[] packet = _sendBuffer.Peek().GetBytes();
                _ioEvent.SetBuffer(packet, 0, packet.Length);
                pending = Socket.SendAsync(_ioEvent);
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
                    if (_sendBuffer.Count > 0)
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
        protected virtual void Dispose(bool IsDispose)
        {
            if (Monitor.TryEnter(this))
            {
                try
                {

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
            if (_disposed)
                return;
            Dispose(true);
            _disposed = true;
        }
    }
}
