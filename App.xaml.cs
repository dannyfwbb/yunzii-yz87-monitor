using System.Windows;
using Timer = System.Windows.Forms.Timer;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using YZ87Monitor.Models;
using YZ87Monitor.Helpers;
using Microsoft.Win32;
using System.Diagnostics;
using System.Reflection;

namespace YZ87Monitor
{
    public partial class App : Application
    {
        private NotifyIcon? _trayIcon;
        private Timer? _updateTimer;

        private const string AppName = "Yunzii YZ87 Battery Monitor";
        private const string StartupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string GitHubRepoUrl = "https://github.com/dannyfwbb/yunzii-yz87-monitor";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _trayIcon = new NotifyIcon
            {
                Icon = IconGenerator.GenerateTrayIcon(0),
                Visible = true,
                Text = "Battery Monitor"
            };

            var contextMenu = new ContextMenuStrip();

            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            var appNameItem = new ToolStripMenuItem($"{AppName} v{version}")
            {
                Enabled = true
            };
            appNameItem.Click += (s, e) => NavigateToGitHub();

            contextMenu.Items.Add(appNameItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            var startupItem = new ToolStripMenuItem("Start with Windows")
            {
                CheckOnClick = true,
                Checked = IsStartupEnabled()
            };
            startupItem.CheckedChanged += (s, e) =>
            {
                if (startupItem.Checked)
                {
                    AddToStartup();
                }
                else
                {
                    RemoveFromStartup();
                }
            };
            contextMenu.Items.Add(startupItem);
            contextMenu.Items.Add("Refresh Battery State", null, OnRefreshBatteryStateClicked);
            contextMenu.Items.Add("Exit", null, OnExitClicked);

            _trayIcon.ContextMenuStrip = contextMenu;

            _updateTimer = new Timer { Interval = 60000 };
            _updateTimer.Tick += OnUpdateTimerTick;
            _updateTimer.Start();

            // Initial update
            UpdateBatteryState();
        }

        private static bool IsStartupEnabled()
        {
            using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, writable: false);
            return key?.GetValue(AppName) != null;
        }

        private static void AddToStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, writable: true)
                                ?? Registry.CurrentUser.CreateSubKey(StartupRegistryPath);
                var executablePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (executablePath != null)
                {
                    key.SetValue(AppName, executablePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to add to startup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void RemoveFromStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(StartupRegistryPath, writable: true);
                key?.DeleteValue(AppName, throwOnMissingValue: false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove from startup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NavigateToGitHub()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = GitHubRepoUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open GitHub link: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            _updateTimer?.Dispose();
            base.OnExit(e);
        }

        private void OnRefreshBatteryStateClicked(object? sender, EventArgs e)
        {
            UpdateBatteryState();
        }

        private void OnExitClicked(object? sender, EventArgs e)
        {
            Shutdown();
        }

        private void OnUpdateTimerTick(object? sender, EventArgs e)
        {
            UpdateBatteryState();
        }

        private void UpdateBatteryState()
        {
            try
            {
                var keyboard = new YunziiKeyboard();
                int batteryState = keyboard.ReadBatteryState();

                Console.WriteLine("Battary state: {0}", batteryState);

                _trayIcon!.Icon = IconGenerator.GenerateTrayIcon(batteryState);
                _trayIcon.Text = $"Battery State: {batteryState}%";
            }
            catch (Exception ex)
            {
                _trayIcon!.Icon = IconGenerator.GenerateTrayIcon(-1);
                _trayIcon.Text = $"Failed to read battery state: {ex.Message}";
            }
        }
    }
}
