using System;

namespace GearPower
{
    public static class DeviceIdentity
    {
        public static bool SameHardware(DeviceReading a, DeviceReading b)
        {
            return a != null && b != null && !string.IsNullOrEmpty(a.HardwareId)
                && !string.IsNullOrEmpty(b.HardwareId) && a.Vid == b.Vid && a.Pid == b.Pid
                && a.IsBluetooth == b.IsBluetooth
                && string.Equals(a.HardwareId, b.HardwareId, StringComparison.OrdinalIgnoreCase);
        }

        // Legacy v1 histories have no serial/unit id. Never infer physical
        // identity from the product name alone; offer a user-confirmed merge.
        public static bool CanConfirmMerge(DeviceReading history, DeviceReading connected)
        {
            if (history == null || connected == null || !history.Disconnected || connected.Disconnected
                || history.IsDemo || connected.IsDemo || history.Vid == 0 || history.Pid == 0
                || history.Vid != connected.Vid || history.Pid != connected.Pid
                || history.IsBluetooth != connected.IsBluetooth
                || string.Equals(history.Id, connected.Id, StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.IsNullOrEmpty(history.HardwareId) && !string.IsNullOrEmpty(connected.HardwareId))
                return SameHardware(history, connected);
            string a = history.OriginalName ?? history.Name;
            string b = connected.OriginalName ?? connected.Name;
            return !string.IsNullOrWhiteSpace(a) && string.Equals(a, b, StringComparison.OrdinalIgnoreCase)
                && string.Equals(history.Transport, connected.Transport, StringComparison.OrdinalIgnoreCase)
                && string.Equals(history.Protocol, connected.Protocol, StringComparison.OrdinalIgnoreCase);
        }
    }
}
