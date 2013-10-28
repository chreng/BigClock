using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BigClockGit {
    /// <summary>
    /// Interaction logic for TrayWindow.xaml
    /// </summary>
    public partial class TrayWindow : Window {

        public bool IsExit { get; set;  }
        public bool IsReset { get; set; }
        public bool IsSetAutoStart { get; set; }
        public bool IsRemoveAutoStart { get; set; }

        public TrayWindow(bool autoStartActive) {
            InitializeComponent();

            IsExit = false;
            IsReset = false;
            IsSetAutoStart = false;
            IsRemoveAutoStart = false;

            if (autoStartActive) {
                SetAutoStart.Visibility = System.Windows.Visibility.Hidden;
                RemoveAutoStart.Visibility = System.Windows.Visibility.Visible;
            } else {
                SetAutoStart.Visibility = System.Windows.Visibility.Visible;
                RemoveAutoStart.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e) {
            IsReset = true;
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e) {
            IsExit = true;
            this.Close();
        }

        private void SetAutoStart_Click(object sender, RoutedEventArgs e) {
            IsSetAutoStart = true;
            this.Close();
        }

        private void RemoveAutoStart_Click(object sender, RoutedEventArgs e) {
            IsRemoveAutoStart = true;
            this.Close();
        }
    }
}
