using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace GearPower
{
    /// <summary>
    /// Themed history merge dialog. The old UI used a native ContextMenu and
    /// MessageBox, which no longer matched the Qiju shell.
    /// </summary>
    public static class HistoryMergeWindow
    {
        public static DeviceReading Show(Window owner, DeviceReading history, IList<DeviceReading> candidates)
        {
            if (owner == null || history == null || candidates == null || candidates.Count == 0) return null;

            DeviceReading selected = candidates[0];
            var radios = new List<RadioButton>();

            var card = new Border();
            card.Width = 460;
            card.Background = Theme.Panel;
            card.BorderBrush = Theme.CardBorder;
            card.BorderThickness = new Thickness(1);
            card.CornerRadius = new CornerRadius(28);
            card.Padding = new Thickness(28);
            card.Effect = new DropShadowEffect
            {
                BlurRadius = 34,
                ShadowDepth = 12,
                Opacity = 0.26,
                Direction = 45,
                Color = Color.FromRgb(0xD6, 0xC8, 0xBD)
            };
            TextOptions.SetTextFormattingMode(card, TextFormattingMode.Display);

            var root = new StackPanel();

            var title = new TextBlock();
            title.Text = "合并历史设备";
            title.FontFamily = Theme.UiFont;
            title.FontSize = 20;
            title.FontWeight = FontWeights.SemiBold;
            title.Foreground = Theme.Text;
            root.Children.Add(title);

            var message = new TextBlock();
            message.Text = "“" + history.Name + "”是旧记录。请选择它对应的当前设备；合并后只保留当前卡片和最新电量，旧名称会继承，历史文件会先备份。";
            message.FontFamily = Theme.UiFont;
            message.FontSize = 11.5;
            message.Foreground = Theme.Secondary;
            message.TextWrapping = TextWrapping.Wrap;
            message.Margin = new Thickness(0, 10, 0, 0);
            root.Children.Add(message);

            var list = new StackPanel();
            list.Margin = new Thickness(0, 16, 0, 0);
            foreach (DeviceReading candidate in candidates)
            {
                DeviceReading target = candidate;
                var row = new Border();
                row.Background = Theme.Input;
                row.BorderBrush = Theme.PanelBorder;
                row.BorderThickness = new Thickness(1);
                row.CornerRadius = new CornerRadius(16);
                row.Padding = new Thickness(12, 9, 12, 9);
                row.Margin = new Thickness(0, 0, 0, 7);

                var radio = new RadioButton();
                radio.GroupName = "HistoryMergeCandidate";
                radio.FontFamily = Theme.UiFont;
                radio.FontSize = 11.5;
                radio.Foreground = Theme.Text;
                radio.Content = target.Name + "  ·  " + target.Id;
                radio.Tag = target;
                radio.IsChecked = ReferenceEquals(target, selected);
                radio.Checked += (s, e) => selected = (DeviceReading)((RadioButton)s).Tag;
                row.Child = radio;
                list.Children.Add(row);
                radios.Add(radio);
            }
            root.Children.Add(list);

            var buttons = new StackPanel();
            buttons.Orientation = Orientation.Horizontal;
            buttons.HorizontalAlignment = HorizontalAlignment.Right;
            buttons.Margin = new Thickness(0, 20, 0, 0);

            var cancel = MakeButton(owner, "取消", "GhostButton");
            buttons.Children.Add(cancel);

            var merge = MakeButton(owner, "合并所选", "PrimaryButton");
            merge.Margin = new Thickness(8, 0, 0, 0);
            merge.IsEnabled = radios.Count > 0;
            buttons.Children.Add(merge);
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

            cancel.Click += (s, e) => { selected = null; dialog.Close(); };
            merge.Click += (s, e) =>
            {
                if (selected != null) dialog.Close();
            };
            dialog.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    selected = null;
                    dialog.Close();
                }
            };

            dialog.ShowDialog();
            return selected;
        }

        private static Button MakeButton(Window owner, string text, string styleKey)
        {
            var button = new Button();
            button.Content = text;
            button.MinWidth = 88;
            button.Height = 40;
            button.Padding = new Thickness(16, 7, 16, 7);
            button.FontFamily = Theme.UiFont;
            button.FontSize = 11;
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
