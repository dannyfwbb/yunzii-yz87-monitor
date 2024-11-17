using System.Drawing.Drawing2D;

namespace YZ87Monitor.Helpers
{
    internal static class IconGenerator
    {
        private const int IconWidth = 64;
        private const int IconHeight = 64;
        private const int BatteryWidth = 50;
        private const int BatteryHeight = 30;
        private const int BatteryX = 7;
        private const int BatteryY = 17;
        private const int TipWidth = 6;
        private const int TipHeight = 10;

        private static readonly Dictionary<int, Color> BatteryColorMap = new()
        {
            { -1, Color.Gray },  // Error state
            { 20, Color.Red },    // Below 20%
            { 50, Color.Orange }, // Below 50%
            { 100, Color.Green }  // 50% and above
        };

        /// <summary>
        /// Generates a tray icon, percentage text, and color-coded level.
        /// </summary>
        /// <param name="batteryPercentage">The battery percentage to display. -1 indicates an error state.</param>
        /// <returns>A dynamically generated icon.</returns>
        public static Icon GenerateTrayIcon(int batteryPercentage)
        {
            using var bitmap = new Bitmap(IconWidth, IconHeight);
            using var graphics = Graphics.FromImage(bitmap);

            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.Transparent);

            var batteryColor = BatteryColorMap
                .Where(kvp => batteryPercentage < kvp.Key)
                .OrderBy(kvp => kvp.Key)
                .FirstOrDefault().Value;

            using var pen = new Pen(Color.LightGray, 2);

            graphics.DrawRectangle(pen, BatteryX, BatteryY, BatteryWidth, BatteryHeight);
            graphics.FillRectangle(Brushes.Gray, BatteryX + BatteryWidth, BatteryY + (BatteryHeight - TipHeight) / 2, TipWidth, TipHeight);

            if (batteryPercentage >= 0)
            {
                int fillWidth = (int)((BatteryWidth - 4) * (batteryPercentage / 100.0));
                graphics.FillRectangle(new SolidBrush(batteryColor), BatteryX + 2, BatteryY + 2, fillWidth, BatteryHeight - 4);
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }
    }
}
