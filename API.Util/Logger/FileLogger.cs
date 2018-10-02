using API.Util.Collections;
using API.Util.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace API.Util.Logger
{
    public class FileLogger : Singleton<FileLogger>
    {
        private DateTimeOffset _time;
        private LoggerPeriod _period;
        private string _path;
        private string _moduleName;
        private DoublePriorityQueue<IMessage> _queue;
        private FileStream _fs;
        private readonly object _append, _write;
        private Thread _thread;
        private AutoResetEvent _resetEvent;
        private CancellationTokenSource _cts;
        private bool _doWork;
        public FileLogger()
        {
            _queue = new DoublePriorityQueue<IMessage>(Order.Descending);
            _append = new object();
            _write = new object();
            _thread = new Thread(Invoke);
            _resetEvent = new AutoResetEvent(false);
            _cts = new CancellationTokenSource();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            _doWork = false;
        }
        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Close();
        }
        public void Close()
        {
            _resetEvent.Set();
            _cts.Cancel();
            _thread.Join();
            _fs.Close();
        }
        public void Init(LoggerPeriod period = LoggerPeriod.Infinitely, string moduleName = "", string path = "")
        {
            _period = period;
            _path = path;
            _moduleName = moduleName;
            if (string.IsNullOrEmpty(_path))
                _path = Environment.CurrentDirectory;
            if(string.IsNullOrEmpty(_moduleName))
                _moduleName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            _path += @"\Log";
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            CreateLogFile();
            _thread.Start();
        }
        public void Write(string message, DateTimeOffset? timeStamp = null)
        {
            Write(new LogMessage() { Message = message, TimeStamp = timeStamp ?? DateTime.Now });
        }
        public void Write(IMessage message)
        {
            if (_fs == null)
                throw new InvalidOperationException("FileLogger Not Initialization");
            _queue.Push(message);
            if (!_doWork)
                _resetEvent.Set();
        }
        private void WriteMessage(IMessage message)
        {
#if DEBUG
            string format = $"[{DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} { message.Message} \r\n";
            Trace.Write(format);
            var bytes = Encoding.UTF8.GetBytes(format);
#else
            var bytes = Encoding.UTF8.GetBytes($"[{DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} { message.Message} \r\n");
#endif
            _fs.Write(bytes, 0, bytes.Count());
            _fs.Flush();
        }
        private void CreateLogFile()
        {
            string fileName;
            switch(_period)
            {
                case LoggerPeriod.Infinitely:
                    fileName = "Log.log";
                    break;
                case LoggerPeriod.Day:
                    fileName = $"{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                    break;
                case LoggerPeriod.Hour:
                    fileName = $"{DateTime.Now.ToString("yyyy-MM-dd-HH")}.log";
                    break;
                default:
                    fileName = null;
                    break;
            }
            _time = DateTimeOffset.Now;
            if (string.IsNullOrEmpty(fileName))
                throw new InvalidOperationException("LoggerPeriod Not Initialization");
            _fs = new FileStream($@"{_path}\{_moduleName} {fileName}", FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096);
        }
        private void HourCompare()
        {
            if(DateTimeOffset.Now.Hour != _time.Hour)
            {
                while(_queue.ReadCount > 0)
                {
                    var message = _queue.Peek();
                    if (message.TimeStamp.Hour != _time.Hour)
                        break;
                    message = _queue.Pop();
                    WriteMessage(message);
                }
                _fs.Close();
                CreateLogFile();
            }
        }
        private void DayCompare()
        {
            if (DateTimeOffset.Now.Day != _time.Day)
            {
                while (_queue.ReadCount > 0)
                {
                    var message = _queue.Peek();
                    if (message.TimeStamp.Day != _time.Day)
                        break;
                    message = _queue.Pop();
                    WriteMessage(message);
                }
                _fs.Close();
                CreateLogFile();
            }
        }
        private void Invoke(object state)
        {
            while(!_cts.IsCancellationRequested)
            {
                _doWork = false;
                _resetEvent.WaitOne(5000);
                _doWork = true;
                while (_queue.AppendCount > 0)
                {
                    _queue.Swap();
                    while (_queue.ReadCount > 0)
                    {
                        switch (_period)
                        {
                            case LoggerPeriod.Day:
                                DayCompare();
                                break;
                            case LoggerPeriod.Hour:
                                HourCompare();
                                break;
                        }
                        WriteMessage(_queue.Pop());
                    }
                }
            }
        }
    }
}
