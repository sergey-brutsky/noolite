using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ThinkingHome.NooLite.SerialPort
{
    public class UnixSerialDevice : SerialDevice
    {
        public const int READING_BUFFER_SIZE = 1024;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private CancellationToken CancellationToken => cts.Token;

        private int? fd;
        private readonly IntPtr readingBuffer = Marshal.AllocHGlobal(READING_BUFFER_SIZE);

        public UnixSerialDevice(string portName, BaudRate baudRate) : base(portName, baudRate)
        {
        }

        public override void Open()
        {
            // open serial port
            int fd = Libc.open(portName, Libc.OpenFlags.O_RDWR | Libc.OpenFlags.O_NONBLOCK);

            if (fd == -1)
            {
                throw new Exception($"failed to open port ({portName})");
            }

            // set baud rate
            byte[] termiosData = new byte[256];

            Libc.tcgetattr(fd, termiosData);
            Libc.cfsetspeed(termiosData, baudRate);
            Libc.tcsetattr(fd, 0, termiosData);

            // start reading
            Task.Run((Action)StartReading, CancellationToken);

            this.fd = fd;
        }

        private void StartReading()
        {
            if (!fd.HasValue)
            {
                throw new Exception();
            }

            while (true)
            {
                CancellationToken.ThrowIfCancellationRequested();

                int res = Libc.read(fd.Value, readingBuffer, READING_BUFFER_SIZE);

                if (res != -1)
                {
                    byte[] buf = new byte[res];
                    Marshal.Copy(readingBuffer, buf, 0, res);

                    OnDataReceived(buf);
                }

                Thread.Sleep(50);
            }
        }

        public override bool IsOpened => fd.HasValue;

        public override void Close()
        {
            if (!fd.HasValue)
            {
                throw new Exception();
            }

            cts.Cancel();
            Libc.close(fd.Value);
            Marshal.FreeHGlobal(readingBuffer);
        }

        public override void Write(byte[] buf)
        {
            if (!fd.HasValue)
            {
                throw new Exception();
            }

            IntPtr ptr = Marshal.AllocHGlobal(buf.Length);
            Marshal.Copy(buf, 0, ptr, buf.Length);
            Libc.write(fd.Value, ptr, buf.Length);
            Marshal.FreeHGlobal(ptr);
        }
    }
}