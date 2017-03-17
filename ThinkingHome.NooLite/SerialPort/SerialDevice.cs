using System;
using System.Runtime.InteropServices;

namespace ThinkingHome.NooLite.SerialPort
{
    public abstract class SerialDevice : IDisposable
    {
        protected readonly string portName;
        protected readonly BaudRate baudRate;

        protected SerialDevice(string portName, BaudRate baudRate)
        {
            this.portName = portName;
            this.baudRate = baudRate;
        }

        public abstract void Open();

        public abstract void Close();

        public abstract void Write(byte[] buf);

        public abstract bool IsOpened { get; }

        public event Action<object, byte[]> DataReceived;

        protected virtual void OnDataReceived(byte[] data)
        {
            DataReceived?.Invoke(this, data);
        }

        public virtual void Dispose()
        {
            if (IsOpened)
            {
                Close();
            }
        }

        public static SerialDevice Create(string portName, BaudRate baudRate)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsSerialDevice(portName, baudRate);
            }
            else
            {
                return new UnixSerialDevice(portName, baudRate);
            }
        }
    }
}
