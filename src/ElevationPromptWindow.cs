using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace GearPower
{
    /// <summary>
    /// Explains why a low-integrity process cannot create a tray icon and
    /// offers a UAC restart. Running elevated allows Shell_NotifyIcon to reach
    /// the medium-integrity Explorer process.
    /// </summary>
    public static class ElevationPromptWindow
    {
        public static bool Show(Window owner)
        {
            bool elevate = false;

            var dialog = new Window();
            dialog.Owner = owner;
            dialog.WindowStyle = WindowStyle.None;
            dialog.ResizeMode = ResizeMode.NoResize;
            dialog.AllowsTransparency = true;
            dialog.Background = Brushes.Transparent;
            dialog.ShowInTaskbar = false;
            dialog.SizeToContent = SizeToContent.WidthAndHeight;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var card = new Border();
            card.Width = 440;
            card.Background = Theme.Panel;
            card.BorderBrush = Theme.CardBorder;
            card.BorderThickness = new Thickness(1);
            card.CornerRadius = new CornerRadius(28);
            card.Padding = new Thickness(28);
            card.Effect = new DropShadowEffect
            {
                BlurRadius = 34,
                ShadowDepth = 12,
                Direction = 45,
                Opacity = 0.26,
                Color = Color.FromRgb(0xD6, 0xC8, 0xBD)
            };
            TextOptions.SetTextFormattingMode(card, TextFormattingMode.Display);

            var root = new StackPanel();

            var header = new StackPanel();
            header.Orientation = Orientation.Horizontal;
            header.Margin = new Thickness(0, 0, 0, 12);

            var iconBox = new Border();
            iconBox.Width = 34;
            iconBox.Height = 34;
            iconBox.CornerRadius = new CornerRadius(10);
            iconBox.Background = Theme.WithAlpha(Theme.WarnColor, 28);
            iconBox.VerticalAlignment = VerticalAlignment.Center;
            var icon = new TextBlock();
            icon.Text = "\uE7BA";
            icon.FontFamily = Theme.IconFont;
            icon.FontSize = 16;
            icon.Foreground = Theme.Warn;
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            icon.VerticalAlignment = VerticalAlignment.Center;
            iconBox.Child = icon;
            header.Children.Add(iconBox);

            var title = new TextBlock();
            title.Text = "需要管理员权限";
            title.FontFamily = Theme.UiFont;
            title.FontSize = 20;
            title.FontWeight = FontWeights.SemiBold;
            title.Foreground = Theme.Text;
            title.VerticalAlignment = VerticalAlignment.Center;
            title.Margin = new Thickness(10, 0, 0, 0);
            header.Children.Add(title);
            root.Children.Add(header);

            var message = new TextBlock();
            message.Text = "当前程序以低权限（Low Integrity）运行，Windows 会阻止系统托盘图标注册。\n\n" +
                           "以管理员身份重新启动后，托盘图标、悬停卡片和后台运行才能正常工作。";
            message.FontFamily = Theme.UiFont;
            message.FontSize = 11;
            message.Foreground = Theme.Secondary;
            message.TextWrapping = TextWrapping.Wrap;
            root.Children.Add(message);

            var note = new TextBlock();
            note.Text = "也可以选择继续以低权限运行，但关闭主窗口会直接退出。";
            note.FontFamily = Theme.UiFont;
            note.FontSize = 10;
            note.Foreground = Theme.Muted;
            note.TextWrapping = TextWrapping.Wrap;
            note.Margin = new Thickness(0, 10, 0, 0);
            root.Children.Add(note);

            var buttons = new StackPanel();
            buttons.Orientation = Orientation.Horizontal;
            buttons.HorizontalAlignment = HorizontalAlignment.Right;
            buttons.Margin = new Thickness(0, 20, 0, 0);

            var low = MakeButton(owner, "继续低权限", "GhostButton");
            low.Click += (s, e) =>
            {
                elevate = false;
                dialog.Close();
            };
            buttons.Children.Add(low);

            var admin = MakeButton(owner, "以管理员身份重启", "PrimaryButton");
            admin.Margin = new Thickness(8, 0, 0, 0);
            admin.FontWeight = FontWeights.SemiBold;
            admin.Click += (s, e) =>
            {
                elevate = true;
                dialog.Close();
            };
            buttons.Children.Add(admin);
            root.Children.Add(buttons);

            card.Child = root;

            var host = new Grid();
            host.Margin = new Thickness(16);
            host.Children.Add(card);
            dialog.Content = host;

            dialog.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    elevate = false;
                    dialog.Close();
                }
            };

            dialog.ShowDialog();
            return elevate;
        }

        private static Button MakeButton(Window owner, string text, string styleKey)
        {
            var button = new Button();
            button.Content = text;
            button.MinWidth = 96;
            button.Padding = new Thickness(14, 7, 14, 7);
            button.FontFamily = Theme.UiFont;
            button.FontSize = 10.5;
            try
            {
                Style style = owner.TryFindResource(styleKey) as Style;
                if (style != null) button.Style = style;
            }
            catch
            {
            }
            return button;
        }
    }
}
