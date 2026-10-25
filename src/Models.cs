using System;
using System.Collections.Generic;
using System.Globalization;

namespace GearPower
{
    public enum DeviceKind
    {
        Mouse,
        Keyboard,
        Headset,
        Controller,
        Speaker,
        Microphone,
        Dock,
        Other
    }

    public enum ChargeState
    {
        Unknown,
        Discharging,
        Charging,
        Full
    }

    public sealed class DeviceReading
    {
        public string Id;
        public string Name;
        public string OriginalName;
        public string Vendor;
        public DeviceKind Kind;
        public string Transport;
        public string Protocol;
        public int? Percent;
        public bool Charging;
        public bool Full;
        public bool Asleep;
        public int? VoltageMv;
        public string Note;
        public DateTime Updated;
        public ushort Vid;
        public ushort Pid;
        public string Path;
        public bool IsBluetooth;
        public bool IsDemo;
        public bool FromCache;
        public bool Disconnected;

        public bool HasLevel
        {
            get { return Percent.HasValue; }
        }

        public ChargeState State
        {
            get
            {
                if (Disconnected) return ChargeState.Unknown;
                if (Charging) return ChargeState.Charging;
                if (Full) return ChargeState.Full;
                if (Asleep) return ChargeState.Unknown;
                if (Percent.HasValue) return ChargeState.Discharging;
                return ChargeState.Unknown;
            }
        }

        public string KindText
        {
            get { return KindNames.Label(Kind); }
        }
    }

    public sealed class ProbeResult
    {
        public bool DeviceDetected;
        public int? Percent;
        public bool Charging;
        public bool Full;
        public bool Asleep;
        public int? VoltageMv;
        public string Protocol;
        public string Note;
        public string ModelName;
        public DeviceKind? Kind;
    }

    public sealed class KnownModel
    {
        public string Name;
        public DeviceKind Kind;
        public bool Wireless;

        public KnownModel(string name, DeviceKind kind, bool wireless)
        {
            Name = name;
            Kind = kind;
            Wireless = wireless;
        }
    }

    public static class KindNames
    {
        public static string Label(DeviceKind kind)
        {
            switch (kind)
            {
                case DeviceKind.Mouse: return "鼠标";
                case DeviceKind.Keyboard: return "键盘";
                case DeviceKind.Headset: return "耳机";
                case DeviceKind.Controller: return "手柄";
                case DeviceKind.Speaker: return "音箱";
                case DeviceKind.Microphone: return "麦克风";
                case DeviceKind.Dock: return "拓展坞";
                default: return "设备";
            }
        }

        public static string Glyph(DeviceKind kind)
        {
            switch (kind)
            {
                case DeviceKind.Mouse: return "\uE962";
                case DeviceKind.Keyboard: return "\uE765";
                case DeviceKind.Headset: return "\uE7F6";
                case DeviceKind.Controller: return "\uE7FC";
                case DeviceKind.Speaker: return "\uE7F6";
                case DeviceKind.Microphone: return "\uE720";
                case DeviceKind.Dock: return "\uE8B7";
                default: return "\uE83F";
            }
        }
    }

    public static class KnownDevices
    {
        private static readonly Dictionary<string, KnownModel> Map = Build();

        private static Dictionary<string, KnownModel> Build()
        {
            var m = new Dictionary<string, KnownModel>(StringComparer.OrdinalIgnoreCase);

            Add(m, 0x373B, 0x1031, "ATK F1 Ultimate", DeviceKind.Mouse, true);
            Add(m, 0x373B, 0x102E, "ATK F1 Ultimate", DeviceKind.Mouse, false);
            Add(m, 0x373B, 0x11D9, "ATK A9 Ultimate", DeviceKind.Mouse, true);
            Add(m, 0x373B, 0x11B6, "ATK A9 Ultimate", DeviceKind.Mouse, false);
            Add(m, 0x373B, 0x1155, "ATK Zero", DeviceKind.Mouse, true);
            Add(m, 0x373B, 0x124F, "ATK Zero", DeviceKind.Mouse, true);
            Add(m, 0x373B, 0x1154, "ATK Zero", DeviceKind.Mouse, false);
            Add(m, 0x373B, 0x120C, "ATK Horizon", DeviceKind.Headset, false);

            Add(m, 0x3554, 0xF58A, "VXE R1 Pro Max", DeviceKind.Mouse, true);
            Add(m, 0x3554, 0xF58C, "VXE R1 Pro Max", DeviceKind.Mouse, false);
            Add(m, 0x3554, 0xF58E, "VXE R1 SE+", DeviceKind.Mouse, true);
            Add(m, 0x3554, 0xF58F, "VXE R1 SE+", DeviceKind.Mouse, false);
            Add(m, 0x3554, 0xF503, "VGN F1 Pro", DeviceKind.Mouse, true);
            Add(m, 0x3554, 0xF502, "VGN F1 Pro", DeviceKind.Mouse, false);
            Add(m, 0x3554, 0xFB3E, "VGN F2 Pro Max", DeviceKind.Mouse, true);
            Add(m, 0x3554, 0xFB3D, "VGN F2 Pro Max", DeviceKind.Mouse, false);
            Add(m, 0x3554, 0xF524, "Zaopin Z2 Mini", DeviceKind.Mouse, true);
            Add(m, 0x3554, 0xF526, "Zaopin Z2 Mini", DeviceKind.Mouse, false);
            Add(m, 0x3554, 0xF5F7, "Scyrox V8", DeviceKind.Mouse, true);
            Add(m, 0x3554, 0xF5F6, "Scyrox V8", DeviceKind.Mouse, false);
            Add(m, 0x3554, 0xFA09, "LEOBOG Hi75c Pro", DeviceKind.Keyboard, true);
            Add(m, 0x37D7, 0x2401, "Flydigi Vader 5 Pro", DeviceKind.Controller, true);

            Add(m, 0x260D, 0x1175, "Dareu A950 Air", DeviceKind.Mouse, true);
            Add(m, 0x260D, 0x1193, "Dareu A950 Air", DeviceKind.Mouse, false);
            Add(m, 0x260D, 0x1154, "Dareu A950 Wing", DeviceKind.Mouse, true);
            Add(m, 0x260D, 0x1196, "Dareu A950 Wing", DeviceKind.Mouse, false);
            Add(m, 0x33E4, 0x3854, "G-Wolves Lycan", DeviceKind.Mouse, true);
            Add(m, 0x33E4, 0x4719, "G-Wolves Lycan", DeviceKind.Mouse, false);

            Add(m, 0x373E, 0x001E, "Lamzu Maya X", DeviceKind.Mouse, true);
            Add(m, 0x373E, 0x001C, "Lamzu Maya X", DeviceKind.Mouse, false);
            Add(m, 0x37B0, 0x0010, "Lamzu Inca", DeviceKind.Mouse, true);
            Add(m, 0x37B0, 0x0009, "Lamzu Inca", DeviceKind.Mouse, false);
            Add(m, 0x37B0, 0x0040, "Lamzu Thorn V2", DeviceKind.Mouse, true);

            Add(m, 0x1D57, 0xFA60, "Attack Shark X3", DeviceKind.Mouse, true);
            Add(m, 0x1D57, 0xFA61, "Attack Shark X3", DeviceKind.Mouse, false);
            Add(m, 0x5253, 0x1020, "MCHOSE L7 Pro", DeviceKind.Mouse, true);
            Add(m, 0x5253, 0x00B0, "MCHOSE L7 Pro", DeviceKind.Mouse, false);
            Add(m, 0x3837, 0x1001, "MCHOSE K7 Ultra", DeviceKind.Mouse, true);
            Add(m, 0x3837, 0x4150, "MCHOSE K7 Ultra", DeviceKind.Mouse, false);
            Add(m, 0x1956, 0x3069, "Bloody SG5 Pro", DeviceKind.Mouse, true);
            Add(m, 0x1956, 0x3144, "Bloody SG5 Pro", DeviceKind.Mouse, false);

            Add(m, 0x03F0, 0x098D, "HyperX Cloud Alpha Wireless", DeviceKind.Headset, true);
            Add(m, 0x03F0, 0x05B7, "HyperX Cloud III Wireless", DeviceKind.Headset, true);
            Add(m, 0x03F0, 0x0E90, "HyperX Cloud Flight Wireless", DeviceKind.Headset, true);
            Add(m, 0x03F0, 0x1749, "HyperX Cloud Flight Wireless", DeviceKind.Headset, true);
            Add(m, 0x03F0, 0x16C4, "HyperX Cloud Flight Wireless", DeviceKind.Headset, true);
            Add(m, 0x0951, 0x1723, "HyperX Cloud Flight Wireless", DeviceKind.Headset, true);
            Add(m, 0x0951, 0x1718, "HyperX Cloud Flight S", DeviceKind.Headset, true);

            Add(m, 0x1532, 0x00A6, "Razer Viper V2 Pro", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x00A5, "Razer Viper V2 Pro", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x00C1, "Razer Viper V3 Pro", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x00C0, "Razer Viper V3 Pro", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x00B7, "Razer DeathAdder V3 Pro", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x00B6, "Razer DeathAdder V3 Pro", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x00BF, "Razer DeathAdder V4 Pro", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x00BE, "Razer DeathAdder V4 Pro", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x00AB, "Razer Basilisk V3 Pro", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x00AA, "Razer Basilisk V3 Pro", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x00CD, "Razer Basilisk V3 Pro 35K", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x00CC, "Razer Basilisk V3 Pro 35K", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x0088, "Razer Basilisk Ultimate", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x0086, "Razer Basilisk Ultimate", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x00B0, "Razer Cobra Pro", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x00AF, "Razer Cobra Pro", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x0090, "Razer Naga Pro", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x008F, "Razer Naga Pro", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x00A8, "Razer Naga V2 Pro", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x00A7, "Razer Naga V2 Pro", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x006F, "Razer Lancehead Wireless", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x0070, "Razer Lancehead Wireless", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x00D1, "Razer Pro Click V2", DeviceKind.Mouse, true);
            Add(m, 0x1532, 0x00D0, "Razer Pro Click V2", DeviceKind.Mouse, false);
            Add(m, 0x1532, 0x0555, "Razer BlackShark V2 Pro 2.4", DeviceKind.Headset, true);
            Add(m, 0x1532, 0x056A, "Razer Seiren V3 Mini", DeviceKind.Microphone, false);
            Add(m, 0x046D, 0xC547, "Logitech PRO X Wireless", DeviceKind.Headset, true);
            Add(m, 0x0B05, 0x193E, "ASUS ROG Falchion", DeviceKind.Keyboard, false);
            Add(m, 0x0B05, 0x1B84, "ASUS ROG PELTA (2.4GHz)", DeviceKind.Headset, true);
            Add(m, 0xCAFE, 0x910B, "NINX NX60 ANSI", DeviceKind.Keyboard, false);
            Add(m, 0xC019, 0x0401, "HM HUB", DeviceKind.Dock, false);
            Add(m, 0xC019, 0xB300, "HM HubX", DeviceKind.Dock, false);

            return m;
        }

        private static void Add(Dictionary<string, KnownModel> m, int vid, int pid, string name, DeviceKind kind, bool wireless)
        {
            m[Key(vid, pid)] = new KnownModel(name, kind, wireless);
        }

        private static string Key(int vid, int pid)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:X4}:{1:X4}", vid, pid);
        }

        public static KnownModel Find(ushort vid, ushort pid)
        {
            KnownModel model;
            if (Map.TryGetValue(Key(vid, pid), out model)) return model;
            return null;
        }
    }

    public static class L10n
    {
        public static string VendorName(ushort vid)
        {
            switch (vid)
            {
                case 0x004C: return "Apple 苹果";
                case 0x05AC: return "Apple 苹果";
                case 0x373B: return "ATK";
                case 0x3554: return "VXE / VGN";
                case 0x260D: return "达尔优";
                case 0x33E4: return "G-Wolves";
                case 0x373E: return "兰族 Lamzu";
                case 0x37B0: return "兰族 Lamzu";
                case 0x1532: return "Razer 雷蛇";
                case 0x0B05: return "ASUS ROG";
                case 0x03F0: return "HyperX";
                case 0x0951: return "HyperX";
                case 0x046D: return "Logitech 罗技";
                case 0x1D57: return "Attack Shark";
                case 0x5253: return "MCHOSE";
                case 0x3837: return "MCHOSE";
                case 0x291D: return "MCHOSE";
                case 0xC019: return "HM";
                case 0xCAFE: return "NINX";
                case 0x1956: return "A4Tech Bloody";
                default: return "未知厂商";
            }
        }

        public static string CleanProduct(string product)
        {
            if (string.IsNullOrEmpty(product)) return "";
            string p = product.Trim();
            if (p.StartsWith("HP, Inc ", StringComparison.OrdinalIgnoreCase)) p = p.Substring(8).Trim();
            if (p.StartsWith("HP ", StringComparison.OrdinalIgnoreCase) && p.IndexOf("HyperX", StringComparison.OrdinalIgnoreCase) > 0)
                p = p.Substring(p.IndexOf("HyperX", StringComparison.OrdinalIgnoreCase)).Trim();
            return p;
        }

        public static bool IsGenericName(string name)
        {
            if (string.IsNullOrEmpty(name)) return true;
            string n = name.ToLowerInvariant();
            string[] generic = new string[]
            {
                "usb receiver", "usb receiver ", "2.4g wireless receiver", "2.4g receiver",
                "wireless receiver", "usb dongle", "receiver", "compx", "usb composite device",
                "hid-compliant mouse", "hid-compliant keyboard", "hid-compliant vendor-defined device",
                "hid-compliant system controller", "hid-compliant consumer control device",
                "usb 输入设备", "usb input device", "usb 复合设备", "无线接收器", "接收器"
            };
            foreach (string g in generic)
            {
                if (n == g || n.StartsWith(g + " ") || n.StartsWith(g + "(")) return true;
            }
            return false;
        }

        public static DeviceKind KindFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return DeviceKind.Other;
            string n = name.ToLowerInvariant();
            if (n.Contains("microphone") || n.Contains("麦克风") || n.Contains("seiren"))
                return DeviceKind.Microphone;
            if (n.Contains("dock") || n.Contains("docking") || n.Contains("拓展坞") || n.Contains("扩展坞")
                || n.Contains("hm hub") || n.Contains("hubx") || n.Contains("hub"))
                return DeviceKind.Dock;
            if (n.Contains("headset") || n.Contains("headphone") || n.Contains("earbud") || n.Contains("earphone")
                || n.Contains("cloud alpha") || n.Contains("cloud iii") || n.Contains("cloud ii")
                || n.Contains("delta") || n.Contains("cetra") || n.Contains("pelta") || n.Contains("clavis")
                || n.Contains("blackshark") || n.Contains("kraken") || n.Contains("barracuda"))
                return DeviceKind.Headset;
            if (n.Contains("keyboard") || n.Contains("keychron") || n.Contains("键盘")
                || n.Contains("nx60") || n.Contains("ninx")) return DeviceKind.Keyboard;
            if (n.Contains("mouse") || n.Contains("鼠标") || n.Contains("mice")) return DeviceKind.Mouse;
            if (n.Contains("gamepad") || n.Contains("controller") || n.Contains("手柄")) return DeviceKind.Controller;
            if (n.Contains("speaker") || n.Contains("soundbar") || n.Contains("音箱")) return DeviceKind.Speaker;
            return DeviceKind.Other;
        }

        public static string DisplayName(ushort vid, ushort pid, string product)
        {
            string clean = CleanProduct(product);
            KnownModel known = KnownDevices.Find(vid, pid);
            if (known != null)
            {
                string lower = clean.ToLowerInvariant();
                if (IsGenericName(clean) || clean.Length < 3 || lower.Contains("dongle") || lower.Contains("receiver") || lower.Contains("8k"))
                    return known.Name;
            }
            if (!string.IsNullOrEmpty(clean) && !IsGenericName(clean)) return clean;
            if (known != null) return known.Name;
            string vendor = VendorName(vid);
            return vendor + " 无线设备";
        }

        public static bool IsWireless(ushort vid, ushort pid, string product)
        {
            KnownModel known = KnownDevices.Find(vid, pid);
            if (known != null) return known.Wireless;
            string n = (product ?? "").ToLowerInvariant();
            return n.Contains("wireless") || n.Contains("receiver") || n.Contains("dongle")
                || n.Contains("2.4g") || n.Contains("lightspeed") || n.Contains("bolt");
        }

        public static string Transport(ushort vid, ushort pid, string product, bool bluetooth)
        {
            if (bluetooth) return "蓝牙";
            if (IsWireless(vid, pid, product)) return "2.4G 接收器";
            return "USB 有线";
        }
    }
}
