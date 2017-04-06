using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ThinkingHome.NooLite.SerialPort
{
    public class WindowsSerialDevice : SerialDevice
    {
        public const int READING_BUFFER_SIZE = 1024;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken CancellationToken => cts.Token;

        private readonly IntPtr readingBuffer = Marshal.AllocHGlobal(READING_BUFFER_SIZE);
        private int? handle;

        public WindowsSerialDevice(string portName, BaudRate baudRate) : base(portName, baudRate)
        {
        }

        public override void Open()
        {
            // open serial port
            int handle = Kernel32.CreateFile ($"\\\\.\\{portName}",
                Kernel32.GenericRead | Kernel32.GenericWrite, 0, 0, Kernel32.OpenExisting, 0, 0);
//                Kernel32.FileFlagOverlapped, 0);

            if (handle == -1)
            {
                throw new Exception($"failed to open port ({portName})");
            }

            // set baud rate
            var  dcb = new Kernel32.DCB ();
            if (!Kernel32.GetCommState(handle, dcb)) throw new Exception($"failed to get state of port ({portName})");

            dcb.baud_rate = (int)baudRate;
            if (!Kernel32.SetCommState (handle, dcb)) throw new Exception($"failed to set state of port ({portName})");

            // set timeouts for non-blocking reading
            var timeouts = new Kernel32.Timeouts { ReadIntervalTimeout = Kernel32.Timeouts.MaxDWord };
            if (!Kernel32.SetCommTimeouts(handle, timeouts)) throw new Exception($"failed to set timeouts for port ({portName})");

            // start reading
            Task.Run((Action)StartReading, CancellationToken);

            this.handle = handle;
        }

        private void StartReading()
        {
            int bytes_read;

            if (!handle.HasValue)
            {
                throw new Exception();
            }

            while (true)
            {
                CancellationToken.ThrowIfCancellationRequested();

                if (Kernel32.ReadFile(handle.Value, readingBuffer, READING_BUFFER_SIZE, out bytes_read, IntPtr.Zero))
                {
                    // если нет ошибки

                    if (bytes_read > 0)
                    {
                        // если есть данные
                        byte[] buf = new byte[bytes_read];
                        Marshal.Copy(readingBuffer, buf, 0, bytes_read);

                        OnDataReceived(buf);
                    }
                }
                else
                {
                    // error

//                    if (Marshal.GetLastWin32Error() != FileIOPending)
//                        ReportIOError (null);

//                    if (!GetOverlappedResult(handle, write_overlapped, ref bytes_read, true))
//                        ReportIOError (null);
                }

                Thread.Sleep(50);
            }
        }

        public override void Close()
        {
            if (!handle.HasValue)
            {
                throw new Exception();
            }

            Kernel32.CloseHandle(handle.Value);
        }

        public override void Write(byte[] buf)
        {
            int bytes_written;

            IntPtr ptr = Marshal.AllocHGlobal(buf.Length);
            Marshal.Copy(buf, 0, ptr, buf.Length);

            if (!Kernel32.WriteFile(handle.Value, ptr, buf.Length, out bytes_written, IntPtr.Zero))
            {
                // проверяем ошибки
                // смотреть здесь http://bit.ly/2nL3Ei2 и здесь http://bit.ly/2nLdksM
//                if (Marshal.GetLastWin32Error() != FileIOPending)
//                    ReportIOError (null);
//
//                if (!GetOverlappedResult(handle, write_overlapped, ref bytes_written, true))
//                    ReportIOError (null);
            }

            Marshal.FreeHGlobal(ptr);
        }

        public override bool IsOpened => handle.HasValue;

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
