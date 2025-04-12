using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
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

    /// 检查程序是否以管理员程序运行
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!IsRunAsAdministrator())
            {
                MessageBox.Show("请右键单击程序，选择“以管理员身份运行”此程序。", "权限不足", MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown(); 
            }
        }

        private bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);
            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
    public partial class MainWindow : Window
    {
        /// 微调程序窗口默认位置
        public MainWindow()
        {
            InitializeComponent();

            // 将窗口水平居中，垂直偏上一点
            WindowStartupLocation = WindowStartupLocation.Manual;
            var screenWidth = SystemParameters.WorkArea.Width;
            var screenHeight = SystemParameters.WorkArea.Height;
            Left = (screenWidth - Width) / 2;
            Top = screenHeight / 4.5;
        }

        /// 超链接打开方式设置
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

        /// 按钮点击回馈
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

        /// 暂停更新时间设置
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

        /// 恢复Windows正常更新
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

                // 删除整个更新策略
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                {
                    var policyPath = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate";
                    baseKey.DeleteSubKeyTree(policyPath, throwOnMissingSubKey: false); 
                }

                // 删除 HwReqChk 路径
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                {
                    var hwReqChkPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\HwReqChk";
                    baseKey.DeleteSubKeyTree(hwReqChkPath, throwOnMissingSubKey: false);
                }

                // 删除 MoSetup 路径
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                {
                    var moSetupPath = @"SYSTEM\Setup\MoSetup";
                    baseKey.DeleteSubKeyTree(moSetupPath, throwOnMissingSubKey: false);
                }

            }, (Button)sender);
        }

        /// // 刷新 Windows 更新 页面
        private bool isProcessing = false;
        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (isProcessing)
                return;

            isProcessing = true;

            try
            {
                await Task.Run(() =>
                {
                    // 尝试关闭所有设置窗口
                    foreach (var proc in Process.GetProcessesByName("SystemSettings"))
                    {
                        try
                        {
                            proc.Kill();
                            proc.WaitForExit();
                        }
                        catch
                        {
                            // 忽略杀不掉的情况
                        }
                    }
                });

                await Task.Delay(500); // 稍等确保进程关闭

                // 启动更新页面
                Process.Start("ms-settings:windowsupdate");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开更新页面失败:\n{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 加一点冷却时间，防止连续点击出问题
                await Task.Delay(2000);
                isProcessing = false;
            }
        }

        // 设置禁止大版本更新
        private void Button_BlockFeatureUpdate(object sender, RoutedEventArgs e)
        {
            HandleError(() =>
            {
                var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
                {
                    var keyPath = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate";
                    using (var key = baseKey.CreateSubKey(keyPath)) 
                    {
                        if (key != null)
                        {
                            key.SetValue("TargetReleaseVersion", 1, RegistryValueKind.DWord);
                            key.SetValue("TargetReleaseVersionInfo", "ithaoge", RegistryValueKind.String);
                            key.DeleteValue("ProductVersion", throwOnMissingValue: false);
                        }
                    }
                }

            }, (Button)sender);
        }

        // 指定最高更新的大版本
        private void SetTargetReleaseVersion(string targetVersion)
        {
            var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
            {
                var keyPath = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate";
                using (var key = baseKey.CreateSubKey(keyPath))
                {
                    if (key != null)
                    {
                        key.SetValue("ProductVersion", "Windows 11", RegistryValueKind.String);
                        key.SetValue("TargetReleaseVersionInfo", targetVersion, RegistryValueKind.String);
                        key.SetValue("TargetReleaseVersion", 1, RegistryValueKind.DWord); // 启用版本锁定
                    }
                }
            }
        }
        
        // 解除Win11升级限制
        private void DisableUpgradeCompatibility()
        {
            var view = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view))
            {
                string[] keysToDelete =
                {
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\CompatMarkers",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Shared",
                    @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\TargetVersionUpgradeExperienceIndicators"
                };

                foreach (var keyPath in keysToDelete)
                {
                    try
                    {
                        baseKey.DeleteSubKeyTree(keyPath, false);
                    }
                    catch
                    {
                        // 忽略删除失败异常
                    }
                }

                string hwReqChkPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\HwReqChk";
                using (var hwKey = baseKey.CreateSubKey(hwReqChkPath))
                {
                    if (hwKey != null)
                    {
                        string[] multiSzValues = new[]
                        {
                    "SQ_SecureBootCapable=TRUE",
                    "SQ_SecureBootEnabled=TRUE",
                    "SQ_TpmVersion=2",
                    "SQ_RamMB=8192",
                    "SQ_SSE2ProcessorSupport=TRUE",
                    "SQ_SSE4_2ProcessorSupport=TRUE",
                    "SQ_NXProcessorSupport=TRUE",
                    "SQ_CompareExchange128=TRUE",
                    "SQ_LahfSahfSupport=TRUE",
                    "SQ_PrefetchWSupport=TRUE",
                    "SQ_PopCntInstructionSupport=TRUE",
                    "SQ_SystemDiskSizeMB=99999",
                    "SQ_CpuCoreCount=9",
                    "SQ_CpuModel=99",
                    "SQ_CpuFamily=99",
                    "SQ_CpuMhz=9999",
                    "" 
                };

                        hwKey.SetValue("HwReqChkVars", multiSzValues, RegistryValueKind.MultiString);
                    }
                }

                string moSetupPath = @"SYSTEM\Setup\MoSetup";
                using (var moKey = baseKey.CreateSubKey(moSetupPath))
                {
                    if (moKey != null)
                    {
                        moKey.SetValue("AllowUpgradesWithUnsupportedTPMOrCPU", 1, RegistryValueKind.DWord);
                    }
                }
            }
        }


        // 具体指定最高更新的Win11大版本
        private void Button_StayOn22H2(object sender, RoutedEventArgs e)
        {
            HandleError(() =>
            {
                SetTargetReleaseVersion("22H2");
                DisableUpgradeCompatibility();
            }, (Button)sender);
        }

        private void Button_StayOn23H2(object sender, RoutedEventArgs e)
        {
            HandleError(() =>
            {
                SetTargetReleaseVersion("23H2");
                DisableUpgradeCompatibility();
            }, (Button)sender);
        }

        private void Button_StayOn24H2(object sender, RoutedEventArgs e)
        {
            HandleError(() =>
            {
                SetTargetReleaseVersion("24H2");
                DisableUpgradeCompatibility();

            }, (Button)sender);
        }


    }
}
