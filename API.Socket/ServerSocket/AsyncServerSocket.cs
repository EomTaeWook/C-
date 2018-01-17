using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using API.Socket.Data.Packet;
using API.Socket.Data;
using System.Diagnostics;
using API.Socket.Exception;
using System.Net;
using System.Collections.Generic;

namespace API.Socket
{
    public abstract class AsyncServerSocket : ServerBase
    {
        private bool isRunning;
        private ManualResetEvent allDone;
        private MemoryPool<Packet> packetPool;
        private MemoryPool<SocketAsyncEventArgs> socketArgPool;
        private SocketAsyncEventArgs accept_Args;
        private Thread thread;
        private IPEndPoint iPEndPoint;
        private Dictionary<ulong, StateObject> clientList;
        private readonly object readMutex;
        private readonly object deleteMutex;
        public AsyncServerSocket() : this(1000)
        {
        }
        public AsyncServerSocket(int poolCount)
        {
            clientList = new Dictionary<ulong, StateObject>();
            readMutex = new object();
            deleteMutex = new object();
            isRunning = false;
            allDone = new ManualResetEvent(false);
            packetPool = new MemoryPool<Packet>(poolCount * 2);
            socketArgPool = new MemoryPool<SocketAsyncEventArgs>(poolCount, false);
            socketArgPool.Init(0, CreateSockEventArg);
        }
        protected void SetPacketMemoryPool(int count)
        {
            if (count > packetPool.Count)
            {
                packetPool.Init(count - socketArgPool.Count, null);
            }
        }
        protected void SetSocketMemoryPool(int count)
        {
            if (count > socketArgPool.Count)
            {
                socketArgPool.Init(count - socketArgPool.Count, CreateSockEventArg);
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
                    Debug.WriteLine("handler {0}: Read {1}", state.Handle, e.BytesTransferred);
                    state.Queue.Append(e.Buffer.Take(e.BytesTransferred).ToArray());
                    if (state.Queue.Count() >= Packet.HeaderSize)
                    {
                        var b = state.Queue.Peek(0, Packet.HeaderSize);
                        SHeader header = ObjectConverter.BytesToObject<SHeader>(b);
                        if (state.Queue.Count() >= header.DataSize)
                        {
                            b = state.Queue.Read(Convert.ToUInt32(header.DataSize));
                            Packet packet = packetPool.Pop();
                            packet.SetHeader(header);
                            packet.Data = b.Skip(Packet.HeaderSize).ToArray();
                            if (state.PacketQueue.Count() <= 0)
                            {
                                state.PacketQueue.Append(packet);
                                ThreadPool.QueueUserWorkItem(new WaitCallback(Work), state);
                            }
                            else
                            {
                                state.PacketQueue.Append(packet);
                            }
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
                    ClosePeer(state);
                }
            }
            catch (SocketException ex)
            {
                ClosePeer(state);
                Debug.WriteLine("ReadCallback SocketException : Handler : " + state.Handle + " Close Message : " + ex.Message);
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("ReadCallback System.Exception : Handler : " + state.Handle + " Close Message : " + ex.Message);
            }
        }
        public sealed override void Close()
        {
            isRunning = false;
            allDone.WaitOne();
            if (clientList != null)
            {
                try
                {
                    Monitor.Enter(clientList);
                    foreach(var c in clientList)
                    {
                        var arg = c.Value.ReceiveAsync;
                        arg.SocketError = SocketError.Shutdown;
                        socketArgPool.Push(arg);
                        c.Value.Init();
                        c.Value.Dispose();
                    }
                }
                finally
                {
                    Monitor.Exit(clientList);
                }
            }
            packetPool.Dispose();
            socketArgPool.Dispose();
            base.Close();
            clientList = null;
        }
        private void StartListening()
        {
            isRunning = true;
            System.Net.Sockets.Socket listener = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(iPEndPoint);
                listener.Listen(200);
                accept_Args = new SocketAsyncEventArgs();
                accept_Args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
                bool accept = true;
                while (isRunning)
                {
                    accept_Args.AcceptSocket = null;
                    allDone.Reset();
                    try
                    {
                        accept = listener.AcceptAsync(accept_Args);
                    }
                    catch(System.Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        continue;
                    }
                    if (!accept)
                    {
                        Accept_Completed(null, accept_Args);
                    }
                    allDone.WaitOne();
                }
                Debug.WriteLine("Server Start");
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine(ex.GetErrorMessage());
            }
        }
        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            StateObject state = null;
            if (e.SocketError == SocketError.Success)
            {
                var arg = socketArgPool.Pop();
                state = (arg.UserToken as StateObject);
                try
                {
                    System.Net.Sockets.Socket handler = e.AcceptSocket;
                    state.Handle = acceptCount.CountAdd();
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
                    Debug.WriteLine("Accept Exception : " + ex.Message);
                    ClosePeer(state);
                }
                catch (System.Exception ex)
                {
                    Debug.WriteLine("Accept Exception : " + ex.Message);
                    ClosePeer(state);
                }
                finally
                {
                    allDone.Set();
                }
                AddPeer(state);
            }
        }
        private void AddPeer(StateObject state)
        {
            try
            {
                if (clientList != null)
                {
                    try
                    {
                        Monitor.Enter(readMutex);
                        if (clientList.ContainsKey(state.Handle))
                        {
                            return;
                        }
                        clientList.Add(state.Handle, state);
                    }
                    finally
                    {
                        Monitor.Exit(readMutex);
                    }
                }
                AcceptComplete(state);
            }
            catch (Exception.Exception ex)
            {
                throw new Exception.Exception(ex.Message);
            }
        }
        public override void Reject(StateObject handler)
        {
            ClosePeer(handler);
        }
        protected void ClosePeer(StateObject state)
        {
            try
            {
                if (clientList != null)
                {
                    ulong handle = state.Handle;
                    try
                    {
                        Monitor.Enter(deleteMutex);
                        if (clientList.ContainsKey(handle))
                        {
                            clientList.Remove(handle);
                        }
                        var arg = state.ReceiveAsync;
                        arg.SocketError = SocketError.Shutdown;
                        socketArgPool.Push(arg);
                        state.Init();
                    }
                    finally
                    {
                        Monitor.Exit(deleteMutex);
                    }
                    DisconnectedComplete(handle);
                }
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine(ex.GetErrorMessage());
            }
        }
        private void Work(object obj)
        {
            StateObject state = (StateObject)obj;
            try
            {
                while (state.PacketQueue.Count() > 0)
                {
                    Packet packet = state.PacketQueue.Read();
                    object[] arg = null;
                    if (!PacketConversionComplete(packet, state, out arg))
                    {
                        Debug.WriteLine("PacketConversionComplete False");
                        continue;
                    }
                    if (packet.GetHeader().Tag != '~')
                    {
                        Debug.WriteLine("Header Tag Wrong!");
                        continue;
                    }
                    switch (VerifyPacket(packet, state))
                    {
                        case Data.Enum.VertifyResult.Vertify_Reject:
                            {
                                Debug.WriteLine("VertifyResult.Vertify_Reject");
                                Reject(state);
                                return;
                            }
                        case Data.Enum.VertifyResult.Vertify_Ignore:
                            {
                                Debug.WriteLine("VertifyResult.Vertify_Ignore");
                                continue;
                            }
                        case Data.Enum.VertifyResult.Vertify_Forward:
                            {
                                ForwardFunc(packet, state);
                                continue;
                            }
                    }
                    if (RunCallbackFunc(packet.GetHeader().Protocol, packet, state, arg))
                    {
                        CallbackComplete(packet, state);
                    }
                    packetPool.Push(packet);
                }
            }
            catch (SocketException ex)
            {
                ClosePeer(state);
                Debug.WriteLine(ex.Message);
            }
            catch (Exception.Exception ex)
            {
                Debug.WriteLine(ex.GetErrorMessage());
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void Init(string ip, int port)
        {
            try
            {
                if (ip == "") iPEndPoint = new IPEndPoint(IPAddress.Any, port);
                else iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
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
                if (thread == null)
                {
                    thread = new Thread(StartListening);
                    thread.Start();
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
        public override void Send(StateObject handler, Packet packet)
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
            catch (Exception.Exception ex)
            {
                Debug.WriteLine("Send Exception : " + ex.GetErrorMessage());
                ClosePeer(handler);
            }
            catch (System.Exception ex)
            {
                ClosePeer(handler);
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
