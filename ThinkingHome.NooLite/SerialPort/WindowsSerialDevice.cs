using System;

namespace ThinkingHome.NooLite.SerialPort
{
    public class WindowsSerialDevice : SerialDevice
    {
        public WindowsSerialDevice(string portName, BaudRate baudRate) : base(portName, baudRate)
        {
            throw new NotImplementedException();
        }

        public override void Open()
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buf)
        {
            throw new NotImplementedException();
        }

        public override bool IsOpened { get; }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
