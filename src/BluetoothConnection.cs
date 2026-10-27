using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GearPower
{
    // PnP presence/battery values survive disconnect. Query the Classic
    // Bluetooth connection flag independently, without discovery or connecting.
    public static class BluetoothConnection
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Search
        {
            public uint Size;
            public int Authenticated, Remembered, Unknown, Connected, Inquiry;
            public byte Timeout;
            public IntPtr Radio;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SystemTime
        {
            public ushort Year, Month, DayOfWeek, Day, Hour, Minute, Second, Milliseconds;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct Device
        {
            public uint Size;
            public ulong Address;
            public uint Class;
            public int Connected, Remembered, Authenticated;
            public SystemTime LastSeen, LastUsed;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 248)] public string Name;
        }

        [DllImport("BluetoothApis.dll", SetLastError = true)]
        private static extern IntPtr BluetoothFindFirstDevice(ref Search search, ref Device device);
        [DllImport("BluetoothApis.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BluetoothFindNextDevice(IntPtr find, ref Device device);
        [DllImport("BluetoothApis.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool BluetoothFindDeviceClose(IntPtr find);

        public static Dictionary<ulong, bool> Snapshot()
        {
            var devices = new Dictionary<ulong, bool>();
            var search = new Search { Size = (uint)Marshal.SizeOf(typeof(Search)),
                Authenticated = 1, Remembered = 1, Connected = 1, Inquiry = 0 };
            var device = new Device { Size = (uint)Marshal.SizeOf(typeof(Device)) };
            IntPtr find = IntPtr.Zero;
            try
            {
                find = BluetoothFindFirstDevice(ref search, ref device);
                if (find == IntPtr.Zero) return devices;
                do
                {
                    // BOOL is any nonzero value (not necessarily 1).
                    bool previous;
                    devices.TryGetValue(device.Address, out previous);
                    devices[device.Address] = previous || device.Connected != 0;
                    device.Size = (uint)Marshal.SizeOf(typeof(Device));
                } while (BluetoothFindNextDevice(find, ref device));
            }
            catch (Exception ex) { Log.Debug("Bluetooth connection state unavailable: " + ex.Message); }
            finally { if (find != IntPtr.Zero) BluetoothFindDeviceClose(find); }
            return devices;
        }

        public static bool TryParseAddress(string instance, out ulong address)
        {
            address = 0;
            const string prefix = "BTHENUM\\DEV_";
            if (string.IsNullOrEmpty(instance) || !instance.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                || instance.Length < prefix.Length + 12) return false;
            string hex = instance.Substring(prefix.Length, 12);
            if (instance.Length > prefix.Length + 12 && instance[prefix.Length + 12] != '\\') return false;
            return ulong.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out address);
        }

        public static bool? State(string container, IDictionary<string, ulong> addresses, IDictionary<ulong, bool> snapshot)
        {
            ulong address;
            bool connected;
            if (string.IsNullOrEmpty(container) || !addresses.TryGetValue(container, out address)
                || !snapshot.TryGetValue(address, out connected)) return null;
            return connected;
        }

        public static bool CanUseCachedBattery(bool classicBluetooth, bool? connected)
        {
            // The Win32 API does not enumerate BLE. Preserve its existing
            // property path; Classic requires positive connection evidence.
            return classicBluetooth ? connected == true : connected != false;
        }
    }
}
