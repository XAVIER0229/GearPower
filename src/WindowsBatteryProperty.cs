using System;
using System.Globalization;
using System.Text;

namespace GearPower
{
    internal static class WindowsBatteryProperty
    {
        internal static bool IsClassicBluetoothInstance(string instance)
        {
            if (string.IsNullOrEmpty(instance)) return false;
            return instance.StartsWith("BTHENUM\\", StringComparison.OrdinalIgnoreCase)
                || instance.StartsWith("BTHHFENUM\\", StringComparison.OrdinalIgnoreCase)
                || instance.StartsWith("HID\\{00001124-0000-1000-8000-00805f9b34fb}", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsBluetoothInstance(string instance)
        {
            if (string.IsNullOrEmpty(instance)) return false;
            return instance.StartsWith("BTHENUM\\", StringComparison.OrdinalIgnoreCase)
                || instance.StartsWith("BTHHFENUM\\", StringComparison.OrdinalIgnoreCase)
                || instance.StartsWith("BTHLE\\", StringComparison.OrdinalIgnoreCase)
                || instance.StartsWith("BTHLEDEVICE\\", StringComparison.OrdinalIgnoreCase)
                || instance.StartsWith("HID\\{00001124-0000-1000-8000-00805f9b34fb}", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsAudioInstance(string instance)
        {
            string lower = (instance ?? "").ToLowerInvariant();
            return lower.Contains("audio") || lower.Contains("a2dp") || lower.Contains("hfp")
                || lower.Contains("{0000111e-") // Hands-Free
                || lower.Contains("{0000110b-") // Audio Sink
                || lower.Contains("{00001108-"); // Headset
        }

        // DEVPROPTYPE values from Microsoft's DEVPROPERTY documentation.
        // BYTE=3, UINT16=5, INT32=6, UINT32=7, STRING=18.
        // BOOLEAN (17), GUID (13), and security descriptors (19) are not levels.
        internal static int? Parse(uint type, byte[] data)
        {
            if (data == null) return null;
            long value;
            if (type == 3 && data.Length == 1) value = data[0];
            else if (type == 5 && data.Length == 2) value = BitConverter.ToUInt16(data, 0);
            else if (type == 6 && data.Length == 4) value = BitConverter.ToInt32(data, 0);
            else if (type == 7 && data.Length == 4) value = BitConverter.ToUInt32(data, 0);
            else if (type == 18 && data.Length >= 2 && data.Length % 2 == 0)
            {
                string text = Encoding.Unicode.GetString(data).TrimEnd('\0').Trim().TrimEnd('%').Trim();
                int parsed;
                if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)) return null;
                value = parsed;
            }
            else return null;
            return value >= 0 && value <= 100 ? (int?)value : null;
        }
    }
}

