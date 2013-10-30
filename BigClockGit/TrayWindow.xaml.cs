using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        public string TextFontColor { set; get; }

        private DispatcherTimer closeWindowTimer;

        public TrayWindow(bool autoStartActive, double textFontSize, string textColorName) {
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

            SetupColors(textColorName);
            SetupFontSize(textFontSize);

            StartCloseWindowTimer();
        }

        private void SetupColors(string textColorName) {

            foreach (PropertyInfo p in typeof(Colors).GetProperties()) {
                if (p.PropertyType == typeof(Color)) {
                    ComboBoxItem colorItem = new ComboBoxItem();
                    colorItem.Content = p.Name;
                    if (string.Compare(textColorName, p.Name, true) == 0) {
                        colorItem.IsSelected = true;
                    }
                    TextColor.Items.Add(colorItem);
                }
            }

            TextColor.SelectionChanged += TextColor_SelectionChanged;
        }

        private void TextColor_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            TextFontColor = (e.AddedItems[0] as ComboBoxItem).Content as string;
            StartCloseWindowTimer();
        }

        private void SetupFontSize(double textFontSize) {
            this.TextFontSize = Math.Round(textFontSize, 0);
            SliderTextFontSize.Value = this.TextFontSize;
            LabelTextFontSize.Text = this.TextFontSize.ToString();
            SliderTextFontSize.ValueChanged += SliderTextFontSize_ValueChanged;
        }

        private void SliderTextFontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            TextFontSize = Math.Round(SliderTextFontSize.Value, 0);
            LabelTextFontSize.Text = TextFontSize.ToString();
            StartCloseWindowTimer();
        }

        private void CloseWindow() {
            StopCloseWindowTimer();
            this.Close();
        }

        private void StartCloseWindowTimer() {
            StopCloseWindowTimer();
            closeWindowTimer = new System.Windows.Threading.DispatcherTimer();
            closeWindowTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            closeWindowTimer.Interval = new TimeSpan(0, 0, 30);
            closeWindowTimer.Start();
        }

        private void StopCloseWindowTimer() {
            if (closeWindowTimer != null) {
                closeWindowTimer.Stop();
                closeWindowTimer = null;
            }
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

        private void Done_Click(object sender, RoutedEventArgs e) {
            CloseWindow();
        }
    }
}
