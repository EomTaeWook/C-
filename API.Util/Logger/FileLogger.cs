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
        private readonly string _moduleName;
        private PriorityQueue<LogMessage> _queue;
        private FileStream _fs;
        private readonly object _sync;
        public FileLogger()
        {
            _moduleName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            _queue = new PriorityQueue<LogMessage>(Order.Descending);
            _sync = new object();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Invoke(null);
            _fs.Close();
        }

        public void Init(LoggerPeriod period = LoggerPeriod.Infinitely, string path = "")
        {
            _period = period;
            _path = path;
            if (string.IsNullOrEmpty(_path))
                _path = Environment.CurrentDirectory;
            _path += @"\Log";
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            CreateLogFile();
        }
        public void Write(string message)
        {
            try
            {
                Monitor.Enter(_sync);
                if (_queue.Count == 0)
                {
                    _queue.Append(new LogMessage()
                    {
                        Message = message
                    });
                    ThreadPool.QueueUserWorkItem(Invoke);
                }
                else
                    _queue.Append(new LogMessage() { Message = message });
            }
            finally
            {
                Monitor.Exit(_sync);
            }
        }
        private void WriteMessage(LogMessage message)
        {
            string format = $"[{message.CreateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}] { message.Message}\r\n";
            Trace.Write(format);
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
                while(_queue.Count > 0)
                {
                    var message = _queue.Peek();
                    if (message.CreateTime.Hour != _time.Hour)
                        break;
                    message = _queue.Read();
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
                while (_queue.Count > 0)
                {
                    var message = _queue.Peek();
                    if (message.CreateTime.Day != _time.Day)
                        break;
                    message = _queue.Read();
                    WriteMessage(message);
                }
                _fs.Close();
                CreateLogFile();
            }
        }
        private void Invoke(object state)
        {
            try
            {
                Monitor.Enter(_sync);
                while (_queue.Count > 0)
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
                    var message = _queue.Read();
                    WriteMessage(message);
                }
            }
            finally
            {
                Monitor.Exit(_sync);
            }
        }
    }
}
