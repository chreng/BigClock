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

        public delegate void TextSizeChangedHandler(Object sender, double textSize);
        public event TextSizeChangedHandler TextSizeChanged;
        public delegate void TextColorChangedHandler(Object sender, string colorName);
        public event TextColorChangedHandler TextColorChanged;
        public delegate void TextVisibilityChangedHandler(Object sender, bool isVisible);
        public event TextVisibilityChangedHandler TextVisibilityChanged;
        public delegate void AutoStartChangedHandler(Object sender, bool autoStartActive);
        public event AutoStartChangedHandler AutoStartChanged;

        public bool IsExit { get; set;  }
        public bool IsReset { get; set; }

        private DispatcherTimer closeWindowTimer;

        public TrayWindow(bool autoStartActive, double textFontSize, string textColorName, bool textVisible) {
            InitializeComponent();

            IsExit = false;
            IsReset = false;

            SetupAutostart(autoStartActive);
            SetupColors(textColorName);
            SetupFontSize(textFontSize);
            SetupVisibility(textVisible);

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
            TextColorChanged(this, (e.AddedItems[0] as ComboBoxItem).Content as string);
            StartCloseWindowTimer();
        }

        private void SetupFontSize(double textFontSize) {
            SliderTextFontSize.Value = Math.Round(textFontSize, 0);
            LabelTextFontSize.Text = SliderTextFontSize.Value.ToString();
            SliderTextFontSize.ValueChanged += SliderTextFontSize_ValueChanged;
        }

        private void SliderTextFontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            double textFontSize = Math.Round(SliderTextFontSize.Value, 0);
            LabelTextFontSize.Text = textFontSize.ToString();
            TextSizeChanged(this, textFontSize);
            StartCloseWindowTimer();
        }

        private void SetupVisibility(bool textVisible) {
            TextVisibility.IsChecked = textVisible;
            TextVisibility.Checked += TextVisibility_Checked;
            TextVisibility.Unchecked += TextVisibility_Checked;
        }

       private void TextVisibility_Checked(object sender, RoutedEventArgs e) {
            bool textVisible = TextVisibility.IsChecked == true;
            TextVisibilityChanged(this, textVisible);
            StartCloseWindowTimer();
        }

       private void SetupAutostart(bool autostartActive) {
           AutoStartActive.IsChecked = autostartActive;
           AutoStartActive.Checked += Autostart_Checked;
           AutoStartActive.Unchecked += Autostart_Checked;
       }

       private void Autostart_Checked(object sender, RoutedEventArgs e) {
           bool autostartActive = TextVisibility.IsChecked == true;
           AutoStartChanged(this, autostartActive);
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

        private void Done_Click(object sender, RoutedEventArgs e) {
            CloseWindow();
        }
    }
}
