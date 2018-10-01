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
        private DoublePriorityQueue<LogMessage> _queue;
        private FileStream _fs;
        private readonly object _append, _write;
        public FileLogger()
        {
            _queue = new DoublePriorityQueue<LogMessage>(Order.Descending);
            _append = new object();
            _write = new object();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Invoke(null);
            _queue.Swap();
            Invoke(null);
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
        }
        public void Write(string message)
        {
            if (_fs == null)
                throw new InvalidOperationException("FileLogger Not Initialization");
            try
            {
                Monitor.Enter(_append);
                _queue.AppendQueue.Push(new LogMessage() { Message = message });
                if (_queue.ReadQueue.Count == 0)
                {
                    _queue.Swap();
                    ThreadPool.QueueUserWorkItem(Invoke);
                }
            }
            finally
            {
                Monitor.Exit(_append);
            }
        }
        private void WriteMessage(LogMessage message)
        {
            string format = $"[{message.CreateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}] { message.Message}\r\n";
#if DEBUG
            Trace.Write(format);
#endif
            var bytes = Encoding.UTF8.GetBytes(format);
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
            _fs = new FileStream($@"{_path}\{_moduleName} {fileName}", FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        }
        private void HourCompare()
        {
            if(DateTimeOffset.Now.Hour != _time.Hour)
            {
                while(_queue.ReadQueue.Count > 0)
                {
                    var message = _queue.ReadQueue.Peek();
                    if (message.CreateTime.Hour != _time.Hour)
                        break;
                    message = _queue.ReadQueue.Pop();
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
                while (_queue.ReadQueue.Count > 0)
                {
                    var message = _queue.ReadQueue.Peek();
                    if (message.CreateTime.Day != _time.Day)
                        break;
                    message = _queue.ReadQueue.Pop();
                    WriteMessage(message);
                }
                _fs.Close();
                CreateLogFile();
            }
        }
        private void Invoke(object state)
        {
            if(Monitor.TryEnter(_write))
            {
                try
                {
                    while (_queue.ReadQueue.Count > 0)
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
                        WriteMessage(_queue.ReadQueue.Pop());
                    }
                }
                finally
                {
                    Monitor.Exit(_write);
                }
            }
        }
    }
}
