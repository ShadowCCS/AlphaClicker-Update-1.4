using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace AlphaClicker
{
    public partial class ChangeHotkey : Window
    {
        public ChangeHotkey()
        {
            InitializeComponent();
        }

        private void hotkeyWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void closeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }


        private string CodeToSpecialKey(int key)
        {
            // Existing method
            switch (key)
            {
                case 96: return "NUMPAD0";
                case 97: return "NUMPAD1";
                case 98: return "NUMPAD2";
                case 99: return "NUMPAD3";
                case 100: return "NUMPAD4";
                case 101: return "NUMPAD5";
                case 102: return "NUMPAD6";
                case 103: return "NUMPAD7";
                case 104: return "NUMPAD8";
                case 105: return "NUMPAD9";
                case 106: return "MULTIPLY";
                case 107: return "ADD";
                case 108: return "SEPARATOR";
                case 109: return "SUBTRACT";
                case 110: return "DECIMAL";
                case 111: return "DIVIDE";
                case 112: return "F1";
                case 113: return "F2";
                case 114: return "F3";
                case 115: return "F4";
                case 116: return "F5";
                case 117: return "F6";
                case 118: return "F7";
                case 119: return "F8";
                case 120: return "F9";
                case 121: return "F10";
                case 122: return "F11";
                case 123: return "F12";
            }
            return "";
        }

        private string CodeToSpecialMouseButton(int key)
        {
            Debug.WriteLine($"CodeToSpecialMouseButton called with key: {key}");

            switch (key)
            {
                case (int)VK.VK_MBUTTON:
                    Debug.WriteLine("Resolved to Middle Mouse Button");
                    return "MMB"; // Middle Mouse Button
                case (int)VK.VK_XBUTTON1:
                    Debug.WriteLine("Resolved to Extra Mouse Button 1");
                    return "X1";  // Extra Mouse Button 1
                case (int)VK.VK_XBUTTON2:
                    Debug.WriteLine("Resolved to Extra Mouse Button 2");
                    return "X2";  // Extra Mouse Button 2
                default:
                    Debug.WriteLine("Unrecognized mouse button code");
                    return ""; // Return empty string for unrecognized keys
            }
        }

        private bool hasSpecKey = false;
        private int key = -1;

        private void hotkeyWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (keyBox.Text == "Press Keys")
            {
                keyBox.Clear();
            }

            if (!startBtn.IsEnabled)
            {
                for (int i = 8; i < 123; i++)
                {
                    if (WinApi.GetAsyncKeyState(i) > 0)
                    {
                        if (i >= 16 && i <= 18) // Special keys (shift, ctrl, alt)
                        {
                            if (hasSpecKey == false)
                            {
                                keyBox.AppendText(e.Key.ToString().Replace("Left", "")
                                    .Replace("Right", "") + " + ");
                                key = i;
                                hasSpecKey = true;
                            }
                        }
                        else if (i >= 48 && i <= 90) // Characters and numbers
                        {
                            keyBox.AppendText(((char)i).ToString());
                            startBtn.IsEnabled = true;
                            key = i;
                            okBtn.IsEnabled = true;
                            break;
                        }
                        else if (i >= 96 && i <= 123) // Function keys, numpad, etc.
                        {
                            keyBox.AppendText(CodeToSpecialKey(i));
                            startBtn.IsEnabled = true;
                            key = i;
                            okBtn.IsEnabled = true;
                            break;
                        }
                    }
                }
            }
        }

        private void hotkeyWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!startBtn.IsEnabled)
            {
                try
                {
                    if (e.ChangedButton == MouseButton.Middle) // Middle Mouse Button
                    {
                        HandleSpecialMouseInput((int)VK.VK_MBUTTON);
                    }
                    else if (e.ChangedButton == MouseButton.XButton1) // Extra Mouse Button 1
                    {
                        HandleSpecialMouseInput((int)VK.VK_XBUTTON1);
                    }
                    else if (e.ChangedButton == MouseButton.XButton2) // Extra Mouse Button 2
                    {
                        HandleSpecialMouseInput((int)VK.VK_XBUTTON2);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error handling mouse input: {ex.Message}");
                }
            }
        }


        private void HandleSpecialMouseInput(int mouseButtonCode)
        {
            if (mouseButtonCode < 0) // Check if mouseButtonCode is valid
            {
                MessageBox.Show("Invalid mouse button code.");
                return;
            }

            if (keyBox.Text == "Press Keys")
            {
                keyBox.Clear();
            }

            // Append the mouse button name to the keyBox
            string buttonName = CodeToSpecialMouseButton(mouseButtonCode);
            if (!string.IsNullOrEmpty(buttonName))
            {
                keyBox.AppendText(buttonName);
                startBtn.IsEnabled = true;
                key = mouseButtonCode; // Store the mouse button code
                okBtn.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Unrecognized mouse button.");
            }
        }


        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            okBtn.IsEnabled = false;
            hasSpecKey = false;
            startBtn.IsEnabled = false;
            keyBox.Text = "Press Keys";
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.TogglePlay = keyBox.Text;
            Properties.Settings.Default.Save();
            MessageBox.Show(Properties.Settings.Default.TogglePlay.ToString());
            ((MainWindow)this.Owner).LoadKeybind();
            Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void hotkeyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            keyBox.Text = Properties.Settings.Default.TogglePlay;
        }

        private void hotkeyWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((MainWindow)this.Owner).keyEnabled = true;
        }

        private void keyBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // Handle text changes if necessary
        }
    }
}
