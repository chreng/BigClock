using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using BigClockGit.Code;

namespace BigClockGit {
    /// <summary>
    /// Interaction logic for TrayWindow.xaml
    /// </summary>
    public partial class TrayWindow : Window {

        public delegate void DoneOrExitHandler(object sender, bool isExit);
        public event DoneOrExitHandler DoneOrExit;
        public delegate void TextResetHandler(object sender, RoutedEventArgs e);
        public event TextResetHandler TextReset;
        public delegate void TextSizeChangedHandler(Object sender, double textSize);
        public event TextSizeChangedHandler TextSizeChanged;
        public delegate void TextColorChangedHandler(Object sender, string colorName);
        public event TextColorChangedHandler TextColorChanged;
        public delegate void TextVisibilityChangedHandler(Object sender, bool isVisible);
        public event TextVisibilityChangedHandler TextVisibilityChanged;
        public delegate void AutoStartChangedHandler(Object sender, bool autoStartActive);
        public event AutoStartChangedHandler AutoStartChanged;
        public delegate void AnchorChangedHandler(Object sender, BigClockAnchorStyles anchorStyle);
        public event AnchorChangedHandler AnchorChanged;
        public delegate void WindowClosingHandler(Object sender);
        public event WindowClosingHandler WindowClosing;

        private bool isSetup { get; set; }

        private DispatcherTimer closeWindowTimer;

        public TrayWindow() {
            InitializeComponent();

            TextColor.SelectionChanged += TextColor_SelectionChanged;
            SliderTextFontSize.ValueChanged += SliderTextFontSize_ValueChanged;
            TextVisibility.Checked += TextVisibility_Checked;
            TextVisibility.Unchecked += TextVisibility_Checked;
            AutoStartActive.Checked += Autostart_Checked;
            AutoStartActive.Unchecked += Autostart_Checked;
            AnchorTopLeft.Checked += Anchors_Checked;
            AnchorTopRight.Checked += Anchors_Checked;
            AnchorBottomLeft.Checked += Anchors_Checked;
            AnchorBottomRight.Checked += Anchors_Checked;
        }

        internal void Setup(bool autoStartActive, double textFontSize, string textColorName, bool textVisible,
            BigClockAnchorStyles anchorStyle) {

            isSetup = true;

            SetupAutostart(autoStartActive);
            SetupColors(textColorName);
            SetupFontSize(textFontSize);
            SetupVisibility(textVisible);
            SetupAnchor(anchorStyle);

            StartCloseWindowTimer();

            isSetup = false;
        }

        private void SetupColors(string textColorName) {

            TextColor.Items.Clear();

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
        }

        private void TextColor_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (isSetup == false) {
                TextColorChanged(this, (e.AddedItems[0] as ComboBoxItem).Content as string);
                StartCloseWindowTimer();
            }
        }

        private void SetupFontSize(double textFontSize) {
            SliderTextFontSize.Value = Math.Round(textFontSize, 0);
            LabelTextFontSize.Text = SliderTextFontSize.Value.ToString();
        }

        private void SliderTextFontSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (isSetup == false) {
                double textFontSize = Math.Round(SliderTextFontSize.Value, 0);
                LabelTextFontSize.Text = textFontSize.ToString();
                TextSizeChanged(this, textFontSize);
                StartCloseWindowTimer();
            }
        }

        private void SetupVisibility(bool textVisible) {
            TextVisibility.IsChecked = textVisible;
        }

       private void TextVisibility_Checked(object sender, RoutedEventArgs e) {
           if (isSetup == false) {
               bool textVisible = TextVisibility.IsChecked == true;
               TextVisibilityChanged(this, textVisible);
               StartCloseWindowTimer();
           }
        }

       private void SetupAutostart(bool autostartActive) {
           AutoStartActive.IsChecked = autostartActive;
       }

       private void Autostart_Checked(object sender, RoutedEventArgs e) {
           if (isSetup == false) {
               bool autostartActive = TextVisibility.IsChecked == true;
               AutoStartChanged(this, autostartActive);
               StartCloseWindowTimer();
           }
       }
        private void SetupAnchor(BigClockAnchorStyles anchorStyle) {
            switch (anchorStyle) {
                case BigClockAnchorStyles.TopLeft:
                    AnchorTopLeft.IsChecked = true;
                    break;
                case BigClockAnchorStyles.TopRight:
                    AnchorTopRight.IsChecked = true;
                    break;
                case BigClockAnchorStyles.BottomLeft:
                    AnchorBottomLeft.IsChecked = true;
                    break;
                case BigClockAnchorStyles.BottomRight:
                    AnchorBottomRight.IsChecked = true;
                    break;
            }
        }

        private void Anchors_Checked(object sender, RoutedEventArgs e) {
            if (isSetup == false) {
                BigClockAnchorStyles anchorStyle = BigClockAnchorStyles.TopLeft;
                if (AnchorTopLeft.IsChecked == true) {
                    anchorStyle = BigClockAnchorStyles.TopLeft;
                } else if (AnchorTopRight.IsChecked == true) {
                    anchorStyle = BigClockAnchorStyles.TopRight;
                } else if (AnchorBottomLeft.IsChecked == true) {
                    anchorStyle = BigClockAnchorStyles.BottomLeft;
                } else if (AnchorBottomRight.IsChecked == true) {
                    anchorStyle = BigClockAnchorStyles.BottomRight;
                }
                AnchorChanged(this, anchorStyle);
                StartCloseWindowTimer();
            }
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
            Close();
        }

        private void Reset_Click(object sender, RoutedEventArgs e) {
            TextReset(sender, e);
        }

        private void Exit_Click(object sender, RoutedEventArgs e) {
            DoneOrExit(sender, true);
        }

        private void Done_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
