using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CommAdapter
{
    public class CommUDP : CommBaseAdapter
    {
        private CommState mCommState = new CommState();
        private bool mIsOpen { get; set; }
        private string mAddress { get; set; }
        private int mPort { get; set; }
        private Socket mSocket { get; set; }
        private Thread mDataReceivedThread { get; set; }

        public override int Connect(string address, int port)
        {
            try
            {
                mCommState.Address = address;
                mCommState.Port = port;

                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);   // 監聽指定的Port上的所有IP(IPAddress.Any)
                UdpClient udpClient = new UdpClient();
                mSocket = udpClient.Client;
                mSocket.Bind(ipEndPoint);

                mDataReceivedThread = new Thread(new ThreadStart(DataReceivedThread));
                mDataReceivedThread.Start();
                mCommState.IsOpen = true;
            }
            catch (Exception err)
            {
                return -1;
            }
            return 0;
        }

        public override int Disconnect()
        {
            try
            {
                mCommState.IsOpen = false;
                mCommState.Address = "";
                mCommState.Port = 0;
                if (mSocket != null)
                {
                    mSocket.Close();
                }
                if (mDataReceivedThread != null)
                {
                    mDataReceivedThread.Abort();
                    mDataReceivedThread = null;
                }
            }
            catch (Exception err)
            {
                return -1;
            }
            return 0;
        }

        public override int Send(byte[] data)
        {
            return this.Send(data, IPAddress.Parse(this.Address), this.Port); // 送出訊息到指定的ip和port上
            // return this.Send(data, IPAddress.Any, this.Port); // 送出訊息廣播所有ip和port上
        }

        public int Send(byte[] data, IPAddress ipAddress, int port)
        {
            try
            {
                UdpClient udpClient = new UdpClient();
                udpClient.Send(data, data.Length, new IPEndPoint(ipAddress, port));
                udpClient.Close();
            }
            catch (Exception err)
            {
                return -1;
            }
            return 0;
        }

        protected override string GetAddress()
        {
            return mCommState.IsOpen ? mCommState.Address : "";
        }

        protected override bool GetOpen()
        {
            return mCommState.IsOpen;
        }

        protected override int GetPort()
        {
            return mCommState.IsOpen ? mCommState.Port : 0;
        }

        private void DataReceivedThread()
        {
            // 同一台電腦所有應用程式只能允許唯一的Port被監聽
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (mCommState.IsOpen)
            {
                byte[] buffer = new byte[CommBaseAdapter.RECEIVED_BUFFER_SIZE];
                int length = mSocket.ReceiveFrom(buffer, ref remoteEndPoint);
                Array.Resize(ref buffer, length);
 
                DataReceived(buffer.ToList(), ((IPEndPoint)remoteEndPoint).Address.ToString(), ((IPEndPoint)remoteEndPoint).Port);
            }

            mSocket.Close();
        }
    }
}
