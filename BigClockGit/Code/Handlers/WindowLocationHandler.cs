using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BigClockGit.Code.Handlers {

    // RECT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int left, int top, int right, int bottom) {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }
    }

    // POINT structure required by WINDOWPLACEMENT structure
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT {
        public int X;
        public int Y;

        public POINT(int x, int y) {
            this.X = x;
            this.Y = y;
        }
    }

    // WINDOWPLACEMENT stores the position, size, and state of a window
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPLACEMENT {
        public int length;
        public int flags;
        public int showCmd;
        public POINT minPosition;
        public POINT maxPosition;
        public RECT normalPosition;
    }

    /// <summary>
    /// Handles window locations
    /// </summary>
    public static class WindowLocationHandler {

        #region Win32 API declarations to set and get window placement
        [DllImport("user32.dll")]
        static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        const int SW_SHOWNORMAL = 1;
        const int SW_SHOWMINIMIZED = 2;
        #endregion

        private static string WindowPlacement_Prefix = "Wp:";

        /// <summary>
        /// Gets the window geometry as a string
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns></returns>
        public static string GetWindowGeometry(Window window) {
            return GetWin32WindowGeometry(window);
        }

        /// <summary>
        /// Gets the window geometry using win32 windowplacement as a string
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns></returns>
        private static string GetWin32WindowGeometry(Window window) {
            WINDOWPLACEMENT windowPlacement = new WINDOWPLACEMENT();
            IntPtr hwnd = new WindowInteropHelper(window).Handle;

            GetWindowPlacement(hwnd, out windowPlacement);
            MemoryStream memoryStream = new MemoryStream();
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, windowPlacement);
            string serializedWindowPlacement = System.Convert.ToBase64String(memoryStream.ToArray());

            // Format: prefix:length of serialized data;base64 encoded
            return WindowPlacement_Prefix + memoryStream.Length.ToString() + ";" + serializedWindowPlacement;
        }

        /// <summary>
        /// Sets the window geometry given a string
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="windowGeometry">The window geometry.</param>
        private static void SetWin32WindowGeometry(Window window, string windowGeometry) {
            // Place using win32 windowplacement
            // Note - if window was closed on a monitor that is now disconnected from the computer,
            //        SetWindowPlacement will place the window onto a visible monitor.
            string rawGeometryString = windowGeometry.Remove(0, WindowPlacement_Prefix.Length);
            int firstSemicolonIndex = rawGeometryString.IndexOf(';');
            if (firstSemicolonIndex < 1) {
                return;
            }

            int base64Length = 0;
            string lengthString = rawGeometryString.Substring(0, firstSemicolonIndex);
            if (int.TryParse(lengthString, out base64Length) == false) {
                return;
            }

            string base64String = rawGeometryString.Substring(firstSemicolonIndex + 1);
            byte[] memorydata = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(memorydata, 0, base64Length);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            WINDOWPLACEMENT windowPlacement = (WINDOWPLACEMENT)binaryFormatter.Deserialize(memoryStream);
            windowPlacement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
            windowPlacement.flags = 0;
            windowPlacement.showCmd = SW_SHOWNORMAL;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            SetWindowPlacement(hwnd, ref windowPlacement);
        }

        /// <summary>
        /// Sets the window geometry given a string
        /// </summary>
        /// <param name="window">The window.</param>
        /// <param name="windowGeometry">The window geometry.</param>
        public static void SetWindowGeometry(Window window, string windowGeometry) {

            if (windowGeometry.StartsWith(WindowPlacement_Prefix) == true) {
                SetWin32WindowGeometry(window, windowGeometry);
                return;
            }

            // Get the remembered window size and position. Use it if it exists and is visible inside primary screen
            string mainWindowSize = windowGeometry;
            if (string.IsNullOrEmpty(mainWindowSize) == false) {
                bool validArray = true;

                string[] geoArray = mainWindowSize.Split(';');

                if (geoArray.Count() != 4) {
                    validArray = false;
                }

                int left = 0;
                int width = 0;
                int top = 0;
                int height = 0;
                if (validArray == true) {
                    if (int.TryParse(geoArray[0], out left) == false) {
                        validArray = false;
                    }
                    if (int.TryParse(geoArray[1], out width) == false) {
                        validArray = false;
                    }
                    if (int.TryParse(geoArray[2], out top) == false) {
                        validArray = false;
                    }
                    if (int.TryParse(geoArray[3], out height) == false) {
                        validArray = false;
                    }

                    if ((left + width) < 0
                        || (left > (System.Windows.SystemParameters.PrimaryScreenWidth - 100))
                        || (top + height < 0)
                        || (top > (System.Windows.SystemParameters.PrimaryScreenHeight - 100))
                        ) {
                        validArray = false;
                    }

                    if (validArray) {
                        window.Left = left;
                        window.Width = width;
                        window.Top = top;
                        window.Height = height;
                    }
                }
            }
        }
    }
}