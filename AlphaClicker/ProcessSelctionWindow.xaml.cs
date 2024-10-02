using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text;

namespace AlphaClicker
{
    public partial class ProcessSelectionWindow : Window
    {
        public string SelectedProcessName { get; private set; }

        public ProcessSelectionWindow()
        {
            InitializeComponent();
            PopulateProcessList();
        }


        public class ProcessWindowInfo
        {
            public IntPtr WindowHandle { get; set; }
            public string WindowTitle { get; set; }
            public string ProcessName { get; set; }

            public override string ToString()
            {
                return $"Process: {ProcessName}, Title: {WindowTitle}, HWND: {WindowHandle.ToInt32()}";
            }
        }


        public class WindowHelper
        {
            // Import user32.dll
            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern bool IsWindowVisible(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            // Delegate for callback
            private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

            public static void GetOpenWindows(Action<ProcessWindowInfo> addWindowInfo)
            {
                EnumWindows((hWnd, lParam) =>
                {
                    if (IsWindowVisible(hWnd))
                    {
                        StringBuilder sb = new StringBuilder(256);
                        GetWindowText(hWnd, sb, sb.Capacity);
                        string windowTitle = sb.ToString();

                        if (!string.IsNullOrEmpty(windowTitle))
                        {
                            uint processId;
                            GetWindowThreadProcessId(hWnd, out processId);

                            Process process = null;
                            try
                            {
                                process = Process.GetProcessById((int)processId);
                                string processName = process.ProcessName;

                                var processWindowInfo = new ProcessWindowInfo
                                {
                                    WindowHandle = hWnd,
                                    WindowTitle = windowTitle,
                                    ProcessName = processName
                                };

                                addWindowInfo(processWindowInfo);
                            }
                            catch (Exception)
                            {
                                // Handle exceptions (e.g., process might have exited)
                            }
                            finally
                            {
                                process?.Dispose();
                            }
                        }
                    }
                    return true;
                }, IntPtr.Zero);
            }
        }


        private void PopulateProcessList()
        {
            ProcessListBox.Items.Clear();
            WindowHelper.GetOpenWindows(processWindowInfo =>
            {
                Dispatcher.Invoke(() => ProcessListBox.Items.Add(processWindowInfo));
            });
        }



        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessListBox.SelectedItem is ProcessWindowInfo selectedInfo)
            {

                int selectedProcessHandle = selectedInfo.WindowHandle.ToInt32();

                string SelectedProcessName = selectedInfo.ProcessName;

                // Save the process ID and Name to settings
                Properties.Settings.Default.ProcessName = SelectedProcessName;
                Properties.Settings.Default.ProcessHWND = selectedProcessHandle;
                Properties.Settings.Default.Save();

                DialogResult = true;
                Close();
            }
        }




        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessListBox.Items.Clear();
            await Task.Delay(50);
            PopulateProcessList();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void closeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void ProcessSelection_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
