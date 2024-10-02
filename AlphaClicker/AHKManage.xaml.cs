using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;


namespace AlphaClicker
{
    /// <summary>
    /// Interaction logic for AHKManage.xaml
    /// </summary>
    public partial class AHKManage : Window
    {
        public AHKManage()
        {
            InitializeComponent();
        }

        private void InstallButton_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.autohotkey.com/download/ahk-v2.exe"; // Autohotkey download
            try
            {
                // Open the URL in the default web browser
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true // Necessary to open URLs in .NET Core and later versions
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to download: {ex.Message}"+ " " + "Please download manually from autohotkey.com");
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            // Create an instance of OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set filter options for the file dialog (optional)
            openFileDialog.Filter = "AutoHotkey Executables (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog.Title = "Select AutoHotkey Executable";

            // Show the dialog and check if the user selected a file
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // Get the selected file path
                string selectedFilePath = openFileDialog.FileName;

                Properties.Settings.Default.AHKInstallPath = selectedFilePath;
                Properties.Settings.Default.Save();

                // Update the label or any other UI element with the selected file path
                CurrentPathLabel.Content = $"Current AHK Path: {selectedFilePath}";
            }
        }
        private void AHK_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void closeButton_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            Close();
        }
    }
}
