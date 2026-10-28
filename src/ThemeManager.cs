using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace GearPower
{
    /// <summary>
    /// One fixed Qiju Soft UI palette. The application no longer ships a theme
    /// switcher or accent picker; every surface is built from this token set.
    /// </summary>
    public static class ThemeManager
    {
        private static Window window;
        private static Dictionary<string, object> values = BuildQiju(false);
        public static bool IsDark { get; private set; }

        public static void Initialize(Window owner)
        {
            window = owner;
            IsDark = DisplaySettings.DarkTheme;
            Apply();
            if (window != null)
            {
                window.SourceInitialized += (s, e) => ApplyTitleBar();
                window.Activated += (s, e) => ApplyTitleBar();
                window.Loaded += (s, e) => ApplyTitleBar();
                window.ContentRendered += (s, e) => ApplyTitleBar();
            }
        }

        public static object GetResource(string key)
        {
            object value;
            if (values.TryGetValue(key, out value)) return value;
            return null;
        }

        public static void SetDark(bool dark)
        {
            if (IsDark == dark) return;
            IsDark = dark;
            Apply();
        }

        public static void ApplyTitleBar()
        {
            if (window == null) return;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;
            HidEnumerator.SetTitleBarTheme(hwnd, IsDark);
        }

        private static void Apply()
        {
            values = BuildQiju(IsDark);
            if (window != null)
            {
                foreach (KeyValuePair<string, object> pair in values)
                    window.Resources[pair.Key] = pair.Value;
            }
            ApplyTitleBar();
        }

        private static Dictionary<string, object> BuildQiju(bool dark)
        {
            var d = new Dictionary<string, object>();
            d["ShadowColor"] = Parse(dark ? "#00000000" : "#D6C8BD");
            d["HighlightColor"] = Parse(dark ? "#00000000" : "#FFFFFF");
            d["SidebarInsetOpacity"] = dark ? 0.0 : 1.0;
            AddColor(d, "NavInsetShade", dark ? "#00000000" : "#88A99684");
            AddColor(d, "NavInsetLight", dark ? "#00000000" : "#77FFFFFF");
            AddGradient(d, "AppBackgroundBrush", "#F7EEE7", "#F2E8E0", "#EDE2D8");
            AddColor(d, "TextPrimaryBrush", "#23201D");
            AddColor(d, "TextSecondaryBrush", "#5F5751");
            AddColor(d, "TextMutedBrush", "#6E655E");
            AddColor(d, "AccentBrush", "#2F4BD0");
            AddColor(d, "AccentDimBrush", "#E3E6F8");
            AddColor(d, "WarnBrush", "#E2552D");
            AddColor(d, "WarnTextBrush", "#B63C1C");
            AddColor(d, "DangerBrush", "#B63C1C");
            AddColor(d, "InfoBrush", "#2F4BD0");
            AddColor(d, "PanelBrush", "#F2E8E0");
            AddColor(d, "PanelBorderBrush", "#4DFFFFFF");
            AddColor(d, "OptionBorderBrush", "#D8CABF");
            AddColor(d, "OptionSelectedBorderBrush", "#2F4BD0");
            AddColor(d, "CardBorderBrush", "#4DFFFFFF");
            AddColor(d, "CardBackgroundBrush", "#F2E8E0");
            AddColor(d, "CardHoverBackgroundBrush", "#F2E8E0");
            AddColor(d, "TrackBrush", "#E3D6CB");
            AddColor(d, "TabBarBrush", "#F2E8E0");
            AddColor(d, "TabActiveBrush", "#E9DCD1");
            AddColor(d, "TabActiveForegroundBrush", "#2F4BD0");
            AddColor(d, "TabInactiveForegroundBrush", "#6E655E");
            AddColor(d, "NavActiveBrush", "#E9DCD1");
            AddColor(d, "NavActiveBorderBrush", "#D6C7BB");
            AddColor(d, "HeaderIconBgBrush", "#E5E8F8");
            AddColor(d, "HeaderIconBorderBrush", "#C9D0F1");
            AddColor(d, "GhostForegroundBrush", "#6E655E");
            AddColor(d, "GhostHoverBrush", "#EADFD5");
            AddColor(d, "GhostPressedBrush", "#E1D4C8");
            AddColor(d, "SwitchOnBrush", "#2F4BD0");
            AddColor(d, "SwitchOffBrush", "#EFE4DA");
            AddColor(d, "SwitchBorderBrush", "#D8CABF");
            AddColor(d, "SwitchKnobBrush", "#F8F0E9");
            AddColor(d, "SummaryBrush", "#F2E8E0");
            AddColor(d, "SummaryBorderBrush", "#4DFFFFFF");
            AddColor(d, "InputBrush", "#EDE2D8");
            AddColor(d, "InputBorderBrush", "#D8CABF");
            AddColor(d, "RowBorderBrush", "#E6D9CE");
            AddColor(d, "RowHoverBrush", "#F7EDE5");
            AddAlpha(d, "RowSupportedBrush", 6, "#2F4BD0");
            AddColor(d, "ScrollThumbBrush", "#CDBEB1");
            AddColor(d, "TooltipBgBrush", "#FFF9F4");
            AddColor(d, "TooltipBorderBrush", "#D8CABF");
            AddColor(d, "TooltipForegroundBrush", "#23201D");
            AddColor(d, "ToastBgBrush", "#23201D");
            AddColor(d, "ToastBorderBrush", "#23201D");
            AddColor(d, "ToastTextBrush", "#F8F1EA");
            AddColor(d, "DemoBgBrush", "#F7E7D8");
            AddColor(d, "DemoBorderBrush", "#E2B99A");
            AddColor(d, "DemoTextBrush", "#B63C1C");
            AddColor(d, "EmptyOrbBrush", "#F7EEE7");
            AddColor(d, "EmptyRingBrush", "#D8CABF");
            AddColor(d, "EmptyGlyphBrush", "#B8A99D");
            AddColor(d, "PrimaryButtonBgBrush", "#23201D");
            AddColor(d, "PrimaryButtonForegroundBrush", "#F8F1EA");
            AddColor(d, "PrimaryButtonHoverBrush", "#2E2A26");
            AddColor(d, "PrimaryButtonPressedBrush", "#181512");
            AddColor(d, "DividerBrush", "#246E655E");
            AddColor(d, "RefreshGlyphBrush", "#6E655E");
            if (dark) ApplyDark(d);
            return d;
        }

        private static void ApplyDark(Dictionary<string, object> d)
        {
            AddGradient(d, "AppBackgroundBrush", "#12100E", "#1B1815", "#0E0C0A");
            AddColor(d, "TextPrimaryBrush", "#F3EAE2");
            AddColor(d, "TextSecondaryBrush", "#CBBDB0");
            AddColor(d, "TextMutedBrush", "#9C8E82");
            AddColor(d, "AccentBrush", "#8A9CFF");
            AddColor(d, "AccentDimBrush", "#2A2F55");
            AddColor(d, "WarnBrush", "#FF9A72");
            AddColor(d, "WarnTextBrush", "#FFB49A");
            AddColor(d, "DangerBrush", "#FF9A82");
            AddColor(d, "InfoBrush", "#8A9CFF");
            AddColor(d, "PanelBrush", "#1F1B17");
            AddColor(d, "PanelBorderBrush", "#33FFFFFF");
            AddColor(d, "OptionBorderBrush", "#4A4038");
            AddColor(d, "OptionSelectedBorderBrush", "#8A9CFF");
            AddColor(d, "CardBorderBrush", "#33FFFFFF");
            AddColor(d, "CardBackgroundBrush", "#211D19");
            AddColor(d, "CardHoverBackgroundBrush", "#26211C");
            AddColor(d, "TrackBrush", "#3B332C");
            AddColor(d, "TabBarBrush", "#1F1B17");
            AddColor(d, "TabActiveBrush", "#2B251F");
            AddColor(d, "NavActiveBrush", "#2C2620");
            AddColor(d, "NavActiveBorderBrush", "#4A3D32");
            AddColor(d, "HeaderIconBgBrush", "#2A2F55");
            AddColor(d, "HeaderIconBorderBrush", "#3D467A");
            AddColor(d, "GhostForegroundBrush", "#B9AA9D");
            AddColor(d, "GhostHoverBrush", "#2A241F");
            AddColor(d, "GhostPressedBrush", "#332B24");
            AddColor(d, "SwitchOnBrush", "#8A9CFF");
            AddColor(d, "SwitchOffBrush", "#3A322B");
            AddColor(d, "SwitchBorderBrush", "#4A4038");
            AddColor(d, "SwitchKnobBrush", "#E9DDD2");
            AddColor(d, "SummaryBrush", "#1F1B17");
            AddColor(d, "SummaryBorderBrush", "#33FFFFFF");
            AddColor(d, "InputBrush", "#171310");
            AddColor(d, "InputBorderBrush", "#4A4038");
            AddColor(d, "RowBorderBrush", "#3A3028");
            AddColor(d, "RowHoverBrush", "#2A241F");
            AddColor(d, "ScrollThumbBrush", "#5A4C40");
            AddColor(d, "TooltipBgBrush", "#2B251F");
            AddColor(d, "TooltipBorderBrush", "#4A4038");
            AddColor(d, "TooltipForegroundBrush", "#F3EAE2");
            AddColor(d, "ToastBgBrush", "#F2E8E0");
            AddColor(d, "ToastBorderBrush", "#F2E8E0");
            AddColor(d, "ToastTextBrush", "#1B1815");
            AddColor(d, "DemoBgBrush", "#3A251B");
            AddColor(d, "DemoBorderBrush", "#6B4634");
            AddColor(d, "DemoTextBrush", "#FFB49A");
            AddColor(d, "EmptyOrbBrush", "#211D19");
            AddColor(d, "EmptyRingBrush", "#4A4038");
            AddColor(d, "EmptyGlyphBrush", "#7A6B5F");
            AddColor(d, "PrimaryButtonBgBrush", "#F2E8E0");
            AddColor(d, "PrimaryButtonForegroundBrush", "#1B1815");
            AddColor(d, "PrimaryButtonHoverBrush", "#E8DCD1");
            AddColor(d, "PrimaryButtonPressedBrush", "#D8C8B8");
            AddColor(d, "DividerBrush", "#33FFFFFF");
            AddColor(d, "RefreshGlyphBrush", "#B9AA9D");
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
