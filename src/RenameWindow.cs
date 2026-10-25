using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GearPower
{
    /// <summary>
    /// Small themed prompt for a user-assigned device name. Returns null on
    /// cancel, an empty string for "restore default", otherwise the new name.
    /// </summary>
    public static class RenameWindow
    {
        public static string Show(Window owner, string current, string deviceId)
        {
            string result = null;
            var card = new Border();
            card.Width = 420;
            card.Background = Theme.Panel;
            card.BorderBrush = Theme.PanelBorder;
            card.BorderThickness = new Thickness(1);
            card.CornerRadius = new CornerRadius(14);
            card.Padding = new Thickness(22);
            card.Effect = new DropShadowEffect
            {
                BlurRadius = 28,
                ShadowDepth = 8,
                Opacity = 0.36,
                Color = Colors.Black
            };
            TextOptions.SetTextFormattingMode(card, TextFormattingMode.Display);

            var root = new StackPanel();
            var title = new TextBlock();
            title.Text = "重命名设备";
            title.FontFamily = Theme.UiFont;
            title.FontSize = 15;
            title.FontWeight = FontWeights.SemiBold;
            title.Foreground = Theme.Text;
            root.Children.Add(title);

            var message = new TextBlock();
            message.Text = "只修改显示名称，不改 VID/PID、协议或设备设置。";
            message.FontFamily = Theme.UiFont;
            message.FontSize = 11;
            message.Foreground = Theme.Secondary;
            message.TextWrapping = TextWrapping.Wrap;
            message.Margin = new Thickness(0, 7, 0, 0);
            root.Children.Add(message);

            var label = new TextBlock();
            label.Text = "显示名称";
            label.FontFamily = Theme.UiFont;
            label.FontSize = 10.5;
            label.Foreground = Theme.Muted;
            label.Margin = new Thickness(0, 16, 0, 5);
            root.Children.Add(label);

            var input = new TextBox();
            input.Text = current ?? "";
            input.FontFamily = Theme.UiFont;
            input.FontSize = 13;
            input.Foreground = Theme.Text;
            input.Background = Theme.Input;
            input.BorderBrush = Theme.InputBorder;
            input.BorderThickness = new Thickness(1);
            input.Padding = new Thickness(9, 7, 9, 7);
            input.MaxLength = 80;
            input.SelectionStart = input.Text.Length;
            root.Children.Add(input);

            var id = new TextBlock();
            id.Text = deviceId ?? "";
            id.FontFamily = Theme.UiFont;
            id.FontSize = 9.5;
            id.Foreground = Theme.Muted;
            id.TextTrimming = TextTrimming.CharacterEllipsis;
            id.Margin = new Thickness(0, 5, 0, 0);
            root.Children.Add(id);

            var error = new TextBlock();
            error.FontFamily = Theme.UiFont;
            error.FontSize = 10;
            error.Foreground = Theme.Muted;
            error.Margin = new Thickness(0, 6, 0, 0);
            error.Visibility = Visibility.Collapsed;
            root.Children.Add(error);

            var buttons = new StackPanel();
            buttons.Orientation = Orientation.Horizontal;
            buttons.HorizontalAlignment = HorizontalAlignment.Right;
            buttons.Margin = new Thickness(0, 18, 0, 0);

            var reset = MakeButton(owner, "恢复默认", "GhostButton");
            reset.Foreground = Theme.Muted;
            reset.Click += (s, e) =>
            {
                result = "";
                CloseDialog(owner);
            };
            buttons.Children.Add(reset);

            var cancel = MakeButton(owner, "取消", "GhostButton");
            cancel.Margin = new Thickness(8, 0, 0, 0);
            cancel.Click += (s, e) =>
            {
                result = null;
                CloseDialog(owner);
            };
            buttons.Children.Add(cancel);

            var save = MakeButton(owner, "保存", "PrimaryButton");
            save.Margin = new Thickness(8, 0, 0, 0);
            save.Background = Theme.Accent;
            save.Foreground = Brushes.White;
            save.BorderBrush = Theme.Accent;
            save.FontWeight = FontWeights.SemiBold;
            save.Click += (s, e) =>
            {
                string name = (input.Text ?? "").Trim();
                if (name.Length == 0)
                {
                    error.Text = "请输入名称；如需恢复厂商名称，请点「恢复默认」。";
                    error.Visibility = Visibility.Visible;
                    input.Focus();
                    return;
                }
                result = name;
                CloseDialog(owner);
            };
            buttons.Children.Add(save);
            root.Children.Add(buttons);
            card.Child = root;

            var host = new Grid();
            host.Margin = new Thickness(16);
            host.Children.Add(card);

            var dialog = new Window();
            dialog.Owner = owner;
            dialog.WindowStyle = WindowStyle.None;
            dialog.ResizeMode = ResizeMode.NoResize;
            dialog.AllowsTransparency = true;
            dialog.Background = Brushes.Transparent;
            dialog.ShowInTaskbar = false;
            dialog.SizeToContent = SizeToContent.WidthAndHeight;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Content = host;
            dialog.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    result = null;
                    dialog.Close();
                }
                else if (e.Key == Key.Enter && input.IsFocused)
                {
                    e.Handled = true;
                    string name = (input.Text ?? "").Trim();
                    if (name.Length > 0)
                    {
                        result = name;
                        dialog.Close();
                    }
                }
            };
            dialog.Loaded += (s, e) =>
            {
                input.Focus();
                input.SelectAll();
            };

            owner.Tag = dialog;
            dialog.ShowDialog();
            owner.Tag = null;
            return result;
        }

        private static Button MakeButton(Window owner, string text, string styleName)
        {
            var button = new Button();
            button.Content = text;
            button.FontFamily = Theme.UiFont;
            button.FontSize = 10.5;
            button.Padding = new Thickness(14, 6, 14, 6);
            Style style = owner.TryFindResource(styleName) as Style;
            if (style != null) button.Style = style;
            return button;
        }

        private static void CloseDialog(Window owner)
        {
            var dialog = owner.Tag as Window;
            if (dialog != null) dialog.Close();
        }
    }
}
