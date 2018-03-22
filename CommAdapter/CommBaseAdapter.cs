using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommAdapter
{
    public enum CommMode
    {
        SerialPort,
        UDP,
        //TCP
    }

    public class CommState
    {
        public CommMode Mode { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public bool IsOpen { get; set; }

        public CommState()
        {
            IsOpen = false;
            Mode = CommMode.SerialPort;
            Address = "";
            Port = 115200;
        }
    }

    public abstract class CommBaseAdapter
    {
        public static int RECEIVED_BUFFER_SIZE = 1024;
        public delegate void DataReceivedHandler(List<byte> data, string address, int port, DateTime dateTime);

        public string Address { get { return GetAddress(); } }
        public int Port { get { return GetPort(); } }
        public bool IsOpen { get { return GetOpen(); } }
        public event DataReceivedHandler DataReceivedEvent;

        protected abstract string GetAddress();
        protected abstract int GetPort();
        protected abstract bool GetOpen();

        public abstract int Disconnect();
        public abstract int Connect(string address, int port);
        public abstract int Send(byte[] data);

        protected void DataReceived(List<byte> data, string address, int port)
        {
            if (DataReceivedEvent != null && data.Count > 0)
            {
                DataReceivedEvent(data, address, port, DateTime.Now);
            }
        }
    }
}
