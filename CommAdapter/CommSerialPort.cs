using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace CommAdapter
{
    public class CommSerialPort : CommBaseAdapter
    {
        private SerialPort mSerialPort = new SerialPort();

        public override int Connect(string address, int port)
        {
            try
            {
                this.Disconnect();
                mSerialPort.PortName = address;
                mSerialPort.BaudRate = port;
                mSerialPort.DataReceived += SerialPortObj_DataReceived;
                mSerialPort.Open();
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
                mSerialPort.DataReceived -= SerialPortObj_DataReceived;
                mSerialPort.Close();
            }
            catch (Exception err)
            {
                return -1;
            }
            return 0;
        }

        public override int Send(byte[] data)
        {
            try
            {
                mSerialPort.DiscardOutBuffer();
                mSerialPort.Write(data, 0, data.Length);
            }
            catch
            {
                return -1;
            }
            return 0;
        }

        protected override string GetAddress()
        {
            return (mSerialPort.IsOpen) ? mSerialPort.PortName : "";
        }

        protected override bool GetOpen()
        {
            return mSerialPort.IsOpen;
        }

        protected override int GetPort()
        {
            return (mSerialPort.IsOpen) ? mSerialPort.BaudRate : 0;
        }

        private void SerialPortObj_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            List<byte> data = new List<byte>();
            SerialPort sp = (SerialPort)sender;
            while (sp.BytesToRead > 0)
            {
                byte[] buffer = new byte[CommBaseAdapter.RECEIVED_BUFFER_SIZE];
                Int32 receivedLen = sp.Read(buffer, 0, buffer.Length);
                Array.Resize(ref buffer, receivedLen);
                data.AddRange(buffer);
            }

            DataReceived(data, this.Address, this.Port);
        }
    }
}
