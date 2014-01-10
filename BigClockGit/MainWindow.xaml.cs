using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Forms;
using System.IO;
using System.Windows.Media;
using IWshRuntimeLibrary;
using Microsoft.Win32;

namespace BigClockGit {

    public partial class MainWindow : Window, IDisposable {

        private DispatcherTimer updateTimer;
        private Screen currentScreen;
        private int currentNumScreens;
        private string textFontColor;
        private bool textVisible;
        private NotifyIcon notifyIcon;
        private TrayWindow trayWindow;
        private WshShellClass wshShell;

        public MainWindow() {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, System.EventArgs e) {

            SetupDefaultIfFirstLaunch();

            InitWindowGeometry();

            // Note: Start tracking location after initializing geometry
            this.LocationChanged += MainWindow_LocationChanged;

            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;

            ShowInTray();

            ShowTime();
            UpdateTimerStart();
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool cleanManagedAndNative) {
            if (notifyIcon != null) {
                notifyIcon.MouseClick -= ShowTrayMenu;
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                notifyIcon = null;
            }
        }

        private void RemoveNotifyIcon() {
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e) {
            // Location changes when number of screens changes, only save geometry when user moved the window
            if (currentNumScreens == Screen.AllScreens.Length) {
                SaveWindowGeometry();
            }
        }

        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e) {
            InitWindowGeometry();
        }

        private void SetupDefaultIfFirstLaunch() {
            string firstLaunch = null;
            try {
                firstLaunch = (string)Properties.Settings.Default["FirstLaunchDone"];
            } catch (Exception) {
            }

            if (string.IsNullOrEmpty(firstLaunch) == true) {
                AddShortcutToStartupGroup();
                Properties.Settings.Default["FirstLaunchDone"] = true.ToString();
                Properties.Settings.Default.Save();
            }
        }

        private void ShowInTray() {
            notifyIcon = new NotifyIcon();
            string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            notifyIcon.Icon = new System.Drawing.Icon(Path.Combine(exeDir, "Assets", "favicon.ico"));
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += ShowTrayMenu;
        }

        private void trayWindow_Closed(object sender, EventArgs e) {
            trayWindow = null;
        }

        private void trayWindow_DoneOrExit(object sender, bool isExit) {
            if (isExit) {
                notifyIcon.MouseClick -= ShowTrayMenu;
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                notifyIcon = null;
                System.Windows.Application.Current.Shutdown();
            } else if (trayWindow != null) {
                trayWindow.Hide();
            }
        }

        private void ShowTrayMenu(object sender, EventArgs args) {

            if (trayWindow == null) {
                trayWindow = new TrayWindow();

                trayWindow.DoneOrExit += trayWindow_DoneOrExit;
                trayWindow.Closed += trayWindow_Closed;
                trayWindow.TextReset += trayWindow_TextReset;
                trayWindow.TextSizeChanged += trayWindow_TextSizeChanged;
                trayWindow.TextColorChanged += trayWindow_TextColorChanged;
                trayWindow.TextVisibilityChanged += trayWindow_TextVisibilityChanged;
                trayWindow.AutoStartChanged += trayWindow_AutoStartChanged;
            }

            this.Activate();
            trayWindow.Show();
            SetupTrayMenu();
        }

        private void SetupTrayMenu() {
            bool autoStartActive = System.IO.File.Exists(GetShortcutPath());
            bool textVisible = CurrentTime.Visibility == System.Windows.Visibility.Visible;
            trayWindow.Setup(autoStartActive, CurrentTime.FontSize, textFontColor, textVisible);
        }
        

        private void trayWindow_TextReset(object sender, RoutedEventArgs e) {
            ResetWindowGeometry();
            SetupTrayMenu();
        }

        private void trayWindow_AutoStartChanged(object sender, bool autoStartActive) {
            if (autoStartActive) {
                AddShortcutToStartupGroup();
            } else {
                RemoveShortcutFromStartupGroup();
            }
        }

        private void trayWindow_TextSizeChanged(object sender, double textSize) {
            CurrentTime.FontSize = textSize;
            SaveWindowGeometry();
        }

        private void trayWindow_TextColorChanged(object sender, string colorName) {
            try {
                CurrentTime.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorName));
                textFontColor = colorName;
                SaveWindowGeometry();
            } catch {
            }
        }

        private void trayWindow_TextVisibilityChanged(object sender, bool isVisible) {
            if (isVisible) {
                CurrentTime.Visibility = System.Windows.Visibility.Visible;
            } else {
                CurrentTime.Visibility = System.Windows.Visibility.Hidden;
            }
            SaveWindowGeometry();
        }

        private void AddShortcutToStartupGroup() {
            wshShell = new WshShellClass();
            IWshRuntimeLibrary.IWshShortcut MyShortcut;
            string tempLinkFile = Path.GetTempFileName() + ".lnk";
            MyShortcut = (IWshRuntimeLibrary.IWshShortcut)wshShell.CreateShortcut(tempLinkFile);
            MyShortcut.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            MyShortcut.Description = "Launches BigClockGit";
            string exeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            MyShortcut.IconLocation = Path.Combine(exeDir, "Assets", "favicon.ico");
            MyShortcut.Save();

            System.IO.File.Copy(tempLinkFile, GetShortcutPath(), true);
            System.IO.File.Delete(tempLinkFile);
        }

        private void RemoveShortcutFromStartupGroup() {

            if (System.IO.File.Exists(GetShortcutPath())) {
                System.IO.File.Delete(GetShortcutPath());
            }
        }

        private string GetShortcutPath() {
            string targetLinkPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                "BigClockGit.lnk");

            return targetLinkPath;
        }

        private void ResetWindowGeometry() {
            this.LocationChanged -= MainWindow_LocationChanged;

            currentNumScreens = Screen.AllScreens.Length;
            Properties.Settings.Default["ScreenGeometry" + currentNumScreens.ToString()] = "";
            Properties.Settings.Default.Save();

            InitWindowGeometry();

            this.LocationChanged += MainWindow_LocationChanged;
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
                if (leftTopArray.Length >= 2) {
                    double left;
                    double top;
                    int screen = currentNumScreens - 1;
                    if (leftTopArray.Length > 2) {
                        int.TryParse(leftTopArray[2], out screen);
                    }

                    double textFontSize = CurrentTime.FontSize;
                    if (leftTopArray.Length > 3) {
                        double.TryParse(leftTopArray[3], out textFontSize);
                    }

                    textFontColor = "Black";
                    if (leftTopArray.Length > 4) {
                        textFontColor = leftTopArray[4];
                    }

                    textVisible = true;
                    if (leftTopArray.Length > 5) {
                        bool.TryParse(leftTopArray[5], out textVisible);
                    }

                    if (double.TryParse(leftTopArray[0], out left) &&
                        double.TryParse(leftTopArray[1], out top)) {

                        currentScreen = Screen.AllScreens[screen];
                        this.Left = left;
                        this.Top = top;

                        if (textFontSize >= 10 &&
                            textFontSize <= 400) {
                            CurrentTime.FontSize = textFontSize;
                        }

                        try {
                            CurrentTime.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(textFontColor));
                        } catch {
                        }

                        if (textVisible) {
                            CurrentTime.Visibility = System.Windows.Visibility.Visible;
                        } else {
                            CurrentTime.Visibility = System.Windows.Visibility.Hidden;
                        }

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

            this.Left = currentScreen.WorkingArea.Left + 10;
            this.Top = currentScreen.WorkingArea.Bottom - this.Height - 250;
            CurrentTime.FontSize = 25;
            CurrentTime.Foreground = new SolidColorBrush(Colors.Black);
            textFontColor = "Black";
            CurrentTime.Visibility = System.Windows.Visibility.Visible;
        }

        private void SaveWindowGeometry() {

            int numScreens = Screen.AllScreens.Length;

            for (int i = 0; i < numScreens; i++) {
                Screen screen = Screen.AllScreens[i];
                if (this.Left >= screen.WorkingArea.Left &&
                    this.Left <= screen.WorkingArea.Right &&
                    this.Top >= screen.WorkingArea.Top &&
                    this.Top <= screen.WorkingArea.Bottom) {

                    bool textVisible = CurrentTime.Visibility == System.Windows.Visibility.Visible ? true : false;
                    string savedGeometry = this.Left.ToString() + ";" + this.Top.ToString() + ";" + i.ToString() +
                        ";" + CurrentTime.FontSize.ToString() + ";" + textFontColor +
                        ";" + textVisible.ToString();

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
