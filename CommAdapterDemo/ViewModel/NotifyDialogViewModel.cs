using CommAdapterDemo.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CommAdapterDemo.ViewModel
{
    public class NotifyDialogViewModel : INotifyPropertyChanged
    {
        public ICommand ConfirmCommand { get { return new MainWindowCommand(Confirm); } }
        public ICommand CancelCommand { get { return new MainWindowCommand(Canecl); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private string mNotifyTitle { get; set; }
        private string mNotifyMessage { get; set; }
        private bool mEnableYesNoQuestion { get; set; }

        public string NotifyTitle
        {
            get { return mNotifyTitle; }
            set
            {
                mNotifyTitle = value;
                NotifyPropertyChanged("NotifyTitle");
            }
        }
        public string NotifyMessage
        {
            get { return mNotifyMessage; }
            set
            {
                mNotifyMessage = value;
                NotifyPropertyChanged("NotifyMessage");
            }
        }
        public System.Windows.Visibility YesNoQuestionVisibility
        {
            get { return (mEnableYesNoQuestion) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
        }
        public bool EnableYesNoQuestion
        {
            get { return mEnableYesNoQuestion; }
            set
            {
                mEnableYesNoQuestion = value;
                NotifyPropertyChanged("YesNoQuestionVisibility");
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Confirm(object obj)
        {
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(true, null);
        }
        private void Canecl(object obj)
        {
            MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(false, null);
        }
    }
}
