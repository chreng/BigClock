using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Forms;
using System.IO;
using System.Windows.Media;
using IWshRuntimeLibrary;

namespace BigClockGit {

    public partial class MainWindow : Window {

        private DispatcherTimer updateTimer;
        private Screen currentScreen;
        private int currentNumScreens;
        private string textFontColor;
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

            ShowInTray();

            ShowTime();
            UpdateTimerStart();
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e) {
            // Location changes when number of screens changes, only save geometry when user moved the window
            if (currentNumScreens == Screen.AllScreens.Length) {
                SaveWindowGeometry();
            }
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

        private void ShowTrayMenu(object sender, EventArgs args) {
            if (trayWindow == null) {
                bool shortCutExists = System.IO.File.Exists(GetShortcutPath());
                trayWindow = new TrayWindow(shortCutExists, CurrentTime.FontSize, textFontColor);
                trayWindow.Top = Screen.PrimaryScreen.WorkingArea.Height - trayWindow.Height - 300;
                trayWindow.Left = Screen.PrimaryScreen.WorkingArea.Width - trayWindow.Width - 250;
                trayWindow.ShowDialog();

                if (trayWindow.IsExit) {
                    if (notifyIcon != null) {
                        notifyIcon.Visible = false;
                        notifyIcon.MouseClick -= ShowTrayMenu;
                    }
                    System.Windows.Application.Current.Shutdown();
                } else if (trayWindow.IsReset) {
                    ResetWindowGeometry();
                } else if (trayWindow.IsSetAutoStart) {
                    AddShortcutToStartupGroup();
                } else if (trayWindow.IsRemoveAutoStart) {
                    RemoveShortcutFromStartupGroup();
                }

                CurrentTime.FontSize = trayWindow.TextFontSize;

                try {
                    CurrentTime.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(trayWindow.TextFontColor));
                    textFontColor = trayWindow.TextFontColor;
                } catch {
                }

                trayWindow = null;

                SaveWindowGeometry();
            }
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

            this.Left = currentScreen.WorkingArea.Width - this.Width - 150;
            this.Top = currentScreen.WorkingArea.Height - this.Height - 100;
        }

        private void SaveWindowGeometry() {

            int numScreens = Screen.AllScreens.Length;

            for (int i = 0; i < numScreens; i++) {
                Screen screen = Screen.AllScreens[i];
                if (this.Left >= screen.WorkingArea.Left &&
                    this.Left <= screen.WorkingArea.Right &&
                    this.Top >= screen.WorkingArea.Top &&
                    this.Top <= screen.WorkingArea.Bottom) {

                    string savedGeometry = this.Left.ToString() + ";" + this.Top.ToString() + ";" + i.ToString() +
                        ";" + CurrentTime.FontSize.ToString() + ";" + textFontColor;

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
