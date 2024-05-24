using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Windows_Update_Delayer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 将窗口水平居中，垂直偏上一点
            WindowStartupLocation = WindowStartupLocation.Manual;
            var screenWidth = SystemParameters.WorkArea.Width;
            var screenHeight = SystemParameters.WorkArea.Height;
            Left = (screenWidth - Width) / 2;
            Top = screenHeight / 3.5;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                // 使用默认浏览器打开链接
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        private async void HandleError(Action action, Button button)
        {
            try
            {
                var successMessage = "✅ 成功！";
                var originalText = button.Content.ToString();  // 保存原始的按钮标签文字
                if (originalText == successMessage)
                    return;  // 如果按钮已经显示成功消息，则不再执行后续操作

                action();
                button.Content = successMessage;  // 设置按钮标签为“操作成功！”
                await Task.Delay(1500);  // 提示的显示时长
                button.Content = originalText;  // 恢复按钮的原始标签文字
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePauseEndTime(string endDate)
        {
                var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                {
                    var keyPath = @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings";
                    using (var key = baseKey.OpenSubKey(keyPath, true))
                    {
                        if (key != null)
                        {
                            key.SetValue("FlightSettingsMaxPauseDays", 0x2AE4, RegistryValueKind.DWord);
                            key.SetValue("PauseFeatureUpdatesStartTime", "2024-01-01T10:00:00Z", RegistryValueKind.String);
                            key.SetValue("PauseFeatureUpdatesEndTime", endDate, RegistryValueKind.String);
                            key.SetValue("PauseQualityUpdatesStartTime", "2024-01-01T10:00:00Z", RegistryValueKind.String);
                            key.SetValue("PauseQualityUpdatesEndTime", endDate, RegistryValueKind.String);
                            key.SetValue("PauseUpdatesStartTime", "2024-01-01T10:00:00Z", RegistryValueKind.String);
                            key.SetValue("PauseUpdatesExpiryTime", endDate, RegistryValueKind.String);
                        }
                    }
                }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            HandleError(() =>
            {
                UpdatePauseEndTime("2026-01-01T10:00:00Z");
            }, (Button)sender);
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            HandleError(() =>
            {
                UpdatePauseEndTime("2035-01-01T10:00:00Z");
            }, (Button)sender);
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            HandleError(() =>
            {
                UpdatePauseEndTime("2054-01-01T10:00:00Z");
            }, (Button)sender);
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            HandleError(() =>
            {
                var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                {
                    var keyPath = @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings";
                    using (var key = baseKey.OpenSubKey(keyPath, true))
                    {
                        if (key != null)
                        {
                            key.SetValue("FlightSettingsMaxPauseDays", 0x23, RegistryValueKind.DWord);
                            key.DeleteValue("PauseFeatureUpdatesStartTime", false);
                            key.DeleteValue("PauseFeatureUpdatesEndTime", false);
                            key.DeleteValue("PauseQualityUpdatesStartTime", false);
                            key.DeleteValue("PauseQualityUpdatesEndTime", false);
                            key.DeleteValue("PauseUpdatesStartTime", false);
                            key.DeleteValue("PauseUpdatesExpiryTime", false);
                        }
                    }
                }

            }, (Button)sender);
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            HandleError(() =>
            {
                // 打开 Windows 更新 页面
                Process.Start("ms-settings:about");
                Thread.Sleep(TimeSpan.FromSeconds(0.5));
                Process.Start("ms-settings:windowsupdate");

            }, (Button)sender);
        }

    }
}
