using System;
using System.Windows;
using System.Windows.Media;

namespace GearPower
{
    public static class Theme
    {
        public static readonly FontFamily UiFont = new FontFamily("Microsoft YaHei UI");
        public static readonly FontFamily LabelFont = new FontFamily("Bahnschrift");
        public static readonly FontFamily NumberFont = new FontFamily("Consolas");
        public static readonly FontFamily IconFont = new FontFamily("Segoe MDL2 Assets");

        public static Color AccentColor
        {
            get { return GetColor("AccentBrush"); }
        }

        public static Color WarnColor
        {
            get { return GetColor("WarnBrush"); }
        }

        public static Color DangerColor
        {
            get { return GetColor("DangerBrush"); }
        }

        public static Color InfoColor
        {
            get { return GetColor("InfoBrush"); }
        }

        public static Color MutedColor
        {
            get { return GetColor("TextMutedBrush"); }
        }

        public static Color TextColor
        {
            get { return GetColor("TextPrimaryBrush"); }
        }

        public static Color SecondaryColor
        {
            get { return GetColor("TextSecondaryBrush"); }
        }

        public static Color PanelColor
        {
            get { return GetColor("PanelBrush"); }
        }

        public static Color PanelBorderColor
        {
            get { return GetColor("PanelBorderBrush"); }
        }

        public static Color GhostHoverColor
        {
            get { return GetColor("GhostHoverBrush"); }
        }

        public static Brush Text { get { return GetBrush("TextPrimaryBrush"); } }
        public static Brush Secondary { get { return GetBrush("TextSecondaryBrush"); } }
        public static Brush Muted { get { return GetBrush("TextMutedBrush"); } }
        public static Brush Accent { get { return GetBrush("AccentBrush"); } }
        public static Brush AccentDim { get { return GetBrush("AccentDimBrush"); } }
        public static Brush Warn { get { return GetBrush("WarnBrush"); } }
        public static Brush Danger { get { return GetBrush("DangerBrush"); } }
        public static Brush Info { get { return GetBrush("InfoBrush"); } }
        public static Brush Panel { get { return GetBrush("PanelBrush"); } }
        public static Brush PanelBorder { get { return GetBrush("PanelBorderBrush"); } }
        public static Brush OptionBorder { get { return GetBrush("OptionBorderBrush"); } }
        public static Brush OptionSelectedBorder { get { return GetBrush("OptionSelectedBorderBrush"); } }
        public static Brush CardBorder { get { return GetBrush("CardBorderBrush"); } }
        public static Brush Track { get { return GetBrush("TrackBrush"); } }
        public static Brush Input { get { return GetBrush("InputBrush"); } }
        public static Brush InputBorder { get { return GetBrush("InputBorderBrush"); } }
        public static Brush RowBorder { get { return GetBrush("RowBorderBrush"); } }
        public static Brush RowHover { get { return GetBrush("RowHoverBrush"); } }
        public static Brush RowSupported { get { return GetBrush("RowSupportedBrush"); } }
        public static Brush TabBar { get { return GetBrush("TabBarBrush"); } }
        public static Brush TabActive { get { return GetBrush("TabActiveBrush"); } }
        public static Brush SwitchOn { get { return GetBrush("SwitchOnBrush"); } }
        public static Color SwitchOnColor
        {
            get
            {
                SolidColorBrush brush = SwitchOn as SolidColorBrush;
                return brush == null ? AccentColor : brush.Color;
            }
        }
        public static Brush TabActiveForeground { get { return GetBrush("TabActiveForegroundBrush"); } }
        public static Brush TabInactiveForeground { get { return GetBrush("TabInactiveForegroundBrush"); } }
        public static Brush GhostHover { get { return GetBrush("GhostHoverBrush"); } }
        public static Brush GhostPressed { get { return GetBrush("GhostPressedBrush"); } }

        public static LinearGradientBrush CardBackground
        {
            get { return (LinearGradientBrush)GetBrush("CardBackgroundBrush"); }
        }

        public static LinearGradientBrush CardHoverBackground
        {
            get { return (LinearGradientBrush)GetBrush("CardHoverBackgroundBrush"); }
        }

        public static Color KeyAccentColor
        {
            get { return ThemeManager.IsDark ? Color.FromRgb(0x8A, 0x9C, 0xFF) : Color.FromRgb(0x2F, 0x4B, 0xD0); }
        }

        public static Color LevelColor(int? percent, bool charging)
        {
            if (charging) return KeyAccentColor;
            if (!percent.HasValue) return MutedColor;
            if (percent.Value <= 15) return WarnColor;
            if (percent.Value <= 35) return WarnColor;
            return KeyAccentColor;
        }

        public static Brush LevelBrush(int? percent, bool charging)
        {
            Color color = LevelColor(percent, charging);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        public static SolidColorBrush WithAlpha(Color color, byte alpha)
        {
            var brush = new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
            brush.Freeze();
            return brush;
        }

        private static Brush GetBrush(string key)
        {
            Brush brush = ThemeManager.GetResource(key) as Brush;
            if (brush == null) return Brushes.Transparent;
            return brush;
        }

        private static Color GetColor(string key)
        {
            var brush = ThemeManager.GetResource(key) as SolidColorBrush;
            if (brush == null) return Colors.Gray;
            return brush.Color;
        }
    }
}
