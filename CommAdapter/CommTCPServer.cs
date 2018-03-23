using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CommAdapter
{
    public class CommTCPServer : CommBaseAdapter
    {
        private CommState mCommState = new CommState();
        private bool mIsOpen { get; set; }
        private string mAddress { get; set; }
        private int mPort { get; set; }
        private TcpListener mTcpListener { get; set; }
        private Dictionary<string, CommTCPClient> mCommTCPClientList = new Dictionary<string, CommTCPClient>();
        private Thread mTCPClientListenerThread { get; set; }

        public override int Connect(string address, int port)
        {
            try
            {
                mCommState.Address = address;
                mCommState.Port = port;

                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);   // 監聽指定的Port上的所有IP(IPAddress.Any)
                mTcpListener = new TcpListener(ipEndPoint);
                mTcpListener.Start();

                mTCPClientListenerThread = new Thread(new ThreadStart(TCPClientListenerThread));
                mTCPClientListenerThread.Start();
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
                if (mTcpListener != null)
                {
                    foreach (string key in mCommTCPClientList.Keys)
                    {
                        mCommTCPClientList[key].DisconnectEvent -= Client_DisconnectEvent;
                        mCommTCPClientList[key].DataReceivedEvent -= Client_DataReceivedEvent;
                        mCommTCPClientList[key].Disconnect();
                    }
                    mCommTCPClientList.Clear();
                    mTcpListener.Stop();
                }
                if (mTCPClientListenerThread != null)
                {
                    mTCPClientListenerThread.Abort();
                    mTCPClientListenerThread = null;
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
            // 送出訊息廣播所有ip和port上
            foreach (string key in mCommTCPClientList.Keys)
            {
                return this.Send(data, mCommTCPClientList[key]);
            }
            return 0;
        }

        public override int Send(byte[] data, string address, int port)
        {
            string key = string.Format("{0}:{1}", address, port);
            if (!mCommTCPClientList.ContainsKey(key)) return -1;

            return this.Send(data, mCommTCPClientList[key]); // 送出訊息到指定的ip和port上
        }

        public int Send(byte[] data, CommTCPClient tcpClient)
        {
            return tcpClient.Send(data); 
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

        private void TCPClientListenerThread()
        {
            while (mCommState.IsOpen)
            {
                try
                {
                    //建立與客戶端的連線
                    TcpClient tcpClient = mTcpListener.AcceptTcpClient();

                    if (tcpClient.Connected)
                    {
                        EndPoint remoteEndPoint = tcpClient.Client.RemoteEndPoint;
                        string address = ((IPEndPoint)remoteEndPoint).Address.ToString();
                        int port  = ((IPEndPoint)remoteEndPoint).Port;
                        string key = string.Format("{0}:{1}", address, port);
                        if (mCommTCPClientList.ContainsKey(key))
                        {
                            mCommTCPClientList[key].DisconnectEvent -= Client_DisconnectEvent;
                            mCommTCPClientList[key].DataReceivedEvent -= Client_DataReceivedEvent;
                            mCommTCPClientList[key].Disconnect();
                            mCommTCPClientList.Remove(key);
                        }
                        CommTCPClient client = new CommTCPClient(tcpClient);
                        client.DisconnectEvent += Client_DisconnectEvent;
                        client.DataReceivedEvent += Client_DataReceivedEvent;
                        mCommTCPClientList.Add(key, client);

                        DeviceConnect(address, port, DateTime.Now);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void Client_DataReceivedEvent(List<byte> data, string address, int port, DateTime dateTime)
        {
            DataReceived(data, address, port, dateTime);
        }

        private void Client_DisconnectEvent(string address, int port, DateTime dateTime)
        {
            string key = string.Format("{0}:{1}", address, port);
            if (mCommTCPClientList.ContainsKey(key))
            {
                mCommTCPClientList[key].DisconnectEvent -= Client_DisconnectEvent;
                mCommTCPClientList[key].DataReceivedEvent -= Client_DataReceivedEvent;
                mCommTCPClientList.Remove(key);
            }
            DeviceDisconnect(address, port, dateTime);
        }
    }
}
