using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Globalization;

namespace BigClockGit {

    public partial class MainWindow : Window {

        private DispatcherTimer updateTimer;
        private Screen currentScreen;
        private int currentNumScreens;

        public MainWindow() {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
            this.LocationChanged += MainWindow_LocationChanged;

        }

        private void MainWindow_Loaded(object sender, System.EventArgs e) {
            InitWindowGeometry();

            ShowTime();
            UpdateTimerStart();
        }

        private void MainWindow_Closed(object sender, System.EventArgs e) {
            SaveWindowGeometry();
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e) {
            // Location changes when number of screens changes, only save geometry when user moved the window
            if (currentNumScreens == Screen.AllScreens.Length) {
                SaveWindowGeometry();
            }
        }

        private void InitWindowGeometry() {

            currentNumScreens = Screen.AllScreens.Length;

            string savedGeometry = null;
            try {
                savedGeometry = (string)Properties.Settings.Default["ScreenGeometry" + currentNumScreens.ToString()];
            } catch (Exception) {
            }

            if (string.IsNullOrEmpty(savedGeometry) == false) {
                string[] leftTopArray = savedGeometry.Split(';');
                if (leftTopArray.Length == 2) {
                    int left;
                    int top;

                    if (int.TryParse(leftTopArray[0], out left) &&
                        int.TryParse(leftTopArray[1], out top)) {
                        this.Left = left;
                        this.Top = top;

                        return;
                    }
                }
            }

            // Default strategy: Bottom right if only one screen, bottom left if several screens
            if (currentNumScreens > 1) {
                currentScreen = Screen.AllScreens[currentNumScreens - 1];
            } else {
                currentScreen = Screen.PrimaryScreen;
            }

            this.Left = currentScreen.WorkingArea.Right - this.Width;
            this.Top = currentScreen.WorkingArea.Bottom - this.Height;
        }

        private void SaveWindowGeometry() {

            int numScreens = Screen.AllScreens.Length;

            for (int i = 0; i < numScreens; i++) {
                Screen screen = Screen.AllScreens[i];
                if (this.Left >= screen.WorkingArea.Left &&
                    this.Left <= screen.WorkingArea.Right &&
                    this.Top >= screen.WorkingArea.Top &&
                    this.Top <= screen.WorkingArea.Bottom) {

                    string savedGeometry = this.Left.ToString() + ";" + this.Top.ToString();

                    Properties.Settings.Default["ScreenGeometry" + numScreens.ToString()] = savedGeometry;
                    Properties.Settings.Default.Save();

                    return;
                }
            }

            // Outside of any screen, remove the entry for current number of screen. Might be
            // annoying if the user really wanted to hide the clock...

            Properties.Settings.Default["ScreenGeometry" + numScreens.ToString()] = "";
            Properties.Settings.Default.Save();
        }

        private void UpdateTimerStart() {
            if (updateTimer == null) {
                updateTimer = new DispatcherTimer();
                updateTimer.Interval = TimeSpan.FromSeconds(3);
                updateTimer.Tick += UpdateTimerTick;
                updateTimer.Start();
            }
        }

        private void UpdateTimerStop() {
            if (updateTimer != null) {
                updateTimer.Stop();
                updateTimer.Tick -= UpdateTimerTick;
                updateTimer = null;
            }
        }

        private void UpdateTimerTick(object sender, EventArgs e) {

            if (Screen.AllScreens.Length != currentNumScreens) {
                UpdateTimerStop();
                InitWindowGeometry();
                UpdateTimerStart();
            }

            ShowTime();
        }

        private void ShowTime() {
            DateTime now = DateTime.Now;
            string dayname = now.ToString("dddd");
            string dateTimeString = now.ToShortDateString() + " " + now.ToShortTimeString();
            dateTimeString = dateTimeString.Replace(dayname, "");

            if (string.Compare(CurrentTime.Content as string, dateTimeString) != 0) {
                CurrentTime.Content = dateTimeString;
            }
        }

        public void DragWindow(object sender, MouseButtonEventArgs args) {
            DragMove();
        }
    }
}
