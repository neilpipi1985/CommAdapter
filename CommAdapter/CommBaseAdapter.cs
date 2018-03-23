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
        TCPClient,
        TCPServer
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
        public static int RECEIVED_BUFFER_SIZE = 8192;
        public delegate void DataReceivedHandler(List<byte> data, string address, int port, DateTime dateTime);
        public delegate void CommStateHandler(string address, int port, DateTime dateTime);

        public string Address { get { return GetAddress(); } }
        public int Port { get { return GetPort(); } }
        public bool IsOpen { get { return GetOpen(); } }
        public event DataReceivedHandler DataReceivedEvent;
        public event CommStateHandler ConnectEvent;
        public event CommStateHandler DisconnectEvent;

        protected abstract string GetAddress();
        protected abstract int GetPort();
        protected abstract bool GetOpen();

        public abstract int Disconnect();
        public abstract int Connect(string address, int port);
        public abstract int Send(byte[] data);
        public abstract int Send(byte[] data, string address, int port);

        protected void DataReceived(List<byte> data, string address, int port, DateTime dateTime)
        {
            if (DataReceivedEvent != null && data.Count > 0)
            {
                DataReceivedEvent(data, address, port, dateTime);
            }
        }

        protected void DeviceConnect(string address, int port, DateTime dateTime)
        {
            if (ConnectEvent != null)
            {
                ConnectEvent(address, port, dateTime);
            }
        }
 
        protected void DeviceDisconnect(string address, int port, DateTime dateTime)
        {
            if (DisconnectEvent != null)
            {
                DisconnectEvent(address, port, dateTime);
            }
        }
    }
}
