using System;

namespace YZ87Monitor.Exceptions
{
    /// <summary>
    /// Represents errors that occur during HID communication.
    /// </summary>
    public class HidCommunicationException : Exception
    {
        public HidCommunicationException(string message) : base(message) { }

        public HidCommunicationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
