using HidLibrary;

namespace YZ87Monitor.Helpers;

internal class HidReportBuilder
{
    private readonly byte[] _data;

    public HidReportBuilder(int byteLength)
    {
        if (byteLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(byteLength), "Byte length must be greater than zero.");

        _data = new byte[byteLength];
    }

    public HidReportBuilder SetByte(int position, byte value)
    {
        if (position < 0 || position >= _data.Length)
            throw new ArgumentOutOfRangeException(nameof(position), "Position is out of bounds.");

        _data[position] = value;
        return this;
    }

    public HidReport Build()
    {
        var report = new HidReport(_data.Length) { Data = _data };
        return report;
    }
}
