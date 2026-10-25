using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace GearPower
{
    /// <summary>
    /// Remembers every device that has ever produced a battery card on this
    /// machine, so its card can remain visible while the device is away.
    /// </summary>
    public static class DeviceHistory
    {
        private sealed class Entry
        {
            public string Id;
            public string Name;
            public string Vendor;
            public DeviceKind Kind;
            public string Transport;
            public string Protocol;
            public int? Percent;
            public bool Charging;
            public bool Full;
            public bool IsBluetooth;
            public DateTime LastSeen;
            public ushort Vid;
            public ushort Pid;
            public string Path;
        }

        private static readonly Dictionary<string, Entry> Entries =
            new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
        private static readonly object Gate = new object();
        private static readonly string FilePath;

        static DeviceHistory()
        {
            string local = "";
            string temp = "";
            try { local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); }
            catch { }
            try { temp = System.IO.Path.GetTempPath(); }
            catch { }

            string[] candidates = new string[]
            {
                string.IsNullOrEmpty(local) ? null : System.IO.Path.Combine(local, "GearPower"),
                string.IsNullOrEmpty(temp) ? null : System.IO.Path.Combine(temp, "GearPower")
            };

            foreach (string directory in candidates)
            {
                if (string.IsNullOrEmpty(directory)) continue;
                try
                {
                    Directory.CreateDirectory(directory);
                    string file = System.IO.Path.Combine(directory, "device-history.tsv");
                    string probe = file + ".probe";
                    File.WriteAllText(probe, "");
                    File.Delete(probe);
                    FilePath = file;
                    Load();
                    return;
                }
                catch (Exception ex)
                {
                    Log.Debug("Device history init failed: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Update the history from the current scan and return current devices
        /// followed by disconnected history cards.
        /// </summary>
        public static List<DeviceReading> Merge(IEnumerable<DeviceReading> current)
        {
            var connected = (current ?? new List<DeviceReading>()).Where(r => r != null).ToList();
            lock (Gate)
            {
                var connectedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (DeviceReading reading in connected)
                {
                    if (string.IsNullOrEmpty(reading.Id)) continue;
                    connectedIds.Add(reading.Id);

                    Entry entry;
                    if (!Entries.TryGetValue(reading.Id, out entry))
                    {
                        entry = new Entry();
                        entry.Id = reading.Id;
                    }

                    entry.LastSeen = DateTime.Now;
                    entry.IsBluetooth = reading.IsBluetooth;
                    if (!string.IsNullOrEmpty(reading.Name)) entry.Name = reading.Name;
                    if (!string.IsNullOrEmpty(reading.Vendor)) entry.Vendor = reading.Vendor;
                    entry.Kind = reading.Kind;
                    if (!string.IsNullOrEmpty(reading.Transport)) entry.Transport = reading.Transport;
                    if (!string.IsNullOrEmpty(reading.Protocol)) entry.Protocol = reading.Protocol;
                    if (reading.Percent.HasValue)
                    {
                        entry.Percent = reading.Percent;
                        entry.Charging = reading.Charging;
                        entry.Full = reading.Full;
                    }
                    entry.Vid = reading.Vid;
                    entry.Pid = reading.Pid;
                    if (!string.IsNullOrEmpty(reading.Path)) entry.Path = reading.Path;
                    Entries[reading.Id] = entry;
                }

                Save();
                var result = new List<DeviceReading>(connected);
                foreach (Entry entry in Entries.Values.OrderByDescending(e => e.LastSeen))
                {
                    if (connectedIds.Contains(entry.Id)) continue;
                    result.Add(ToReading(entry));
                }
                return result;
            }
        }

        public static void Remove(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            lock (Gate)
            {
                if (!Entries.Remove(id)) return;
                Save();
            }
        }

        private static DeviceReading ToReading(Entry entry)
        {
            var reading = new DeviceReading();
            reading.Id = entry.Id;
            reading.Name = entry.Name;
            reading.OriginalName = entry.Name;
            reading.Vendor = entry.Vendor;
            reading.Kind = entry.Kind;
            reading.Transport = entry.Transport;
            reading.Protocol = entry.Protocol;
            reading.Percent = entry.Percent;
            reading.Charging = entry.Charging;
            reading.Full = entry.Full;
            reading.Asleep = false;
            reading.FromCache = true;
            reading.IsBluetooth = entry.IsBluetooth;
            reading.Disconnected = true;
            reading.Updated = entry.LastSeen;
            reading.Vid = entry.Vid;
            reading.Pid = entry.Pid;
            reading.Path = entry.Path;
            reading.Note = "历史设备，最后连接 " + entry.LastSeen.ToString("yyyy-MM-dd HH:mm");
            return reading;
        }

        private static void Save()
        {
            if (string.IsNullOrEmpty(FilePath)) return;
            try
            {
                var lines = new List<string>();
                lines.Add("# GearPower device-history v1");
                foreach (Entry entry in Entries.Values.OrderByDescending(e => e.LastSeen))
                {
                    lines.Add(Escape(entry.Id) + "\t" +
                              Escape(entry.Name) + "\t" +
                              Escape(entry.Vendor) + "\t" +
                              ((int)entry.Kind).ToString(CultureInfo.InvariantCulture) + "\t" +
                              Escape(entry.Transport) + "\t" +
                              Escape(entry.Protocol) + "\t" +
                              (entry.Percent.HasValue ? entry.Percent.Value.ToString(CultureInfo.InvariantCulture) : "") + "\t" +
                              (entry.Charging ? "1" : "0") + "\t" +
                              (entry.Full ? "1" : "0") + "\t" +
                              entry.LastSeen.ToBinary().ToString(CultureInfo.InvariantCulture) + "\t" +
                              entry.Vid.ToString(CultureInfo.InvariantCulture) + "\t" +
                              entry.Pid.ToString(CultureInfo.InvariantCulture) + "\t" +
                              Escape(entry.Path) + "\t" +
                              (entry.IsBluetooth ? "1" : "0"));
                }
                File.WriteAllLines(FilePath, lines, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Log.Debug("Device history save failed: " + ex.Message);
            }
        }

        private static void Load()
        {
            if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath)) return;
            try
            {
                foreach (string line in File.ReadAllLines(FilePath, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line) || line[0] == '#') continue;
                    string[] parts = line.Split('\t');
                    if (parts.Length < 13) continue;

                    var entry = new Entry();
                    entry.Id = parts[0];
                    if (string.IsNullOrEmpty(entry.Id)) continue;
                    entry.Name = parts[1];
                    entry.Vendor = parts[2];
                    int kind;
                    if (!int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out kind)) continue;
                    entry.Kind = (DeviceKind)kind;
                    entry.Transport = parts[4];
                    entry.Protocol = parts[5];
                    int percent;
                    if (int.TryParse(parts[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out percent))
                    {
                        if (percent >= 0 && percent <= 100) entry.Percent = percent;
                    }
                    entry.Charging = parts[7] == "1";
                    entry.Full = parts[8] == "1";
                    long ticks;
                    if (!long.TryParse(parts[9], NumberStyles.Integer, CultureInfo.InvariantCulture, out ticks)) continue;
                    try { entry.LastSeen = DateTime.FromBinary(ticks); }
                    catch { continue; }
                    ushort vid;
                    ushort pid;
                    ushort.TryParse(parts[10], NumberStyles.Integer, CultureInfo.InvariantCulture, out vid);
                    ushort.TryParse(parts[11], NumberStyles.Integer, CultureInfo.InvariantCulture, out pid);
                    entry.Vid = vid;
                    entry.Pid = pid;
                    entry.Path = parts[12];
                    entry.IsBluetooth = parts.Length > 13 && parts[13] == "1";
                    Entries[entry.Id] = entry;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Device history load failed: " + ex.Message);
            }
        }

        private static string Escape(string text)
        {
            return (text ?? "").Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ');
        }
    }
}
