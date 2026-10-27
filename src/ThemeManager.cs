using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace GearPower
{
    public enum ThemeMode
    {
        System,
        Light,
        Dark
    }

    public enum AccentPreset
    {
        Green,
        Pink,
        Yellow,
        Blue
    }

    public static class ThemeManager
    {
        private const string SettingsPath = @"Software\GearPower";
        private const string LegacySettingsPath = @"Software\BatteryPeek";
        private const string PersonalizePath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

        private static Window window;
        private static Dictionary<string, object> values = BuildDark();
        private static DispatcherTimer titleTimer;

        public static ThemeMode Mode { get; private set; }
        public static AccentPreset Accent { get; private set; }
        public static bool IsDark { get; private set; }
        public static event EventHandler Changed;

        public static void Initialize(Window owner)
        {
            window = owner;
            Mode = LoadMode();
            Accent = LoadAccent();
            Apply(false);
            if (window != null)
            {
                window.SourceInitialized += (s, e) => ApplyTitleBar();
                window.Activated += (s, e) => ApplyTitleBar();
                window.Loaded += (s, e) =>
                {
                    ApplyTitleBar();
                    ScheduleTitleBar();
                };
                window.ContentRendered += (s, e) =>
                {
                    ApplyTitleBar();
                    ScheduleTitleBar();
                };
            }
        }

        public static void SetMode(ThemeMode mode)
        {
            SetMode(mode, true);
        }

        public static void SetMode(ThemeMode mode, bool save)
        {
            Mode = mode;
            if (save) SaveMode();
            Apply(true);
        }

        public static void SetAccent(AccentPreset accent)
        {
            SetAccent(accent, true);
        }

        public static void SetAccent(AccentPreset accent, bool save)
        {
            Accent = accent;
            if (save) SaveAccent();
            Apply(true);
        }

        public static void RefreshSystem()
        {
            if (Mode != ThemeMode.System || window == null) return;
            if (SystemUsesDark() != IsDark) Apply(true);
        }

        public static object GetResource(string key)
        {
            object value;
            if (values.TryGetValue(key, out value)) return value;
            return null;
        }

        public static void ApplyTitleBar()
        {
            if (window == null) return;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;
            HidEnumerator.SetTitleBarTheme(hwnd, IsDark);
        }

        private static void ScheduleTitleBar()
        {
            if (window == null) return;
            if (titleTimer == null)
            {
                titleTimer = new DispatcherTimer();
                titleTimer.Interval = TimeSpan.FromMilliseconds(2000);
                titleTimer.Tick += (s, e) =>
                {
                    titleTimer.Stop();
                    ApplyTitleBar();
                };
            }
            titleTimer.Stop();
            titleTimer.Start();
        }

        private static void Apply(bool notify)
        {
            IsDark = Mode == ThemeMode.Dark || (Mode == ThemeMode.System && SystemUsesDark());
            values = IsDark ? BuildDark() : BuildLight();
            ApplyAccent(values);

            if (window != null)
            {
                foreach (KeyValuePair<string, object> pair in values)
                {
                    window.Resources[pair.Key] = pair.Value;
                }
            }

            ApplyTitleBar();
            ScheduleTitleBar();
            if (window != null)
            {
                window.Dispatcher.BeginInvoke(new Action(ApplyTitleBar), DispatcherPriority.ApplicationIdle);
            }

            if (notify)
            {
                EventHandler handler = Changed;
                if (handler != null) handler(null, EventArgs.Empty);
            }
        }

        private static ThemeMode LoadMode()
        {
            ThemeMode? mode = ReadMode(SettingsPath);
            if (!mode.HasValue) mode = ReadMode(LegacySettingsPath);
            return mode.HasValue ? mode.Value : ThemeMode.System;
        }

        private static ThemeMode? ReadMode(string path)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        string text = key.GetValue("ThemeMode") as string;
                        if (!string.IsNullOrEmpty(text))
                        {
                            if (text.Equals("light", StringComparison.OrdinalIgnoreCase)) return ThemeMode.Light;
                            if (text.Equals("dark", StringComparison.OrdinalIgnoreCase)) return ThemeMode.Dark;
                            if (text.Equals("system", StringComparison.OrdinalIgnoreCase)) return ThemeMode.System;
                        }
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        private static void SaveMode()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(SettingsPath))
                {
                    if (key != null) key.SetValue("ThemeMode", Mode.ToString().ToLowerInvariant());
                }
            }
            catch
            {
            }
        }

        private static AccentPreset LoadAccent()
        {
            AccentPreset? accent = ReadAccent(SettingsPath);
            if (!accent.HasValue) accent = ReadAccent(LegacySettingsPath);
            return accent.HasValue ? accent.Value : AccentPreset.Green;
        }

        private static AccentPreset? ReadAccent(string path)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null)
                    {
                        string text = key.GetValue("AccentColor") as string;
                        if (!string.IsNullOrEmpty(text))
                        {
                            if (text.Equals("green", StringComparison.OrdinalIgnoreCase)) return AccentPreset.Green;
                            if (text.Equals("pink", StringComparison.OrdinalIgnoreCase)) return AccentPreset.Pink;
                            if (text.Equals("yellow", StringComparison.OrdinalIgnoreCase)) return AccentPreset.Yellow;
                            if (text.Equals("blue", StringComparison.OrdinalIgnoreCase)) return AccentPreset.Blue;
                        }
                    }
                }
            }
            catch
            {
            }
            return null;
        }

        private static void SaveAccent()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(SettingsPath))
                {
                    if (key != null) key.SetValue("AccentColor", Accent.ToString().ToLowerInvariant());
                }
            }
            catch
            {
            }
        }

        private static bool SystemUsesDark()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(PersonalizePath))
                {
                    if (key != null)
                    {
                        object value = key.GetValue("AppsUseLightTheme");
                        if (value is int) return (int)value == 0;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private static void ApplyAccent(Dictionary<string, object> d)
        {
            string accent;
            string dim;
            string selected;
            string primary;
            string primaryForeground;
            string hover;
            string pressed;

            switch (Accent)
            {
                case AccentPreset.Pink:
                    if (IsDark)
                    {
                        accent = "#F472B6"; dim = "#3A1426"; selected = "#A34E75";
                        primary = "#F472B6"; primaryForeground = "#2A0714";
                        hover = "#F9A8D4"; pressed = "#EC4899";
                    }
                    else
                    {
                        accent = "#DB2777"; dim = "#FCE7F3"; selected = "#F0A6C4";
                        primary = "#DB2777"; primaryForeground = "#FFFFFF";
                        hover = "#BE185D"; pressed = "#9D174D";
                    }
                    break;
                case AccentPreset.Yellow:
                    if (IsDark)
                    {
                        accent = "#FACC15"; dim = "#332B0A"; selected = "#96771A";
                        primary = "#FACC15"; primaryForeground = "#221A00";
                        hover = "#FDE047"; pressed = "#EAB308";
                    }
                    else
                    {
                        accent = "#B45309"; dim = "#FEF3C7"; selected = "#E7C16A";
                        primary = "#B45309"; primaryForeground = "#FFFFFF";
                        hover = "#92400E"; pressed = "#78350F";
                    }
                    break;
                case AccentPreset.Blue:
                    if (IsDark)
                    {
                        accent = "#60A5FA"; dim = "#132A4A"; selected = "#3B74B8";
                        primary = "#60A5FA"; primaryForeground = "#06152A";
                        hover = "#93C5FD"; pressed = "#3B82F6";
                    }
                    else
                    {
                        accent = "#2563EB"; dim = "#DBEAFE"; selected = "#93B4F5";
                        primary = "#2563EB"; primaryForeground = "#FFFFFF";
                        hover = "#1D4ED8"; pressed = "#1E3A8A";
                    }
                    break;
                default:
                    if (IsDark)
                    {
                        accent = "#4ADE80"; dim = "#173325"; selected = "#3F916A";
                        primary = "#4ADE80"; primaryForeground = "#07130C";
                        hover = "#6EE79A"; pressed = "#3CCB6F";
                    }
                    else
                    {
                        accent = "#16A34A"; dim = "#DCFCE7"; selected = "#8BCFA8";
                        primary = "#16A34A"; primaryForeground = "#FFFFFF";
                        hover = "#15803D"; pressed = "#166534";
                    }
                    break;
            }

            AddColor(d, "AccentBrush", accent);
            AddColor(d, "AccentDimBrush", dim);
            AddColor(d, "SwitchOnBrush", IsDark ? "#8A9CFF" : "#2F4BD0");
            AddColor(d, "OptionSelectedBorderBrush", selected);
            AddColor(d, "PrimaryButtonBgBrush", primary);
            AddColor(d, "PrimaryButtonForegroundBrush", primaryForeground);
            AddColor(d, "PrimaryButtonHoverBrush", hover);
            AddColor(d, "PrimaryButtonPressedBrush", pressed);
            AddAlpha(d, "RowSupportedBrush", IsDark ? (byte)28 : (byte)30, accent);
        }

        private static Dictionary<string, object> BuildDark()
        {
            var d = new Dictionary<string, object>();
            AddGradient(d, "AppBackgroundBrush", "#1E1B18", "#191613", "#221E1A");
            AddColor(d, "TextPrimaryBrush", "#F2EDE8");
            AddColor(d, "TextSecondaryBrush", "#C3B6AB");
            AddColor(d, "TextMutedBrush", "#94877C");
            AddColor(d, "AccentBrush", "#8A9CFF");
            AddColor(d, "AccentDimBrush", "#28305A");
            AddColor(d, "WarnBrush", "#F0A36B");
            AddColor(d, "DangerBrush", "#F07A57");
            AddColor(d, "InfoBrush", "#8A9CFF");
            AddColor(d, "PanelBrush", "#201C18");
            AddColor(d, "PanelBorderBrush", "#00201C18");
            AddColor(d, "OptionBorderBrush", "#3A332C");
            AddColor(d, "OptionSelectedBorderBrush", "#8A9CFF");
            AddColor(d, "CardBorderBrush", "#00201C18");
            AddGradient(d, "CardBackgroundBrush", "#26211C", "#201B17", null);
            AddGradient(d, "CardHoverBackgroundBrush", "#2B2620", "#221D18", null);
            AddColor(d, "TrackBrush", "#332D27");
            AddColor(d, "TabBarBrush", "#201C18");
            AddColor(d, "TabActiveBrush", "#26211C");
            AddColor(d, "TabActiveForegroundBrush", "#8A9CFF");
            AddColor(d, "TabInactiveForegroundBrush", "#C3B6AB");
            AddColor(d, "HeaderIconBgBrush", "#28305A");
            AddColor(d, "HeaderIconBorderBrush", "#3A467E");
            AddColor(d, "GhostForegroundBrush", "#C3B6AB");
            AddColor(d, "GhostHoverBrush", "#2A2520");
            AddColor(d, "GhostPressedBrush", "#332D27");
            AddColor(d, "SwitchOnBrush", "#8A9CFF");
            AddColor(d, "SwitchOffBrush", "#2A2520");
            AddColor(d, "SwitchBorderBrush", "#3A332C");
            AddColor(d, "SwitchKnobBrush", "#F2EDE8");
            AddColor(d, "SummaryBrush", "#201C18");
            AddColor(d, "SummaryBorderBrush", "#00201C18");
            AddColor(d, "InputBrush", "#1A1613");
            AddColor(d, "InputBorderBrush", "#3A332C");
            AddColor(d, "RowBorderBrush", "#2A2520");
            AddColor(d, "RowHoverBrush", "#26211C");
            AddAlpha(d, "RowSupportedBrush", 28, "#8A9CFF");
            AddColor(d, "ScrollThumbBrush", "#4A423B");
            AddColor(d, "TooltipBgBrush", "#2A2520");
            AddColor(d, "TooltipBorderBrush", "#3A332C");
            AddColor(d, "TooltipForegroundBrush", "#F2EDE8");
            AddColor(d, "ToastBgBrush", "#26211C");
            AddColor(d, "ToastBorderBrush", "#3A332C");
            AddColor(d, "ToastTextBrush", "#C9D2FF");
            AddColor(d, "DemoBgBrush", "#33291E");
            AddColor(d, "DemoBorderBrush", "#5A4632");
            AddColor(d, "DemoTextBrush", "#F0A36B");
            AddColor(d, "EmptyOrbBrush", "#26211C");
            AddColor(d, "EmptyRingBrush", "#3A332C");
            AddColor(d, "EmptyGlyphBrush", "#6A5D52");
            AddColor(d, "PrimaryButtonBgBrush", "#4ADE80");
            AddColor(d, "PrimaryButtonForegroundBrush", "#07130C");
            AddColor(d, "PrimaryButtonHoverBrush", "#6EE79A");
            AddColor(d, "PrimaryButtonPressedBrush", "#3CCB6F");
            AddColor(d, "DividerBrush", "#1E2A38");
            AddColor(d, "RefreshGlyphBrush", "#B7C6D8");
            return d;
        }

        private static Dictionary<string, object> BuildLight()
        {
            var d = new Dictionary<string, object>();
            AddGradient(d, "AppBackgroundBrush", "#F2E8E0", "#EDE2D8", "#F5ECE5");
            AddColor(d, "TextPrimaryBrush", "#23201D");
            AddColor(d, "TextSecondaryBrush", "#6E655E");
            AddColor(d, "TextMutedBrush", "#9A8E84");
            AddColor(d, "AccentBrush", "#2F4BD0");
            AddColor(d, "AccentDimBrush", "#E3E6F8");
            AddColor(d, "WarnBrush", "#E2552D");
            AddColor(d, "DangerBrush", "#C9401E");
            AddColor(d, "InfoBrush", "#2F4BD0");
            AddColor(d, "PanelBrush", "#F2E8E0");
            AddColor(d, "PanelBorderBrush", "#00F2E8E0");
            AddColor(d, "OptionBorderBrush", "#D8CABF");
            AddColor(d, "OptionSelectedBorderBrush", "#2F4BD0");
            AddColor(d, "CardBorderBrush", "#00F2E8E0");
            AddGradient(d, "CardBackgroundBrush", "#F7EEE7", "#F0E5DB", null);
            AddGradient(d, "CardHoverBackgroundBrush", "#FAF2EC", "#EFE3D8", null);
            AddColor(d, "TrackBrush", "#E3D6CB");
            AddColor(d, "TabBarBrush", "#F2E8E0");
            AddColor(d, "TabActiveBrush", "#F2E8E0");
            AddColor(d, "TabActiveForegroundBrush", "#2F4BD0");
            AddColor(d, "TabInactiveForegroundBrush", "#6E655E");
            AddColor(d, "HeaderIconBgBrush", "#E5E8F8");
            AddColor(d, "HeaderIconBorderBrush", "#C9D0F1");
            AddColor(d, "GhostForegroundBrush", "#6E655E");
            AddColor(d, "GhostHoverBrush", "#EADFD5");
            AddColor(d, "GhostPressedBrush", "#E1D4C8");
            AddColor(d, "SwitchOnBrush", "#2F4BD0");
            AddColor(d, "SwitchOffBrush", "#EFE4DA");
            AddColor(d, "SwitchBorderBrush", "#D8CABF");
            AddColor(d, "SwitchKnobBrush", "#FFFDFC");
            AddColor(d, "SummaryBrush", "#F2E8E0");
            AddColor(d, "SummaryBorderBrush", "#00F2E8E0");
            AddColor(d, "InputBrush", "#EDE2D8");
            AddColor(d, "InputBorderBrush", "#D8CABF");
            AddColor(d, "RowBorderBrush", "#E6D9CE");
            AddColor(d, "RowHoverBrush", "#F7EDE5");
            AddAlpha(d, "RowSupportedBrush", 30, "#2F4BD0");
            AddColor(d, "ScrollThumbBrush", "#CDBEB1");
            AddColor(d, "TooltipBgBrush", "#FFF9F4");
            AddColor(d, "TooltipBorderBrush", "#D8CABF");
            AddColor(d, "TooltipForegroundBrush", "#23201D");
            AddColor(d, "ToastBgBrush", "#F7EEE7");
            AddColor(d, "ToastBorderBrush", "#D8CABF");
            AddColor(d, "ToastTextBrush", "#2F4BD0");
            AddColor(d, "DemoBgBrush", "#F7E7D8");
            AddColor(d, "DemoBorderBrush", "#E2B99A");
            AddColor(d, "DemoTextBrush", "#B84E24");
            AddColor(d, "EmptyOrbBrush", "#F7EEE7");
            AddColor(d, "EmptyRingBrush", "#D8CABF");
            AddColor(d, "EmptyGlyphBrush", "#B8A99D");
            AddColor(d, "PrimaryButtonBgBrush", "#2F4BD0");
            AddColor(d, "PrimaryButtonForegroundBrush", "#FFFFFF");
            AddColor(d, "PrimaryButtonHoverBrush", "#263DAE");
            AddColor(d, "PrimaryButtonPressedBrush", "#1F338F");
            AddColor(d, "DividerBrush", "#DED0C4");
            AddColor(d, "RefreshGlyphBrush", "#6E655E");
            return d;
        }

        private static Color Parse(string hex)
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }

        private static SolidColorBrush Frozen(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private static void AddColor(Dictionary<string, object> target, string key, string hex)
        {
            target[key] = Frozen(Parse(hex));
        }

        private static void AddAlpha(Dictionary<string, object> target, string key, byte alpha, string hex)
        {
            Color c = Parse(hex);
            target[key] = Frozen(Color.FromArgb(alpha, c.R, c.G, c.B));
        }

        private static void AddGradient(Dictionary<string, object> target, string key, string start, string end, string middle)
        {
            var brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 1);
            brush.GradientStops.Add(new GradientStop(Parse(start), 0));
            if (!string.IsNullOrEmpty(middle))
                brush.GradientStops.Add(new GradientStop(Parse(middle), 0.55));
            brush.GradientStops.Add(new GradientStop(Parse(end), 1));
            brush.Freeze();
            target[key] = brush;
        }
    }
}

