using HidLibrary;
using YZ87Monitor.Exceptions;
using YZ87Monitor.Helpers;

namespace YZ87Monitor.Models
{
    internal class YunziiKeyboard : IDisposable
    {
        private const int VendorId = 0x05AC;
        private const int ProductId = 0x024F;
        private const string InterfaceNumber = "mi_03";

        private HidDevice? _device;

        public YunziiKeyboard()
        {
            try
            {
                Connect();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to initialize YunziiKeyboard: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Reads the battery state from the keyboard.
        /// </summary>
        /// <returns>The battery level as an integer.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the device is not connected.</exception>
        /// <exception cref="HidCommunicationException">Thrown when communication with the device fails.</exception>
        public int ReadBatteryState()
        {
            EnsureDeviceConnected();

            // Prepare the HID report
            var report = new HidReportBuilder(_device!.Capabilities.InputReportByteLength)
                .SetByte(0, 0x20)
                .SetByte(1, 0x01)
                .SetByte(31, 0x21)
                .Build();

            // Write the report to the device
            if (!_device!.WriteReport(report))
            {
                throw new HidCommunicationException("Failed to send the HID report to the device.");
            }

            // Read the response
            var response = _device.ReadReport();

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
}
