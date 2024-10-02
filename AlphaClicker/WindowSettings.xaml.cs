using System.IO;
using System.Linq;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Media;

namespace AlphaClicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WindowSettings: Window
    {

        private bool isInitializingJitterSelector = true; // Flag to track initialization

        public WindowSettings()
        {
            InitializeComponent();
            LoadSavedTheme();
            LoadSettings();
            isInitializingJitterSelector = false; // Set to false after initialization
            LoadThemes();
        }


        private void LoadSavedTheme() //Uses properties to retrive the last theme
        {
            ThemeSelector.SelectionChanged -= ThemeSelector_SelectionChanged;

            string savedTheme = Properties.Settings.Default.SelectedTheme;
            string defaultTheme = Properties.Settings.Default.DefaultTheme;


            {
                // Determine if the theme is predefined or user-defined
                if (savedTheme.StartsWith("Themes/"))
                {
                    // Predefined theme
                    SetTheme(new Uri(savedTheme, UriKind.Relative));
                }
                else if (savedTheme.StartsWith("UserThemes/"))
                {
                    // User-defined theme
                    var absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, savedTheme);

                    if (File.Exists(absolutePath))
                    {
                        SetTheme(new Uri(absolutePath, UriKind.Absolute));
                    }
                    else
                    {
                        // Handle case where user-defined theme file no longer exists
                        // Handle missing theme (e.g., if the user deleted the last applied theme)
                        SetTheme(new Uri(defaultTheme, UriKind.Relative)); // Revert to default light theme
                        Properties.Settings.Default.SelectedTheme = defaultTheme; // Save the default theme
                        Properties.Settings.Default.Save();
                    }
                }
                else
                {
                    // Fallback to default theme if an unknown URI is found
                    Properties.Settings.Default.SelectedTheme = defaultTheme;
                    Properties.Settings.Default.Save();
                    SetTheme(new Uri(defaultTheme, UriKind.Relative));
                }

            }
        }

        private void LoadSettings()
        {
            topMostSwitch.IsChecked = Properties.Settings.Default.OnTopMost;
            saveAllSettingsSwitch.IsChecked = Properties.Settings.Default.SaveAllSettings;

            // Load the Jitter Factor setting with validation
            int jitterFactor = Properties.Settings.Default.JitterFactor;
            if (jitterFactor >= 0 && jitterFactor <= 5)
            {
                JitterSelector.SelectedIndex = jitterFactor;
            }
            else
            {
                // Reset to default if the value is invalid
                JitterSelector.SelectedIndex = 0; // Assuming 0 (None) is the default
            }

            //string processName = Properties.Settings.Default.ProcessName;
            //int processHWND = Properties.Settings.Default.ProcessHWND;

            //if (!string.IsNullOrEmpty(processName) && processHWND != 0)
            //{
            //    selectedProcessLabel.Content = processName + " {" + processHWND + "}";
            //}
            //else
            //{
            //    selectedProcessLabel.Content = "No process selected";
            //}

        }

        private void LoadThemes()
        {
            // Clear existing items in the ComboBox, this is important to make sure nothing is lingering or accidentally added
            ThemeSelector.Items.Clear();

            // Add predefined themes from the /themes folder, the names and location here is important which is not the case for the user defined ones
            ThemeSelector.Items.Add(new ComboBoxItem { Content = "Light", Tag = "Themes/LightTheme.xaml" });
            ThemeSelector.Items.Add(new ComboBoxItem { Content = "Dark", Tag = "Themes/DarkTheme.xaml" });

            // Load user-defined themes
            string userThemesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserThemes");
            if (Directory.Exists(userThemesPath))
            {
                var themeFiles = Directory.GetFiles(userThemesPath, "*.xaml");
                foreach (var themeFile in themeFiles)
                {
                    string themeName = Path.GetFileNameWithoutExtension(themeFile);
                    string themePath = $"UserThemes/{themeName}.xaml";

                    // Check if the theme name is "Template" or "Base"
                    if (themeName.Equals("Template", StringComparison.OrdinalIgnoreCase) ||
                        themeName.Equals("Base", StringComparison.OrdinalIgnoreCase))
                    {
                        // Skip adding the theme to the ComboBox if it's "Template" or "Base"
                        continue;
                    }

                    // Add the theme to the ComboBox if it's not "Template" or "Base"
                    ThemeSelector.Items.Add(new ComboBoxItem { Content = themeName, Tag = themePath });
                }
            }

            //sets the user defined theme from the properties
            string savedTheme = Properties.Settings.Default.SelectedTheme;
            var selectedItem = ThemeSelector.Items
            .OfType<ComboBoxItem>()
            .FirstOrDefault(item => item.Tag.ToString() == savedTheme);

            if (selectedItem != null)
            {
                ThemeSelector.SelectedItem = selectedItem;
            }
            ThemeSelector.SelectionChanged += ThemeSelector_SelectionChanged;
        }


        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string defaultTheme = Properties.Settings.Default.DefaultTheme;

            // Ensure there is a selected item in the ThemeSelector
            if (ThemeSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                string themeUri = selectedItem.Tag.ToString();

                // Save the selected theme to the settings
                Properties.Settings.Default.SelectedTheme = themeUri;
                Properties.Settings.Default.Save();

                // Determine if the theme is predefined or user-defined
                if (themeUri.StartsWith("Themes/"))
                {
                    // Predefined theme - apply it as a relative URI
                    SetTheme(new Uri(themeUri, UriKind.Relative));
                }
                else if (themeUri.StartsWith("UserThemes/"))
                {
                    // User-defined theme - create the absolute path
                    var absolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, themeUri);

                    // Check if the user-defined theme file exists
                    if (File.Exists(absolutePath))
                    {
                        // Apply the user-defined theme using an absolute URI
                        SetTheme(new Uri(absolutePath, UriKind.Absolute));
                    }
                    else
                    {
                        // User-defined theme file doesn't exist, revert to the default theme
                        SetTheme(new Uri(defaultTheme, UriKind.Relative)); // Revert to default theme
                        Properties.Settings.Default.SelectedTheme = defaultTheme; // Save the default theme
                        Properties.Settings.Default.Save();
                    }
                }
                else
                {
                    // Unknown theme format, fallback to default theme
                    SetTheme(new Uri(defaultTheme, UriKind.Relative)); // Revert to default theme
                    Properties.Settings.Default.SelectedTheme = defaultTheme; // Save the default theme
                    Properties.Settings.Default.Save();
                }
            }
        }



        private void SetTheme(Uri themeUri)
        {
            //sets application resource to URI
            string themeURLString = themeUri.ToString();


            ResourceDictionary Theme = new ResourceDictionary() { Source = themeUri };

            Application.Current.Resources.Clear();
            App.Current.Resources.MergedDictionaries.Add(Theme);
        }


        private void JitterSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInitializingJitterSelector)
            {
                return; // Exit if initializing
            }

            if (JitterSelector.SelectedItem is ComboBoxItem selectedItem)
            {
                string content = selectedItem.Content.ToString();
                if (int.TryParse(content.Substring(0, 1), out int jitterFactor))
                {
                    // Use the jitterFactor as needed in your logic
                    Properties.Settings.Default.JitterFactor = jitterFactor;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void windowSettings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Makes the window draggable
            DragMove();
        }

        private void closeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Closes the application
            Close();
        }

        private void windowSettings_Loaded(object sender, RoutedEventArgs e)
        {
            //Irelevant, previously used
        }

        private void windowSettings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((MainWindow)this.Owner).keyEnabled = true; //Important, do not remove
        }


        //Saves to settings on action
        private void topMostSwitch_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.OnTopMost = true; 
            Properties.Settings.Default.Save();
            SetOwnerTopMost(true);
        }

        private void topMostSwitch_UnChecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.OnTopMost = false;
            Properties.Settings.Default.Save();
            SetOwnerTopMost(false);
        }

        private void saveAllSettingsSwitch_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SaveAllSettings = true;
            Properties.Settings.Default.Save();

        }

        private void saveAllSettingsSwitch_UnChecked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.SaveAllSettings = false;
            Properties.Settings.Default.Save();
        }


        private void SetOwnerTopMost(bool Bool)
        {
            if(Owner != null)
            {
                Owner.Topmost = Bool;
            }
        }


        //private void CheckAHKInstalled() //A external program that could potentially handle process selection and clicking, it is currently disabled due to issues, but is left here currently
        //{
        //    string AHKInstallPath = Properties.Settings.Default.AHKInstallPath;

        //    if (!string.IsNullOrEmpty(AHKInstallPath))
        //    {
        //        if (File.Exists(AHKInstallPath))
        //        {
        //            // Create and configure the ProcessSelectionWindow
        //            ProcessSelectionWindow win = new ProcessSelectionWindow
        //            {
        //                WindowStartupLocation = WindowStartupLocation.CenterOwner,
        //                Owner = this
        //            };

        //            // Show the dialog and check the result
        //            bool? result = win.ShowDialog();
        //            if (result == true)
        //            {
        //                // The dialog was closed with DialogResult = true
        //                string processName = Properties.Settings.Default.ProcessName;
        //                int processHWND = Properties.Settings.Default.ProcessHWND;

        //                if (!string.IsNullOrEmpty(processName) && processHWND != 0)
        //                {
        //                    selectedProcessLabel.Content = processName + " {" + processHWND + "}";
        //                }
        //                else
        //                {
        //                    selectedProcessLabel.Content = "No process selected";
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // AutoHotkey executable does not exist at the provided path, prompt user
        //            MessageBox.Show("The specified AutoHotkey executable was not found. Please select the correct path.",
        //                            "AutoHotkey Not Found",
        //                            MessageBoxButton.OK,
        //                            MessageBoxImage.Warning);
        //            PromptAHKInstall();
        //        }
        //    }
        //    else
        //    {
        //        // No path provided, prompt user to enter the path
        //        PromptAHKInstall();
        //    }
        //}

        //private void PromptAHKInstall()
        //{
        //    AHKManage win = new AHKManage();
        //    win.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        //    win.Owner = this;
        //    win.Show();
        //}

        private void processSelectionWindowButton_Click(object sender, RoutedEventArgs e)
        {
            //CheckAHKInstalled();
        }


    }
}
