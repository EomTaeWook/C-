using API.Socket.Data.Packet;
using System;
using System.Diagnostics;
using System.Net.Sockets;
namespace API.Socket.Data
{
    public class StateObject : IDisposable
    {
        public const int BufferSize = 2048;

        protected ulong handle;

        private System.Net.Sockets.Socket workSocket = null;
        private BufferQueue<byte> recieveQueue = null;
        private BufferQueue<Packet.Packet> packetQueue = null;
        private BufferQueue<Packet.Packet> sendQueue = null;

        public System.Net.Sockets.Socket WorkSocket { get => workSocket; set => workSocket = value; }
        public BufferQueue<byte> Queue { get => recieveQueue; }
        public BufferQueue<Packet.Packet> PacketQueue { get => packetQueue; }
        public ulong Handle { get => handle; set => handle = value; }

        private SocketAsyncEventArgs receiveAsync = null;
        public SocketAsyncEventArgs ReceiveAsync
        {
            get { return receiveAsync; }
            set => receiveAsync = value;
        }
        public StateObject()
        {
            recieveQueue = new BufferQueue<byte>();
            packetQueue = new BufferQueue<Packet.Packet>();
            sendQueue = new BufferQueue<Packet.Packet>();
        }
        public void Init()
        {
            lock(this)
            {
                if(!recieveQueue.IsDisposable)
                    recieveQueue.Clear();
                if (!packetQueue.IsDisposable)
                    packetQueue.Clear();
                if (!sendQueue.IsDisposable)
                    sendQueue.Clear();

                handle = 0;
                receiveAsync = null;
                if(workSocket != null)
                {
                    lock(workSocket)
                    {
                        try
                        {
                            workSocket.Shutdown(SocketShutdown.Both);
                        }
                        catch (System.Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        workSocket.Close();
                        workSocket = null;
                    }
                }
            }
        }
        public bool IsConnect()
        {
            if(workSocket != null)
            {
                return workSocket.Connected;
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
                if (sendQueue.Count() <= 0)
                {
                    sendQueue.Append(packet);
                    BeginSend();
                    return;
                }
                sendQueue.Append(packet);
            }
            catch(System.Exception ex)
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
                Packet.Packet packet = sendQueue.Peek();
                WorkSocket.BeginSend(packet.GetBytes(), 0, packet.GetBytes().Length, 0, new AsyncCallback(SendCallback), this);                 
            }
            catch(System.Exception ex)
            {
                throw new Exception.Exception("State Send Exception :" + ex.Message);
            }            
        }
        private void SendCallback(IAsyncResult ar)
        {
            StateObject handler = (StateObject)ar.AsyncState;
            try
            {
                if(handler.WorkSocket != null)
                {
                    int bytesSent = handler.WorkSocket.EndSend(ar);
                    Debug.WriteLine(string.Format("Sent {0} bytes to client.", bytesSent));

                    if (sendQueue.Count() > 0)
                    {
                        var packet = sendQueue.Read();
                        packet.Dispose();
                        packet = null;
                    }
                    if (sendQueue.Count() > 0)
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
                lock(this)
                {
                    if(ReceiveAsync != null)
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
                    recieveQueue.Dispose();
                    packetQueue.Dispose();
                    sendQueue.Dispose();
                }
            }
            catch(Exception.Exception ex)
            {
                throw new Exception.Exception("State Dispose :" + ex.Message);
            }
        }
    }
}
