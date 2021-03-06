﻿using CommAdapter;
using CommAdapterDemo.Model;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CommAdapterDemo.ViewModel
{
    public class ClientInfo
    {
        public bool EnableTransmission { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
    }

    class MainWindowViewModel : INotifyPropertyChanged
    {
        public ICommand TransmissionTargetCommand { get { return new MainWindowCommand(TransmissionTarget); } }
        public ICommand TransmissionHEXCommand { get { return new MainWindowCommand(TransmissionHEX); } }
        public ICommand TransmissionASCIICommand { get { return new MainWindowCommand(TransmissionASCII); } }
        public ICommand ExcuteUtilityDialogCommand { get { return new MainWindowCommand(ExcuteUtilityDialog); } }
        public ICommand StopUtilityDialogCommand { get { return new MainWindowCommand(StopUtilityDialog); } }
        public ICommand ExitAppCommand { get { return new MainWindowCommand(ExitApp); } }

        public System.Windows.Visibility ConnectionVisibility
        {
            get { return (mCommAdapter != null && mCommAdapter.IsOpen) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
            set
            {
                NotifyPropertyChanged("ConnectionVisibility");
            }
        }

        public System.Windows.Visibility DisconnectionVisibility
        {
            get { return !(mCommAdapter != null && mCommAdapter.IsOpen) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
            set
            {
                NotifyPropertyChanged("DisconnectionVisibility");
            }
        }

        public string AppVersion
        {
            get { return string.Format("Ver: {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()); }
            set
            {
                NotifyPropertyChanged("AppVersion");
            }
        }

        public string CommStateMode
        {
            get { return string.Format("Mode: {0}", mCommState.Mode.ToString()); }
            set
            {
                NotifyPropertyChanged("CommStateMode");
            }
        }

        public string CommStateAddress
        {
            get { return string.Format("Address: {0}", mCommState.Address.ToString()); }
            set
            {
                NotifyPropertyChanged("CommStateAddress");
            }
        }

        public string CommStatePort
        {
            get { return string.Format("Port: {0}", mCommState.Port.ToString()); }
            set
            {
                NotifyPropertyChanged("CommStatePort");
            }
        }

        public string TransmissionMessage
        {
            get { return mTransmissionMessage; }
            set
            {
                mTransmissionMessage = value;
                NotifyPropertyChanged("TransmissionMessage");
            }
        }

        public string ReceivedMessage
        {
            get { return mReceivedMessage; }
            set
            {
                mReceivedMessage = value;
                NotifyPropertyChanged("ReceivedMessage");
            }
        }
        
        public System.Windows.Visibility EnableIPAddressVisibility
        {
            get { return (mCommState.Mode == CommMode.TCPServer) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
            set { NotifyPropertyChanged("EnableIPAddressVisibility"); }
        }
        
        public string Address
        {
            get { return mCommState.Address; }
            set
            {
                mCommState.Address = value;
                NotifyPropertyChanged("Address");
            }
        }

        public int Port
        {
            get { return mCommState.Port; }
            set
            {
                mCommState.Port = value;
                NotifyPropertyChanged("Port");
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string mTransmissionMessage = "";
        private string mReceivedMessage = "";

        private CommState mCommState = new CommState();
        private CommBaseAdapter mCommAdapter { get; set; }

        private bool mEnableBroadcast = true;
        private Dictionary<string, ClientInfo> mTCPClientList = new Dictionary<string, ClientInfo>();

        public MainWindowViewModel()
        {
            RefreshSettingUI();
        }

        private void RefreshSettingUI()
        {
            NotifyPropertyChanged("ConnectionVisibility");
            NotifyPropertyChanged("DisconnectionVisibility");
            NotifyPropertyChanged("EnableIPAddressVisibility");
            NotifyPropertyChanged("AppVersion");
            NotifyPropertyChanged("CommStateMode");
            NotifyPropertyChanged("CommStateAddress");
            NotifyPropertyChanged("CommStatePort");
        }

        private async void TransmissionTarget(object obj)
        {
            await DialogHost.Show(new TransmissionTargetDialog()
            {
                DataContext = new TransmissionTargetDialogViewModel(mEnableBroadcast, mTCPClientList)
            }, "RootDialog", async (object sender, DialogClosingEventArgs eventArgs) =>
            {
                if (eventArgs.Parameter == null) return;

                mEnableBroadcast = (bool)eventArgs.Parameter;
            });
        }

        private void TransmissionHEX(object obj)
        {
            if (mCommAdapter != null && mCommAdapter.IsOpen)
            {
            }
        }

        private void TransmissionASCII(object obj)
        {
            if (mCommAdapter != null && mCommAdapter.IsOpen)
            {
                byte[] data = System.Text.Encoding.ASCII.GetBytes(TransmissionMessage);
                if (mCommState.Mode != CommMode.TCPServer)
                {
                    mCommAdapter.Send(data, mCommState.Address, mCommState.Port);
                }
                else
                {
                    foreach (string key in mTCPClientList.Keys)
                    {
                        if (mTCPClientList[key].EnableTransmission)
                        {
                            mCommAdapter.Send(data, mTCPClientList[key].Address, mTCPClientList[key].Port);
                        }
                    }
                }

                mTransmissionMessage = "";
                TransmissionMessage = "";
            }
        }

        private async void ExcuteUtilityDialog(object obj)
        {
            await DialogHost.Show(new UtilityRunDialog()
            {
                DataContext = new UtilityRunDialogViewModel(mCommState)
            }, "RootDialog", async (object sender, DialogClosingEventArgs eventArgs) =>
            {
                if (eventArgs.Parameter == null) return;

                mCommState = (CommState)eventArgs.Parameter;

                switch (mCommState.Mode)
                {
                    case CommMode.SerialPort:
                        {
                            mCommAdapter = new CommSerialPort();
                            break;
                        }
                    case CommMode.UDP:
                        {
                            mCommAdapter = new CommUDP();
                            break;
                        }
                    case CommMode.TCPClient:
                        {
                            mCommAdapter = new CommTCPClient();
                            break;
                        }
                    case CommMode.TCPServer:
                        {
                            mCommAdapter = new CommTCPServer();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                if (mCommAdapter == null)
                {
                    await DialogHost.Show(new NotifyDialog()
                    {
                        DataContext = new NotifyDialogViewModel
                        {
                            EnableYesNoQuestion = false,
                            NotifyTitle = "Warning",
                            NotifyMessage = "Please check Mode!"
                        }
                    }, "NotifyDialog");

                    return;
                }

                int ret = mCommAdapter.Connect(mCommState.Address, mCommState.Port);
                if (ret != 0)
                {
                    await DialogHost.Show(new NotifyDialog()
                    {
                        DataContext = new NotifyDialogViewModel
                        {
                            EnableYesNoQuestion = false,
                            NotifyTitle = "Warning",
                            NotifyMessage = "Please check Address and Port!"
                        }
                    }, "NotifyDialog");

                    return;
                }

                TransmissionMessage = "";
                ReceivedMessage = "";

                mCommAdapter.ConnectEvent += CommAdapter_ConnectEvent;
                mCommAdapter.DisconnectEvent += CommAdapter_DisconnectEvent;
                mCommAdapter.DataReceivedEvent += CommAdapter_DataReceivedEvent;

                RefreshSettingUI();
            });
        }
        
        private async void StopUtilityDialog(object obj)
        {
            await DialogHost.Show(new NotifyDialog()
            {
                DataContext = new NotifyDialogViewModel()
                {
                    EnableYesNoQuestion = true,
                    NotifyTitle = "Warning",
                    NotifyMessage = "Do you want to cancel the task?"
                }
            }, "RootDialog", (object sender, DialogClosingEventArgs eventArgs) =>
            {
                if ((bool)eventArgs.Parameter == false) return;

                if (mCommAdapter != null)
                {
                    mCommAdapter.Disconnect();
                    mCommAdapter.DataReceivedEvent -= CommAdapter_DataReceivedEvent;
                    mCommAdapter.ConnectEvent -= CommAdapter_ConnectEvent;
                    mCommAdapter.DisconnectEvent -= CommAdapter_DisconnectEvent;
                    mCommAdapter = null;
                }
                RefreshSettingUI();
            });
        }

        private async void ExitApp(object obj)
        {
            await DialogHost.Show(new NotifyDialog()
            {
                DataContext = new NotifyDialogViewModel()
                {
                    EnableYesNoQuestion = true,
                    NotifyTitle = "Warning",
                    NotifyMessage = "Do you want to leave now?"
                }
            }, "RootDialog", (object sender, DialogClosingEventArgs eventArgs) =>
            {
                if ((bool)eventArgs.Parameter == false) return;

                if (mCommAdapter != null)
                {
                    mCommAdapter.Disconnect();
                    mCommAdapter.DataReceivedEvent -= CommAdapter_DataReceivedEvent;
                    mCommAdapter.ConnectEvent -= CommAdapter_ConnectEvent;
                    mCommAdapter.DisconnectEvent -= CommAdapter_DisconnectEvent;
                    mCommAdapter = null;
                }
                RefreshSettingUI();
                MainWindow.ExitAPP();
            });
        }

        private void CommAdapter_ConnectEvent(string address, int port, DateTime dateTime)
        {
            string key = string.Format("{0}{1}", address, port);
            if (!mTCPClientList.ContainsKey(key))
            {
                mTCPClientList.Add(key, new ClientInfo { Address = address, Port = port, EnableTransmission = mEnableBroadcast });
            }
        }

        private void CommAdapter_DisconnectEvent(string address, int port, DateTime dateTime)
        {
            string key = string.Format("{0}{1}", address, port);
            if (mTCPClientList.ContainsKey(key))
            {
                mTCPClientList.Remove(key);
            }
        }

        private void CommAdapter_DataReceivedEvent(List<byte> data, string address, int port, DateTime dateTime)
        {
            string str = string.Format("<{0}>({1}:{2}): {3}\r\n", dateTime.ToString("yyyy/MM/dd HH:mm:ss.fff"), address, port, System.Text.Encoding.ASCII.GetString(data.ToArray()));

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                () =>
                {
                    ReceivedMessage += str;
                }));
        }
    }
}
