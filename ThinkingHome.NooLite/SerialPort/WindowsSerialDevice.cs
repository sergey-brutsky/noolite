using System;

namespace ThinkingHome.NooLite.SerialPort
{
    public class WindowsSerialDevice : SerialDevice
    {
        private int? handle;

        public WindowsSerialDevice(string portName, BaudRate baudRate) : base(portName, baudRate)
        {
        }

        public override void Open()
        {
            // open serial port
            int handle = Kernel32.CreateFile ($"\\\\.\\{portName}",
                Kernel32.GenericRead | Kernel32.GenericWrite, 0, 0, Kernel32.OpenExisting,
                Kernel32.FileFlagOverlapped, 0);

            if (handle == -1)
            {
                throw new Exception($"failed to open port ({portName})");
            }

            // set baud rate
            var  dcb = new Kernel32.DCB ();
            if (!Kernel32.GetCommState(handle, dcb)) throw new Exception($"failed to get state of port ({portName})");

            dcb.baud_rate = (int)baudRate;
            if (!Kernel32.SetCommState (handle, dcb)) throw new Exception($"failed to set state of port ({portName})");

            this.handle = handle;
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
            throw new NotImplementedException();
        }

        public override bool IsOpened => handle.HasValue;

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
