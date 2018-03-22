using CommAdapterDemo.ViewModel;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommAdapterDemo
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static MainWindow MAINWINDOW { get; set; }

        public static void ExitAPP()
        {
            if (MAINWINDOW != null)
            {
                MAINWINDOW.EnableExitAPP = true;
                MAINWINDOW.Close();
            }
        }

        private bool EnableExitAPP = false;

        public MainWindow()
        {
            MAINWINDOW = this;
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        /// <summary>
        /// 視窗準備關閉之事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!EnableExitAPP) // 決定可否關閉視窗之條件
            {
                e.Cancel = true;
            }
        }
    }
}
