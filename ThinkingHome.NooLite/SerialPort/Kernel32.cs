using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
namespace ThinkingHome.NooLite.SerialPort
{
    public static class Kernel32
    {
        public const uint GenericRead = 0x80000000;
        public const uint GenericWrite = 0x40000000;
        public const uint OpenExisting = 3;
        public const uint FileFlagOverlapped = 0x40000000;

        [DllImport("kernel32", SetLastError = true)]
        public static extern int CreateFile(string port_name, uint desired_access,
            uint share_mode, uint security_attrs, uint creation, uint flags,
            uint template);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool GetCommState (int handle, [Out] DCB dcb);

        [DllImport ("kernel32", SetLastError=true)]
        public static extern bool SetCommState (int handle, DCB dcb);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle (int handle);

        [DllImport("kernel32", SetLastError = true)]
        static extern bool ReadFile (int handle, IntPtr buffer, int bytes_to_read, out int bytes_read, IntPtr overlapped);

        [DllImport("kernel32", SetLastError = true)]
        static extern bool WriteFile (int handle, IntPtr buffer, int bytes_to_write, out int bytes_written, IntPtr overlapped);

        [StructLayout (LayoutKind.Sequential)]
        public struct DCB
        {
            public int dcb_length;
            public int baud_rate;
            public int flags;
            public short w_reserved;
            public short xon_lim;
            public short xoff_lim;
            public byte byte_size;
            public byte parity;
            public byte stop_bits;
            public byte xon_char;
            public byte xoff_char;
            public byte error_char;
            public byte eof_char;
            public byte evt_char;
            public short w_reserved1;
        }
    }
}