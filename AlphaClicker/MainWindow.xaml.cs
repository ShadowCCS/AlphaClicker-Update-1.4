using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.IO;
using System.IO.Ports;
using AlphaClicker.Properties;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Policy;

namespace AlphaClicker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Properties.Settings.Default.TogglePlay = "F4";
            //Properties.Settings.Default.ProcessHWND = 0;
            //Properties.Settings.Default.ProcessName = null;
        }

        string holdMouseBtn;

        public void LoadKeybind()
        {
            string keyBinding = Properties.Settings.Default.TogglePlay;
            startBtn.Content = $"Start ({keyBinding})";
            //stopBtn.Content = $"Stop ({keyBinding})"; //Not Running text instead
        }

        private void Cerror(string errormessage)
        {
            ToggleClick();
            MessageBox.Show(errormessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private int ToInt(string number)
        {
            return Int32.Parse((number == "") ? "0" : number);
        }

        private void UpdateEllipsColor(bool isRunning)
        {
            // Define color based on the running state
            Brush markerColor = isRunning ? Brushes.Green : (Brush)new BrushConverter().ConvertFromString("#ff605c");

            // Update UI elements
            IsRunningMarker.Fill = markerColor;
            IsRunningText.Content = isRunning ? "Running" : "Stopped";

            // Get key binding from settings
            string keyBinding = Properties.Settings.Default.TogglePlay;

            // Update button contents
            stopBtn.Content = isRunning ? "Stop (" + keyBinding + ")" : "Not Running";
            startBtn.Content = isRunning ? "Running" : "Start (" + keyBinding + ")";
        }

        private Color AdjustColorBrightness(Color color, double factor)
        {
            // Clamp factor to a range of -1 to 1
            factor = Math.Max(-1, Math.Min(1, factor));

            // Calculate the new color values
            int r = (int)(color.R * (1 + factor));
            int g = (int)(color.G * (1 + factor));
            int b = (int)(color.B * (1 + factor));

            // Ensure values stay within the valid range [0, 255]
            r = Math.Min(255, Math.Max(0, r));
            g = Math.Min(255, Math.Max(0, g));
            b = Math.Min(255, Math.Max(0, b));

            return Color.FromRgb((byte)r, (byte)g, (byte)b);
        }


        private void ToggleClick()
        {
            SolidColorBrush startButtonBrush = (SolidColorBrush)FindResource("StartButton");
            SolidColorBrush stopButtonBrush = (SolidColorBrush)FindResource("StopButton");

            if (startBtn.IsEnabled)
            {
                // Disable start button, enable stop button
                startBtn.IsEnabled = false;
                stopBtn.IsEnabled = true;

                Color lighterStartColor = AdjustColorBrightness(startButtonBrush.Color, -0.4); // Darker
                Color darkerStopColor = AdjustColorBrightness(stopButtonBrush.Color, 0.4); // Lighter

                startBtn.Background = new SolidColorBrush(lighterStartColor);
                stopBtn.Background = new SolidColorBrush(darkerStopColor);

                UpdateEllipsColor(true);

                // Start async task for
                Task.Run(() => ClickHandlerAsync());
            }
            else
            {
                // Enable start button, disable stop button
                startBtn.IsEnabled = true;
                stopBtn.IsEnabled = false;

                if (!String.IsNullOrEmpty(holdMouseBtn))
                {
                    WinApi.DoMouseUp(holdMouseBtn);
                }

                UpdateEllipsColor(false);

                startBtn.Background = startButtonBrush;
                stopBtn.Background = stopButtonBrush;
            }
        }





        public bool keyEnabled = true;

        void KeyHandler()
        {
            while (true)
            {
                if (keyEnabled)
                {
                    //Mouse inputs are not considered keys and therefor it causes problems in the parsing
                    string keyBinding = Properties.Settings.Default.TogglePlay;
                    int keyCode;

                    // Check if the binding is a special mouse button
                    if (keyBinding == "MMB")
                    {
                        keyCode = (int)VK.VK_MBUTTON; // Middle Mouse Button
                    }
                    else if (keyBinding == "X1")
                    {
                        keyCode = (int)VK.VK_XBUTTON1; // Extra Mouse Button 1
                    }
                    else if (keyBinding == "X2")
                    {
                        keyCode = (int)VK.VK_XBUTTON2; // Extra Mouse Button 2
                    }
                    else
                    {
                        try
                        {
                            // Attempt to parse it as a standard key
                            keyCode = KeyInterop.VirtualKeyFromKey((Key)Enum.Parse(typeof(Key), keyBinding, true));
                        }
                        catch (ArgumentException)
                        {
                            // Handle the case where the key is not found
                            continue; // Skip to the next iteration
                        }
                    }

                    // Check if the key or mouse button is pressed
                    if (WinApi.GetAsyncKeyState(keyCode) > 0)
                    {
                        Dispatcher.Invoke((Action)(() =>
                        {
                            ToggleClick();
                        }));
                    }
                }
                Thread.Sleep(200);
            }
        }

        private int calcSleep()
        {
            if (millisecsBox.Text == "0")
            {
                millisecsBox.Text = "1";
            }

            int sleep = 0;

            sleep = ToInt(millisecsBox.Text) +
                    ToInt(secondsBox.Text) * 1000 +
                    ToInt(minsBox.Text) * 60000 +
                    ToInt(hoursBox.Text) * 3600000;
            sleep = (sleep == 0) ? 1 : sleep;

            return sleep;
        }

        private async Task ClickHandlerAsync()
        {
            int sleep = 0;
            bool useRandomSleep = false;
            int randnum1 = 0;
            int randnum2 = 0;
            string mouseBtn = "";
            string clickType = "";
            bool repeatTimesChecked = false;
            int repeatTimes = 0;
            bool customCoordsChecked = false;
            int customCoordsX = 0, customCoordsY = 0;


            //Jittering
            int jitterFactor = Properties.Settings.Default.JitterFactor; // This will be set based on user input (0-5)


            // Collect settings in the dispatcher to ensure thread safety
            Dispatcher.Invoke(() =>
            {
                try
                {

                    // Collecting time interval settings
                    useRandomSleep = (bool)randomIntervalMode.IsChecked;
                    if (useRandomSleep)
                    {
                        randnum1 = (int)(float.Parse(randomSecs1Box.Text, CultureInfo.InvariantCulture) * 1000);
                        randnum2 = (int)(float.Parse(randomSecs2Box.Text, CultureInfo.InvariantCulture) * 1000);
                        randnum1 = Math.Max(1, randnum1);
                        randnum2 = Math.Max(1, randnum2);
                    }
                    else
                    {
                        sleep = calcSleep();
                    }

                    // Collecting mouse button and click type
                    mouseBtn = mouseBtnCBOX.Text;
                    clickType = clickTypeCBOX.Text;

                    // Collecting repeat settings
                    repeatTimesChecked = (bool)repeatTimesRBtn.IsChecked;
                    if (repeatTimesChecked)
                    {
                        if (!Int32.TryParse(repeatTimesBox.Text, out repeatTimes))
                        {
                            Cerror("Invalid Repeat Times Number");
                            return;
                        }
                    }

                    // Collecting custom coordinates
                    customCoordsChecked = (bool)coordsCBtn.IsChecked;
                    if (customCoordsChecked)
                    {
                        if (!Int32.TryParse(xBox.Text, out customCoordsX) || !Int32.TryParse(yBox.Text, out customCoordsY))
                        {
                            Cerror("Invalid Coordinates");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Cerror("An error occurred while collecting settings: " + ex.Message);
                    return;
                }
            });

            int repeatCount = 0;
            Random rnd = new Random();

            while (true)
            {
                bool doClick = Dispatcher.Invoke(() => stopBtn.IsEnabled);

                if (doClick)
                {
                    if (repeatTimesChecked && repeatCount >= repeatTimes)
                    {
                        Dispatcher.Invoke(() => ToggleClick());
                        break;
                    }
                    repeatCount++;

                    // Calculate jitter
                    int jitterDistance = jitterFactor * 2; // Define jitter distance based on factor
                    int jitterTime = jitterFactor * 10; // Define jitter time based on factor

                    // Initialize effective coordinates without jitter adjustments
                    int effectiveCoordsX = customCoordsChecked ? customCoordsX : 0;
                    int effectiveCoordsY = customCoordsChecked ? customCoordsY : 0;

                    // Apply jitter only if both conditions are met and not a "Hold" click type
                    if (jitterFactor > 0 && customCoordsChecked && clickType != "Hold")
                    {
                        effectiveCoordsX += rnd.Next(-jitterDistance, jitterDistance + 1);
                        effectiveCoordsY += rnd.Next(-jitterDistance, jitterDistance + 1);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        TestLabel.Content = "X: " + effectiveCoordsX + " Y: " + effectiveCoordsY;
                    });

                    // Get the current mouse position
                    Point currentMousePos = WinApi.GetCursorPosition();

                    // Interpolate mouse movement to smooth move mouse button
                    if (jitterFactor > 0 && customCoordsChecked && clickType != "Hold")
                    {
                        await InterpolateMouseMovement((int)currentMousePos.X, (int)currentMousePos.Y, effectiveCoordsX, effectiveCoordsY, jitterTime);
                    }

                    // Performing the click or hold
                    if (clickType == "Single")
                    {
                        WinApi.DoClick(mouseBtn, customCoordsChecked, effectiveCoordsX, effectiveCoordsY);
                    }
                    else if (clickType == "Double")
                    {
                        WinApi.DoClick(mouseBtn, customCoordsChecked, effectiveCoordsX, effectiveCoordsY);
                        await Task.Delay(300);
                        WinApi.DoClick(mouseBtn, customCoordsChecked, effectiveCoordsX, effectiveCoordsY);
                    }
                    else if (clickType == "Hold")
                    {
                        WinApi.DoMouseDown(mouseBtn, customCoordsChecked, effectiveCoordsX, effectiveCoordsY);
                        holdMouseBtn = mouseBtn;
                    }

                    // Handle sleep time
                    int actualSleep = useRandomSleep ? rnd.Next(Math.Min(randnum1, randnum2), Math.Max(randnum1, randnum2)) : sleep;

                    // Add jitter to sleep if the jitter factor is greater than 0
                    if (jitterFactor > 0)
                    {
                        actualSleep += rnd.Next(0, jitterTime + 1); // Add jitter time
                    }
                    await Task.Delay(actualSleep);
                }
                else
                {
                    break;
                }
            }
        }

        private async Task InterpolateMouseMovement(int startX, int startY, int endX, int endY, int jitterTime)
        {
            int steps = jitterTime / 10; // Adjust this value for smoother or faster movement
            float stepX = (endX - startX) / (float)steps;
            float stepY = (endY - startY) / (float)steps;

            for (int i = 0; i < steps; i++)
            {
                startX += (int)stepX;
                startY += (int)stepY;

                WinApi.SetCursorPos(startX, startY); // Set the mouse position to the new interpolated position
                await Task.Delay(10); // Small delay for smooth movement
            }

            // Ensure the final position is set
            WinApi.SetCursorPos(endX, endY);
        }

        //private void UpdateIniSettings()
        //{

        //    string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
        //    string iniFilePath = Path.Combine(projectDirectory, "AHK_Scripts", "config.ini");
        //    MessageBox.Show(iniFilePath);
        //    // Create an instance of IniFile class
        //    IniFile iniFile = new IniFile(iniFilePath);

        //    // Set values for each key in the [Settings] section using values from Properties.Settings.Default
        //    //iniFile.WriteValue("Settings", "ProcessPath", Properties.Settings.Default.ProcessPath);
        //    iniFile.WriteValue("Settings", "RandNum1", Properties.Settings.Default.RandNum1.ToString());
        //    iniFile.WriteValue("Settings", "RandNum2", Properties.Settings.Default.RandNum2.ToString());
        //    iniFile.WriteValue("Settings", "ClickType", Properties.Settings.Default.ClickType);
        //    iniFile.WriteValue("Settings", "RepeatTimesChecked", Properties.Settings.Default.RepeatTimesChecked.ToString());
        //    iniFile.WriteValue("Settings", "RepeatTimes", Properties.Settings.Default.RepeatTimes.ToString());
        //    iniFile.WriteValue("Settings", "CustomCoordsX", Properties.Settings.Default.CustomCoordsX.ToString());
        //    iniFile.WriteValue("Settings", "CustomCoordsY", Properties.Settings.Default.CustomCoordsY.ToString());
        //    iniFile.WriteValue("Settings", "CustomCoordsChecked", Properties.Settings.Default.CustomCoordsChecked.ToString());
        //    iniFile.WriteValue("Settings", "MouseBtn", Properties.Settings.Default.MouseBtn);
        //    iniFile.WriteValue("Settings", "Sleep", Properties.Settings.Default.Sleep.ToString());
        //    iniFile.WriteValue("Settings", "UseRandomSleep", Properties.Settings.Default.UseRandomSleep.ToString());

        //    MessageBox.Show(iniFile.ReadValue("Settings", "ProcessPath"));
        //}



        private void SaveAllSettings()
        {
            if (Properties.Settings.Default.SaveAllSettings)
            {
                Properties.Settings.Default.Sleep = calcSleep();
                Properties.Settings.Default.UseRandomSleep = (bool)randomIntervalMode.IsChecked;

                int randnum1 = (int)(float.Parse(randomSecs1Box.Text, CultureInfo.InvariantCulture) * 1000);
                int randnum2 = (int)(float.Parse(randomSecs2Box.Text, CultureInfo.InvariantCulture) * 1000);
                randnum1 = (randnum1 == 0) ? 1 : randnum1;
                randnum2 = (randnum2 == 0) ? 1 : randnum2;

                Properties.Settings.Default.RandNum1 = randnum1;
                Properties.Settings.Default.RandNum2 = randnum2;
                Properties.Settings.Default.ClickType = clickTypeCBOX.Text;
                Properties.Settings.Default.RepeatTimesChecked = (bool)repeatTimesRBtn.IsChecked;
                Properties.Settings.Default.RepeatTimes = Int32.Parse(repeatTimesBox.Text);
                Properties.Settings.Default.CustomCoordsX = Int32.Parse(xBox.Text);
                Properties.Settings.Default.CustomCoordsY = Int32.Parse(yBox.Text);
                Properties.Settings.Default.CustomCoordsChecked = (bool)coordsCBtn.IsChecked;
                Properties.Settings.Default.MouseBtn = clickTypeCBOX.Text;

                Properties.Settings.Default.Save();
            }

        }

        private void setSleepValue(int sleep)
        {
            // Ensure that sleep is not less than 1 (as set in calcSleep)
            sleep = Math.Max(1, sleep);

            // Extract hours, minutes, seconds, and milliseconds from the total sleep time
            int hours = sleep / 3600000;
            sleep %= 3600000;

            int minutes = sleep / 60000;
            sleep %= 60000;

            int seconds = sleep / 1000;
            sleep %= 1000;

            int milliseconds = sleep;

            // Set the values back into the respective text boxes
            hoursBox.Text = hours.ToString();
            minsBox.Text = minutes.ToString();
            secondsBox.Text = seconds.ToString();
            millisecsBox.Text = milliseconds.ToString();
        }

        private void LoadAllSettings()
        {
            if(Properties.Settings.Default.SaveAllSettings)
            {
                // Load the saved sleep time and set the individual components
                setSleepValue(Properties.Settings.Default.Sleep);

                // Load the random interval mode
                randomIntervalMode.IsChecked = Properties.Settings.Default.UseRandomSleep;

                // Load the random intervals, converting milliseconds back to seconds
                randomSecs1Box.Text = (Properties.Settings.Default.RandNum1 / 1000.0f).ToString(CultureInfo.InvariantCulture);
                randomSecs2Box.Text = (Properties.Settings.Default.RandNum2 / 1000.0f).ToString(CultureInfo.InvariantCulture);

                // Load the click type
                clickTypeCBOX.Text = Properties.Settings.Default.ClickType;

                // Load the repeat times and coordinates checked states
                repeatTimesRBtn.IsChecked = Properties.Settings.Default.RepeatTimesChecked;
                coordsCBtn.IsChecked = Properties.Settings.Default.CustomCoordsChecked;

                // Load the repeat times value
                repeatTimesBox.Text = Properties.Settings.Default.RepeatTimes.ToString();

                // Load the custom coordinates values
                xBox.Text = Properties.Settings.Default.CustomCoordsX.ToString();
                yBox.Text = Properties.Settings.Default.CustomCoordsY.ToString();
            }

        }

        private void LoadSavedTheme()
        {
            string savedTheme = Properties.Settings.Default.SelectedTheme;
            string defaultTheme = Properties.Settings.Default.DefaultTheme;

            // Check if the saved theme starts with the "Themes/" directory
            if (savedTheme.StartsWith("Themes/"))
            {
                SetTheme(new Uri(savedTheme, UriKind.Relative));
            }
            // Check if the saved theme starts with the "UserThemes/" directory
            else if (savedTheme.StartsWith("UserThemes/"))
            {
                var absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, savedTheme);

                // Ensure the path exists before applying the theme
                if (File.Exists(absolutePath))
                {
                    SetTheme(new Uri(absolutePath, UriKind.Absolute));
                }
                else
                {
                    // Handle missing theme (e.g., if the user deleted the last applied theme)
                    SetTheme(new Uri(defaultTheme, UriKind.Relative)); // Revert to default light theme
                    Properties.Settings.Default.SelectedTheme = defaultTheme; // Save the default theme
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                // Optionally revert to default theme here as well to ensure the app doesn't crash
                Properties.Settings.Default.SelectedTheme = defaultTheme;
                Properties.Settings.Default.Save();
                SetTheme(new Uri(defaultTheme, UriKind.Relative));
            }
        }


        private void SetTheme(Uri themeUri)
        {
            ResourceDictionary Theme = new ResourceDictionary() { Source = themeUri };
            Application.Current.Resources.Clear();
            App.Current.Resources.MergedDictionaries.Add(Theme);
        }


        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSavedTheme();
            this.Topmost = Properties.Settings.Default.OnTopMost;

            if (Properties.Settings.Default.SaveAllSettings)
            {
                LoadAllSettings();
            }

            Thread keyhandler = new Thread(KeyHandler);
            keyhandler.Start();
            LoadKeybind();
        }


        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Properties.Settings.Default.SaveAllSettings)
            {
                SaveAllSettings();
            }
            Environment.Exit(0);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System)
            {
                e.Handled = true;
            }
        }

        private void closeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void minimizeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void getCoordsBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            GetCursorPos win = new GetCursorPos();
            win.Owner = this;
            win.Show();
            win.Activate();
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleClick();
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleClick();
        }

        private void changeHotkeyBtn_Click(object sender, RoutedEventArgs e)
        {
            keyEnabled = false;
            ChangeHotkey win = new ChangeHotkey();
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.Owner = this;
            win.ShowDialog();
        }

        private void windowSettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowSettings win = new WindowSettings();
            win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            win.Owner = this;
            win.ShowDialog();
        }
    }
}
