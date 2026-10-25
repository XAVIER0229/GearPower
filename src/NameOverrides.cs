using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GearPower
{
    /// <summary>
    /// User-assigned display names, keyed by the stable device id used for
    /// history. The original name is kept on DeviceReading so clearing an
    /// override restores the vendor/product name without rescanning.
    /// </summary>
    public static class NameOverrides
    {
        private static readonly Dictionary<string, string> Names =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object Gate = new object();
        private static readonly string FilePath;
        private static bool loaded;

        static NameOverrides()
        {
            try
            {
                string root = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GearPower");
                Directory.CreateDirectory(root);
                FilePath = Path.Combine(root, "device-names.tsv");
            }
            catch (Exception ex)
            {
                FilePath = "";
                Log.Debug("Device names path failed: " + ex.Message);
            }
            Log.Debug("Device names file: " + (string.IsNullOrEmpty(FilePath) ? "(unavailable)" : FilePath));
        }

        public static string StoragePath
        {
            get { return FilePath; }
        }

        public static string Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            lock (Gate)
            {
                EnsureLoaded();
                string name;
                return Names.TryGetValue(id, out name) ? name : null;
            }
        }

        public static void Set(string id, string name)
        {
            if (string.IsNullOrEmpty(id)) return;
            name = (name ?? "").Trim();
            if (name.Length > 80) name = name.Substring(0, 80);
            lock (Gate)
            {
                EnsureLoaded();
                if (name.Length == 0) Names.Remove(id);
                else Names[id] = name;
                Save();
            }
        }

        public static void Remove(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            lock (Gate)
            {
                EnsureLoaded();
                if (Names.Remove(id)) Save();
            }
        }

        public static void Apply(IEnumerable<DeviceReading> readings)
        {
            if (readings == null) return;
            lock (Gate)
            {
                EnsureLoaded();
                foreach (DeviceReading reading in readings)
                {
                    if (reading == null || reading.IsDemo) continue;
                    if (string.IsNullOrEmpty(reading.OriginalName)) reading.OriginalName = reading.Name;
                    string custom;
                    reading.Name = Names.TryGetValue(reading.Id ?? "", out custom) && custom.Length > 0
                        ? custom
                        : reading.OriginalName;
                }
            }
        }

        private static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;
            if (string.IsNullOrEmpty(FilePath) || !File.Exists(FilePath)) return;
            try
            {
                foreach (string line in File.ReadAllLines(FilePath, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace(line) || line[0] == '#') continue;
                    string[] parts = line.Split('\t');
                    if (parts.Length < 2 || string.IsNullOrEmpty(parts[0])) continue;
                    Names[parts[0]] = Unescape(parts[1]);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Device names load failed: " + ex.Message);
            }
        }

        private static void Save()
        {
            if (string.IsNullOrEmpty(FilePath)) return;
            try
            {
                var lines = new List<string>();
                lines.Add("# GearPower device-names v1");
                foreach (KeyValuePair<string, string> pair in Names.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
                    lines.Add(pair.Key + "\t" + Escape(pair.Value));
                string temporary = FilePath + ".tmp";
                File.WriteAllLines(temporary, lines, Encoding.UTF8);
                if (File.Exists(FilePath)) File.Replace(temporary, FilePath, null);
                else File.Move(temporary, FilePath);
            }
            catch (Exception ex)
            {
                Log.Debug("Device names save failed: " + ex.Message);
            }
        }

        private static string Escape(string text)
        {
            return (text ?? "").Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "").Replace("\n", "\\n");
        }

        private static string Unescape(string text)
        {
            var sb = new StringBuilder();
            bool slash = false;
            foreach (char c in text ?? "")
            {
                if (slash)
                {
                    if (c == 't') sb.Append('\t');
                    else if (c == 'n') sb.Append('\n');
                    else sb.Append(c);
                    slash = false;
                }
                else if (c == '\\') slash = true;
                else sb.Append(c);
            }
            if (slash) sb.Append('\\');
            return sb.ToString();
        }
    }
}
