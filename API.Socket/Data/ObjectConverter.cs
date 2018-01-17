using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace API.Socket.Data
{
    public class ObjectConverter
    {
        public static int GetObjectBytesSize<T>()
        {
            return Marshal.SizeOf(default(T));
        }
        public static byte[] ObjectToBytes<T>(T obj)
        {
            int datasize = Marshal.SizeOf(obj);
            IntPtr buff = Marshal.AllocHGlobal(datasize);
            Marshal.StructureToPtr(obj, buff, true);
            byte[] data = new byte[datasize];
            Marshal.Copy(buff, data, 0, datasize);
            Marshal.FreeHGlobal(buff);
            return data;
        }
        public static T BytesToObject<T>(byte[] data)
        {
            IntPtr ptr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, ptr, data.Length);
            var obj = (T)Marshal.PtrToStructure(ptr, typeof(T));
            Marshal.FreeHGlobal(ptr);

            if (Marshal.SizeOf(obj) != data.Length)
            {
                return default(T);
            }
            return obj;
        }
    }
}
