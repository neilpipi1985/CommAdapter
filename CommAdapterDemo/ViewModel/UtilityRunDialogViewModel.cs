using CommAdapter;
using CommAdapterDemo.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CommAdapterDemo.ViewModel
{
    public class UtilityRunDialogViewModel : INotifyPropertyChanged
    {
        public ICommand ConfirmCommand { get { return new MainWindowCommand(Confirm); } }
        public ICommand CancelCommand { get { return new MainWindowCommand(Canecl); } }

        private string[] mPortList = System.IO.Ports.SerialPort.GetPortNames();
        private CommState mCommState { get; set; }
        private ObservableCollection<string> mModeList = new ObservableCollection<string>();
        private ObservableCollection<string> mCOMPortList = new ObservableCollection<string>();
        private string mMode = "";
        private string mAddress = "";
        private int mPort = 0;
        private string mErrorMessage = "";

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<string> COMPortList
        {
            get { return mCOMPortList; }
            set { mCOMPortList = value; NotifyPropertyChanged("COMPortList"); }
        }
        public ObservableCollection<string> ModeList
        {
            get { return mModeList; }
            set { mModeList = value; NotifyPropertyChanged("ModeList"); }
        }
        public string Mode
        {
            get { return mMode; }
            set
            {
                mMode = value;
                if (mMode != CommMode.SerialPort.ToString())
                {
                    Address = "";
                }
                else
                {
                    Address = (mCOMPortList.Count > 0) ? mCOMPortList [0] : "";
                }
                NotifyPropertyChanged("EnableSerialPortVisibility");
                NotifyPropertyChanged("EnableIPAddressVisibility");
                NotifyPropertyChanged("Mode");
            }
        }
        public string Address
        {
            get { return mAddress; }
            set {
                mAddress = value;
                NotifyPropertyChanged("Address");
            }
        }
        public int Port
        {
            get { return mPort; }
            set
            {
                mPort = value;
                NotifyPropertyChanged("Port");
            }
        }
        public string ErrorMessage
        {
            get { return mErrorMessage; }
            set {
                mErrorMessage = value;
                NotifyPropertyChanged("ErrorMessage");
            }
        }
        public System.Windows.Visibility EnableSerialPortVisibility
        {
            get { return (mMode == CommMode.SerialPort.ToString()) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
            set { NotifyPropertyChanged("EnableSerialPortVisibility"); }
        }
        public System.Windows.Visibility EnableIPAddressVisibility
        {
            get { return (mMode != CommMode.SerialPort.ToString()) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
            set { NotifyPropertyChanged("EnableIPAddressVisibility"); }
        }

        public UtilityRunDialogViewModel(CommState state)
        {
            mCommState = state;
            mPort = mCommState.Port;
            SetModeList();
            SetCOMPortList();

            NotifyPropertyChanged("Mode");
            NotifyPropertyChanged("Address");
            NotifyPropertyChanged("Port");
            NotifyPropertyChanged("ErrorMessage");
            NotifyPropertyChanged("EnableSerialPortVisibility");
            NotifyPropertyChanged("EnableIPAddressVisibility");
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyPropertyChanged(params string[] propertyNames)
        {
            if (PropertyChanged != null) foreach (string propertyName in propertyNames) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetModeList()
        {
            mModeList.Clear();

            string matchTargetStr = "";
            string[] modeList = Enum.GetNames(typeof(CommMode)).ToArray();
            foreach (string str in modeList)
            {
                mModeList.Add(str);
                mMode = str;
                if (mCommState.Mode.ToString() == str) matchTargetStr = str;
            }

            if (matchTargetStr != "") mMode = matchTargetStr;
            ModeList = mModeList;
        }

        private void SetCOMPortList()
        {
            mCOMPortList.Clear();

            string matchTargetStr = "";
            foreach (string str in mPortList)
            {
                mCOMPortList.Add(str);
                mAddress = str;
                if (mCommState.Address == str) matchTargetStr = str;
            }

            if (matchTargetStr != "") mAddress = matchTargetStr;
            if (mMode != CommMode.SerialPort.ToString())
            {
                mAddress = mCommState.Address;
            }
            COMPortList = mCOMPortList;
        }

        private void Confirm(object obj)
        {
            if (Address == "" || Port == 0)
            {
                ErrorMessage = "Please Check Address and Port";
                return;
            }

            if (Mode != CommMode.SerialPort.ToString())
            {
                IPAddress address;
                if (!IPAddress.TryParse(Address, out address))
                {
                    // Valid IP, with address containing the IP
                    ErrorMessage = "Invalid IP Address";
                    return;
                }

                if (Port > 65535)
                {
                    // Valid IP, with address containing the IP
                    ErrorMessage = "Invalid Port";
                    return;
                }
            }

            mCommState.Mode = (CommMode)(Enum.Parse(typeof(CommMode), Mode, false));
            mCommState.Address = Address;
            mCommState.Port = Port;
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(mCommState, null);
        }

        private void Canecl(object obj)
        {
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
