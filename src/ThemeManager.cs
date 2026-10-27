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
        private static Dictionary<string, object> values = BuildQiju();

        public static void Initialize(Window owner)
        {
            window = owner;
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

        public static void ApplyTitleBar()
        {
            if (window == null) return;
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd == IntPtr.Zero) return;
            HidEnumerator.SetTitleBarTheme(hwnd, false);
        }

        private static void Apply()
        {
            values = BuildQiju();
            if (window != null)
            {
                foreach (KeyValuePair<string, object> pair in values)
                    window.Resources[pair.Key] = pair.Value;
            }
            ApplyTitleBar();
        }

        private static Dictionary<string, object> BuildQiju()
        {
            var d = new Dictionary<string, object>();
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
