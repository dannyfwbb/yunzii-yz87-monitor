using System.Windows;
using Timer = System.Windows.Forms.Timer;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using YZ87Monitor.Models;
using YZ87Monitor.Helpers;
using System.Diagnostics;
using System.Reflection;

namespace YZ87Monitor;

public partial class App : Application
{
    private readonly NotifyIcon _trayIcon;
    private readonly Timer _updateTimer;

    private const string AppName = "Yunzii YZ87 Battery Monitor";
    private const string GitHubRepoUrl = "https://github.com/dannyfwbb/yunzii-yz87-monitor";

    public App()
    {
        _trayIcon = CreateTrayIcon();
        _updateTimer = CreateTimer();
    }

    protected async override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _updateTimer.Start();

        // Initial update
        await UpdateBatteryState();
    }

    private Timer CreateTimer()
    {
        var timer = new Timer { Interval = 60000 };
        timer.Tick += OnUpdateTimerTick;
        return timer;
    }

    private NotifyIcon CreateTrayIcon()
    {
        var trayIcon = new NotifyIcon
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
            Checked = RegistryHelper.IsStartupEnabled()
        };
        startupItem.CheckedChanged += (s, e) =>
        {
            if (startupItem.Checked)
            {
                RegistryHelper.AddToStartup();
            }
            else
            {
                RegistryHelper.RemoveFromStartup();
            }
        };
        contextMenu.Items.Add(startupItem);

        contextMenu.Items.Add("Refresh Battery State", null, OnRefreshBatteryStateClicked);
        contextMenu.Items.Add("Exit", null, OnExitClicked);

        trayIcon.ContextMenuStrip = contextMenu;

        return trayIcon;
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
        _trayIcon.Dispose();
        _updateTimer.Dispose();

        base.OnExit(e);
    }

    private async void OnRefreshBatteryStateClicked(object? sender, EventArgs e)
    {
        await UpdateBatteryState();
    }

    private void OnExitClicked(object? sender, EventArgs e)
    {
        Shutdown();
    }

    private async void OnUpdateTimerTick(object? sender, EventArgs e)
    {
        await UpdateBatteryState();
    }

    private async Task UpdateBatteryState()
    {
        try
        {
            var keyboard = new YunziiKeyboard();
            var batteryState = await keyboard.ReadBatteryState();

            Console.WriteLine("Battary state: {0}", batteryState);

            _trayIcon.Icon = IconGenerator.GenerateTrayIcon(batteryState);
            _trayIcon.Text = $"Battery State: {batteryState}%";

            keyboard.Dispose();
        }
        catch (Exception ex)
        {
            _trayIcon.Icon = IconGenerator.GenerateTrayIcon(-1);
            _trayIcon.Text = $"Failed to read battery state: {ex.Message}";
        }
    }
}