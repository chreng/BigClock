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
using System.Windows.Threading;

namespace BigClockGit {
    /// <summary>
    /// Interaction logic for TrayWindow.xaml
    /// </summary>
    public partial class TrayWindow : Window {

        public bool IsExit { get; set;  }
        public bool IsReset { get; set; }
        public bool IsSetAutoStart { get; set; }
        public bool IsRemoveAutoStart { get; set; }
        public double TextFontSize { get; set; }

        private DispatcherTimer closeWindowTimer;

        public TrayWindow(bool autoStartActive, double textFontSize) {
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

            this.TextFontSize = Math.Round(textFontSize, 0);
            SliderTextFontSize.Value = this.TextFontSize;
            LabelTextFontSize.Text = this.TextFontSize.ToString();
            SliderTextFontSize.ValueChanged += SliderTextFontSize_ValueChanged;

            StartCloseWindowTimer();
        }

        private void SliderTextFontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            TextFontSize = Math.Round(SliderTextFontSize.Value, 0);
            LabelTextFontSize.Text = TextFontSize.ToString();
        }

        private void CloseWindow() {
            if (closeWindowTimer != null) {
                closeWindowTimer.Stop();
            }

            this.Close();
        }

        private void StartCloseWindowTimer() {
            closeWindowTimer = new System.Windows.Threading.DispatcherTimer();
            closeWindowTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            closeWindowTimer.Interval = new TimeSpan(0, 0, 10);
            closeWindowTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e) {
            CloseWindow();
        }

        private void Reset_Click(object sender, RoutedEventArgs e) {
            IsReset = true;
            CloseWindow();
        }

        private void Exit_Click(object sender, RoutedEventArgs e) {
            IsExit = true;
            CloseWindow();
        }

        private void SetAutoStart_Click(object sender, RoutedEventArgs e) {
            IsSetAutoStart = true;
            CloseWindow();
        }

        private void RemoveAutoStart_Click(object sender, RoutedEventArgs e) {
            IsRemoveAutoStart = true;
            CloseWindow();
        }
    }
}
