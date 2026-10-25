using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace GearPower
{
    public sealed class InventoryRow
    {
        public string Name;
        public string Kind;
        public string VidPid;
        public string Usage;
        public string Interface;
        public string Reports;
        public string Status;
        public string Path;
        public string Detail;
        public bool IsSupported;
        public string GroupId;
        public string KindId;
        public string Vendor;
        public string Product;
        public ushort Vid;
        public ushort Pid;
        public ushort UsagePage;
        public ushort UsageValue;
        public int InputLength;
        public int OutputLength;
        public int FeatureLength;
    }

    public sealed class ScanResult
    {
        public List<DeviceReading> Readings = new List<DeviceReading>();
        public List<InventoryRow> Inventory = new List<InventoryRow>();
        public string Error;
        public double Seconds;
        public int HidCount;
        public int BluetoothCount;
    }

    public static class Scanner
    {
        public static ScanResult Scan()
        {
            var watch = Stopwatch.StartNew();
            var result = new ScanResult();
            ProfileRunner.Reload();
            bool includeBluetooth = DisplaySettings.BluetoothEnabled;

            List<HidInterfaceInfo> interfaces;
            try
            {
                interfaces = HidEnumerator.Enumerate();
            }
            catch (Exception ex)
            {
                Log.Error("HID enumeration failed: " + ex);
                result.Error = "无法枚举 HID 设备：" + ex.Message;
                return result;
            }

            result.HidCount = interfaces.Count;
            Log.Info("Scan start: " + interfaces.Count + " HID interfaces in " +
                watch.Elapsed.TotalSeconds.ToString("0.00", CultureInfo.InvariantCulture) + "s");

            var groups = new Dictionary<string, DeviceGroup>(StringComparer.OrdinalIgnoreCase);
            var groupOrder = new List<string>();
            foreach (HidInterfaceInfo iface in interfaces)
            {
                string key = iface.PhysicalKey;
                DeviceGroup group;
                if (!groups.TryGetValue(key, out group))
                {
                    group = new DeviceGroup();
                    group.Key = key;
                    groups[key] = group;
                    groupOrder.Add(key);
                }
                group.Interfaces.Add(iface);
            }

            foreach (string key in groupOrder)
            {
                DeviceGroup group = groups[key];
                PrepareGroup(group);
                if (!includeBluetooth && group.Bluetooth) continue;
                ProbeResult probe = null;
                try
                {
                    Log.Debug("Probe " + group.Name + " " + group.Vid.ToString("X4") + ":" + group.Pid.ToString("X4")
                        + " (" + group.Interfaces.Count + " collections)");
                    probe = ProtocolRegistry.Probe(group);
                }
                catch (Exception ex)
                {
                    Log.Error("Probe " + group.Name + " failed: " + ex);
                }

                if (probe != null)
                {
                    if (probe.Kind.HasValue) group.Kind = probe.Kind.Value;
                    DeviceReading reading = ToReading(group, probe);
                    if (reading != null)
                    {
                        result.Readings.Add(reading);
                        Log.Info(string.Format(CultureInfo.InvariantCulture,
                            "{0} ({1:X4}:{2:X4}) -> {3}% via {4}",
                            reading.Name, reading.Vid, reading.Pid,
                            reading.Percent.HasValue ? reading.Percent.Value.ToString() : "?",
                            reading.Protocol));
                    }
                }
                Log.Debug("Probe done " + group.Name);
            }

            // Bluetooth devices that expose a Windows battery property.
            var hidContainers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DeviceReading reading in result.Readings)
            {
                // An enumerated HID interface does not imply a successful battery read.
                if (reading.Percent.HasValue && reading.Id.StartsWith("container:", StringComparison.OrdinalIgnoreCase))
                    hidContainers.Add(reading.Id.Substring("container:".Length));
            }
            if (includeBluetooth)
            try
            {
                List<BluetoothBatteryInfo> bt = HidEnumerator.EnumerateBluetoothBattery();
                result.BluetoothCount = bt.Count;
                foreach (BluetoothBatteryInfo info in bt)
                {
                    if (!string.IsNullOrEmpty(info.ContainerId) && hidContainers.Contains(info.ContainerId)) continue;
                    var reading = new DeviceReading();
                    reading.Id = "bt:" + (string.IsNullOrEmpty(info.ContainerId) ? info.InstanceId : info.ContainerId);
                    reading.Name = info.Name;
                    reading.OriginalName = info.Name;
                    reading.Vendor = "蓝牙";
                    reading.Kind = info.Kind;
                    reading.Transport = "蓝牙";
                    reading.Protocol = "Windows 蓝牙电量";
                    reading.Percent = info.Percent;
                    reading.Updated = DateTime.Now;
                    reading.Note = "系统蓝牙电量属性";
                    reading.IsBluetooth = true;
                    // Replace an identified HID device without a level with the system reading.
                    if (!string.IsNullOrEmpty(info.ContainerId))
                    {
                        DeviceReading pending = result.Readings.FirstOrDefault(r =>
                            string.Equals(r.Id, "container:" + info.ContainerId, StringComparison.OrdinalIgnoreCase));
                        if (pending != null)
                        {
                            pending.Percent = info.Percent;
                            pending.Protocol = reading.Protocol;
                            pending.Note = reading.Note;
                            pending.Transport = "蓝牙";
                            continue;
                        }
                    }
                    result.Readings.Add(reading);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Bluetooth battery enumeration failed: " + ex);
            }

            result.Readings.Sort(CompareReadings);
            BuildInventory(interfaces, groups, result);

            result.Seconds = watch.Elapsed.TotalSeconds;
            Log.Info("Scan done in " + result.Seconds.ToString("0.00", CultureInfo.InvariantCulture) + "s, " + result.Readings.Count + " devices");
            return result;
        }

        private static int CompareReadings(DeviceReading a, DeviceReading b)
        {
            int left = a.Asleep ? 200 : (a.Percent.HasValue ? a.Percent.Value : 150);
            int right = b.Asleep ? 200 : (b.Percent.HasValue ? b.Percent.Value : 150);
            int cmp = left.CompareTo(right);
            if (cmp != 0) return cmp;
            return string.Compare(a.Name, b.Name, StringComparison.CurrentCultureIgnoreCase);
        }

        private static void PrepareGroup(DeviceGroup group)
        {
            group.Interfaces.Sort((a, b) =>
            {
                int av = a.Vid == 0 ? 1 : 0;
                int bv = b.Vid == 0 ? 1 : 0;
                return av.CompareTo(bv);
            });

            HidInterfaceInfo primary = group.Interfaces[0];
            group.Bluetooth = group.Interfaces.Any(i =>
                (i.Path ?? "").IndexOf("{00001124-0000-1000-8000-00805f9b34fb}", StringComparison.OrdinalIgnoreCase) >= 0
                || (i.InstanceId ?? "").StartsWith("BTH", StringComparison.OrdinalIgnoreCase));
            string product = group.Interfaces
                .Select(i => L10n.CleanProduct(i.Product))
                .Where(s => !string.IsNullOrEmpty(s))
                .OrderByDescending(s => L10n.IsGenericName(s) ? 0 : 1000 + s.Length)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(product)) product = primary.FriendlyName;
            group.Product = product ?? "";
            group.Vendor = L10n.VendorName(primary.Vid);
            group.Name = L10n.DisplayName(primary.Vid, primary.Pid, group.Product);
            group.Known = KnownDevices.Find(primary.Vid, primary.Pid);

            DeviceKind kind = DeviceKind.Other;
            if (group.Known != null) kind = group.Known.Kind;
            else kind = L10n.KindFromName(group.Name + " " + group.Product);
            if (kind == DeviceKind.Other)
            {
                foreach (HidInterfaceInfo iface in group.Interfaces)
                {
                    if (iface.UsagePage == 0x01 && iface.Usage == 0x02) { kind = DeviceKind.Mouse; break; }
                    if (iface.UsagePage == 0x01 && iface.Usage == 0x06) { kind = DeviceKind.Keyboard; break; }
                    if (iface.UsagePage == 0x01 && iface.Usage == 0x05) { kind = DeviceKind.Controller; break; }
                    if (iface.UsagePage == 0x0C && iface.Usage == 0x01) { kind = DeviceKind.Headset; break; }
                }
            }
            group.Kind = kind;
        }

        private static DeviceReading ToReading(DeviceGroup group, ProbeResult probe)
        {
            if (!probe.Percent.HasValue && !probe.Charging && !probe.Full && !probe.Asleep && !probe.DeviceDetected) return null;

            var reading = new DeviceReading();
            reading.Id = group.Key;
            reading.Name = string.IsNullOrEmpty(probe.ModelName) ? group.Name : probe.ModelName;
            reading.OriginalName = reading.Name;
            reading.Vendor = group.Vendor;
            reading.Kind = group.Kind;
            reading.Transport = L10n.Transport(group.Vid, group.Pid, group.Product, group.Bluetooth);
            reading.IsBluetooth = group.Bluetooth;
            reading.Protocol = probe.Protocol;
            reading.Percent = probe.Percent;
            reading.Charging = probe.Charging;
            reading.Full = probe.Full;
            reading.Asleep = probe.Asleep;
            reading.VoltageMv = probe.VoltageMv;
            reading.Note = probe.Note;
            reading.Updated = DateTime.Now;
            reading.Vid = group.Vid;
            reading.Pid = group.Pid;
            reading.Path = group.Interfaces.Count > 0 ? group.Interfaces[0].Path : "";
            return reading;
        }

        private static void BuildInventory(List<HidInterfaceInfo> interfaces, Dictionary<string, DeviceGroup> groups, ScanResult result)
        {
            var readingByGroup = new Dictionary<string, DeviceReading>(StringComparer.OrdinalIgnoreCase);
            foreach (DeviceReading reading in result.Readings)
            {
                if (!reading.IsBluetooth) readingByGroup[reading.Id] = reading;
            }

            foreach (HidInterfaceInfo iface in interfaces)
            {
                var row = new InventoryRow();
                string key = iface.PhysicalKey;
                DeviceGroup group;
                groups.TryGetValue(key, out group);
                DeviceReading reading;
                readingByGroup.TryGetValue(key, out reading);

                row.Name = group != null ? group.Name : L10n.DisplayName(iface.Vid, iface.Pid, iface.Product);
                if (iface.InterfaceNumber >= 0) row.Name += "  ·  IF" + iface.InterfaceNumber;
                row.Kind = KindNames.Label(group != null ? group.Kind : L10n.KindFromName(iface.Product));
                row.KindId = InventoryGroups.KindId(group != null ? group.Kind : L10n.KindFromName(iface.Product));
                row.GroupId = key;
                row.Vid = iface.Vid;
                row.Pid = iface.Pid;
                row.UsagePage = iface.UsagePage;
                row.UsageValue = iface.Usage;
                row.InputLength = iface.InputLength;
                row.OutputLength = iface.OutputLength;
                row.FeatureLength = iface.FeatureLength;
                row.Vendor = group != null ? group.Vendor : L10n.VendorName(iface.Vid);
                row.Product = iface.Product;
                row.VidPid = iface.VidPid;
                row.Usage = iface.UsageText;
                row.Interface = iface.InterfaceNumber >= 0 ? iface.InterfaceNumber.ToString(CultureInfo.InvariantCulture) : "—";
                row.Reports = iface.ReportsText;
                row.Path = iface.Path;
                row.IsSupported = reading != null && (reading.Percent.HasValue || reading.Charging || reading.Full || reading.Asleep);

                if (reading != null)
                {
                    if (reading.Asleep)
                    {
                        row.Status = reading.Percent.HasValue
                            ? "设备休眠 · 上次 " + reading.Percent.Value + "%"
                            : "设备休眠";
                    }
                    else if (reading.Percent.HasValue) row.Status = "电量 " + reading.Percent.Value + "%";
                    else if (reading.Charging) row.Status = "充电中";
                    else row.Status = "已识别 · 未读到电量";
                    row.Status += " · " + reading.Protocol;
                }
                else if (iface.FindBatteryValueCap() != null)
                {
                    row.Status = "含标准电量字段（未响应）";
                }
                else
                {
                    row.Status = "—";
                }

                var detail = new StringBuilder();
                detail.AppendLine(row.Name);
                detail.AppendLine("VID:PID  " + iface.VidPid);
                detail.AppendLine("用途页  " + iface.UsageText);
                detail.AppendLine("报告    " + iface.ReportsText);
                detail.AppendLine("传输    " + L10n.Transport(iface.Vid, iface.Pid, group != null ? group.Product : iface.Product, group != null && group.Bluetooth));
                if (reading != null && !string.IsNullOrEmpty(reading.Note)) detail.AppendLine("诊断    " + reading.Note);
                detail.AppendLine("接口号  " + row.Interface);
                detail.AppendLine("路径    " + iface.Path);
                if (!string.IsNullOrEmpty(iface.InstanceId)) detail.AppendLine("实例    " + iface.InstanceId);
                if (!string.IsNullOrEmpty(iface.ContainerId)) detail.AppendLine("容器    " + iface.ContainerId);
                if (!string.IsNullOrEmpty(iface.OpenError)) detail.AppendLine("打开    " + iface.OpenError);
                detail.AppendLine();
                detail.AppendLine("字段：");
                if (iface.ValueCaps.Count == 0)
                {
                    detail.AppendLine("(无)");
                }
                else
                {
                    foreach (HidValueCap cap in iface.ValueCaps)
                    {
                        detail.AppendLine("  " + cap.Describe());
                    }
                }
                row.Detail = detail.ToString();
                result.Inventory.Add(row);
            }

            foreach (DeviceReading reading in result.Readings)
            {
                if (!reading.IsBluetooth) continue;
                var row = new InventoryRow();
                row.Name = reading.Name;
                row.Kind = KindNames.Label(reading.Kind);
                row.KindId = InventoryGroups.KindId(reading.Kind);
                row.GroupId = "bt:" + reading.Id;
                row.VidPid = "蓝牙";
                row.Usage = "BLE / 经典蓝牙";
                row.Interface = "—";
                row.Reports = "—";
                row.Status = "电量 " + reading.Percent + "% · " + reading.Protocol;
                row.IsSupported = true;
                row.Path = reading.Id;
                row.Detail = reading.Name + Environment.NewLine + "蓝牙设备" + Environment.NewLine + reading.Note;
                result.Inventory.Add(row);
            }
        }

        public static string HexDump(byte[] data)
        {
            if (data == null || data.Length == 0) return "(无)";
            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i += 16)
            {
                sb.Append(i.ToString("X4", CultureInfo.InvariantCulture)).Append("  ");
                for (int j = 0; j < 16; j++)
                {
                    if (i + j < data.Length) sb.Append(data[i + j].ToString("X2", CultureInfo.InvariantCulture)).Append(' ');
                    else sb.Append("   ");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
