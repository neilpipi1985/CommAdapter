using CommAdapterDemo.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CommAdapterDemo.ViewModel
{
    public class TransmissionTargetDialogViewModel : INotifyPropertyChanged
    {
        public ICommand OKCommand { get { return new MainWindowCommand(Confirm); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private bool mEnableBroadcast = false;
        private ObservableCollection<ClientInfo> mTargetList = new ObservableCollection<ClientInfo>();


        public System.Windows.Visibility EnableTableVisibility
        {
            get { return (!mEnableBroadcast) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
            set { NotifyPropertyChanged("EnableTableVisibility"); }
        }
        public ObservableCollection<ClientInfo> TargetList
        {
            get { return mTargetList; }
            set { mTargetList = value; NotifyPropertyChanged("TargetList"); }
        }
        public bool EnableBroadcast
        {
            get { return mEnableBroadcast; }
            set
            {
                mEnableBroadcast = value;
                NotifyPropertyChanged("EnableBroadcast");
                NotifyPropertyChanged("EnableTableVisibility");
            }
        }
        public TransmissionTargetDialogViewModel(bool enableBroadcast, Dictionary<string, ClientInfo> targetList)
        {
            EnableBroadcast = enableBroadcast;
            mTargetList.Clear();
            foreach (string key in targetList.Keys)
            {
                mTargetList.Add(targetList[key]);
            }

            TargetList = mTargetList;
            NotifyPropertyChanged("EnableBroadcast");
            NotifyPropertyChanged("EnableTableVisibility"); 
        }
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyPropertyChanged(params string[] propertyNames)
        {
            if (PropertyChanged != null) foreach (string propertyName in propertyNames) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Confirm(object obj)
        {
            if (EnableBroadcast)
            {
                foreach (ClientInfo info in TargetList)
                {
                    info.EnableTransmission = EnableBroadcast;
                }
            }
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(mEnableBroadcast, null);
        }
    }
}
