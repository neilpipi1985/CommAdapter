using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CommAdapter
{
    public class CommTCPClient : CommBaseAdapter
    {
        private CommState mCommState = new CommState();
        private bool mIsOpen { get; set; }
        private string mAddress { get; set; }
        private int mPort { get; set; }
        private TcpClient mTcpClient { get; set; }
        private Thread mDataReceivedThread { get; set; }

        public CommTCPClient()
        {

        }

        public CommTCPClient(TcpClient tcpClient)
        {
            this.Connect(tcpClient);
        }

        public override int Connect(string address, int port)
        {
            try
            {
                mCommState.Address = address;
                mCommState.Port = port;

                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(address), port);   // 監聽指定的Port上的所有IP(IPAddress.Any)
                mTcpClient = new TcpClient();
                mTcpClient.Connect(ipEndPoint);
                mTcpClient.NoDelay = false;

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

        public int Connect(TcpClient tcpClient)
        {
            try
            {
                EndPoint remoteEndPoint = tcpClient.Client.RemoteEndPoint;
                mCommState.Address = ((IPEndPoint)remoteEndPoint).Address.ToString();
                mCommState.Port = ((IPEndPoint)remoteEndPoint).Port;

                mTcpClient = tcpClient;

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
                string address = this.Address;
                int port = this.Port;

                mCommState.IsOpen = false;
                mCommState.Address = "";
                mCommState.Port = 0;
                if (mTcpClient != null)
                {
                    mTcpClient.Close();
                }
                if (mDataReceivedThread != null)
                {
                    mDataReceivedThread.Abort();
                    mDataReceivedThread = null;
                }

                DeviceDisconnect(address, port, DateTime.Now);
            }
            catch (Exception err)
            {
                return -1;
            }
            return 0;
        }

        public override int Send(byte[] data)
        {
            return this.Send(data, mTcpClient); // 送出訊息到指定的ip和port上
            // return this.Send(data, IPAddress.Any, this.Port); // 送出訊息廣播所有ip和port上
        }

        public override int Send(byte[] data, string address, int port)
        {
            return this.Send(data, mTcpClient); // 送出訊息到指定的ip和port上
        }

        public int Send(byte[] data, TcpClient tcpClient)
        {
            try
            {
                NetworkStream netStream = tcpClient.GetStream();
                if (netStream.CanWrite)
                {
                    netStream.Write(data, 0, data.Length);
                }
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
            EndPoint remoteEndPoint = mTcpClient.Client.RemoteEndPoint;

            NetworkStream netStream = mTcpClient.GetStream();
            while (mCommState.IsOpen)
            {
                try
                {
                    if (netStream.CanRead)
                    {
                        byte[] buffer = new byte[CommBaseAdapter.RECEIVED_BUFFER_SIZE];

                        int length = netStream.Read(buffer, 0, (int)mTcpClient.ReceiveBufferSize);
                        Array.Resize(ref buffer, length);

                        remoteEndPoint = mTcpClient.Client.RemoteEndPoint;
                        DataReceived(buffer.ToList(), ((IPEndPoint)remoteEndPoint).Address.ToString(), ((IPEndPoint)remoteEndPoint).Port, DateTime.Now);
                    }
                }
                catch (Exception ex)
                {
                    this.Disconnect();
                }
            }
        }
    }
}
