using HidLibrary;
using YZ87Monitor.Exceptions;
using YZ87Monitor.Helpers;

namespace YZ87Monitor.Models;

internal class YunziiKeyboard : IDisposable
{
    private const int VendorId = 0x05AC;
    private const int ProductId = 0x024F;
    private const string InterfaceNumber = "mi_03";

    private HidDevice? _device;

    public YunziiKeyboard()
    {
        Connect();
    }

    /// <summary>
    /// Reads the battery state from the keyboard.
    /// </summary>
    /// <returns>The battery level as an integer.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the device is not connected.</exception>
    /// <exception cref="HidCommunicationException">Thrown when communication with the device fails.</exception>
    public async Task<int> ReadBatteryState()
    {
        EnsureDeviceConnected();

        // Prepare the HID report
        var report = new HidReportBuilder(_device!.Capabilities.InputReportByteLength)
            .SetByte(0, 0x20)
            .SetByte(1, 0x01)
            .SetByte(31, 0x21)
            .Build();

        // Write the report to the device
        if (!await WriteReportAsync(report))
        {
            throw new HidCommunicationException("Failed to send the HID report to the device.");
        }

        // Read the response
        var response = await ReadReportAsync();

        // Validate response length and extract battery state
        if (response.Data.Length > 3)
        {
            return response.Data[3];
        }
        else
        {
            throw new HidCommunicationException("Received an incomplete response from the device.");
        }
    }

    /// <summary>
    /// Sends a HID report to the connected device asynchronously, with a configurable timeout.
    /// </summary>
    /// <param name="report">The HID report to send.</param>
    /// <param name="timeoutMs">The maximum time, in milliseconds, to wait for the operation to complete. Defaults to 5000ms.</param>
    /// <returns>A task representing the asynchronous operation. Returns <c>true</c> if the report was successfully sent; otherwise, <c>false</c>.</returns>
    /// <exception cref="HidCommunicationException">Thrown if the device is not connected.</exception>
    /// <exception cref="TimeoutException">Thrown if the operation exceeds the specified timeout.</exception>
    private async Task<bool> WriteReportAsync(HidReport report, int timeoutMs = 5000)
    {
        using var cts = new CancellationTokenSource(timeoutMs);
        var task = Task.Run(() =>
        {
            if (_device == null)
            {
                throw new HidCommunicationException("Device is not connected.");
            }
            return _device.WriteReport(report);
        });

        var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMs, cts.Token));
        if (completedTask == task)
        {
            return await task; // Successfully completed within timeout
        }
        else
        {
            throw new HidCommunicationException("Failed to write a report to the device by timeout.");
        }
    }

    /// <summary>
    /// Reads a HID report from the connected device asynchronously, with a configurable timeout.
    /// </summary>
    /// <param name="timeoutMs">The maximum time, in milliseconds, to wait for the operation to complete. Defaults to 5000ms.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the <see cref="HidReport"/> received from the device.
    /// </returns>
    /// <exception cref="HidCommunicationException">Thrown if the device is not connected or if reading the report fails.</exception>
    /// <exception cref="TimeoutException">Thrown if the operation exceeds the specified timeout.</exception>
    private async Task<HidReport> ReadReportAsync(int timeoutMs = 5000)
    {
        using var cts = new CancellationTokenSource(timeoutMs);
        var task = Task.Run(() =>
        {
            if (_device == null)
            {
                throw new HidCommunicationException("Device is not connected.");
            }
            var report = _device.ReadReport();
            if (report == null)
            {
                throw new HidCommunicationException("Failed to read a report from the device.");
            }
            return report;
        });

        var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMs, cts.Token));
        if (completedTask == task)
        {
            return await task; // Successfully completed within timeout
        }
        else
        {
            throw new HidCommunicationException("Failed to read a report from the device by timeout.");
        }
    }

    /// <summary>
    /// Ensures the device is connected.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the device is not connected.</exception>
    private void EnsureDeviceConnected()
    {
        if (_device == null)
        {
            throw new InvalidOperationException("The device is not connected.");
        }
    }

    /// <summary>
    /// Establishes a connection to the device.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the device cannot be found or connected.</exception>
    private void Connect()
    {
        var device = HidDevices.Enumerate(VendorId, ProductId).FirstOrDefault(d => d.DevicePath.Contains(InterfaceNumber));

        if (device == null)
        {
            throw new InvalidOperationException("The device is not connected.");
        }

        _device = device;
    }

    /// <summary>
    /// Disposes the HID device when the instance is no longer needed.
    /// </summary>
    public void Dispose()
    {
        _device?.CloseDevice();
        _device = null;
    }
}
