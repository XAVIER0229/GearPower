using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Drawing = System.Drawing;
using WinForms = System.Windows.Forms;

namespace GearPower
{
    /// <summary>
    /// A small themed window used as the tray context menu. A real top-level
    /// window (instead of WinForms ContextMenuStrip) lets the menu use the same
    /// rounded cards, typography, colors and hover feedback as the main UI.
    /// </summary>
    public static class TrayMenuWindow
    {
        private static Window current;

        public static void Show(int screenX, int screenY, List<DeviceReading> readings, Action open, Action refresh, Action exit)
        {
            CloseCurrent();

            List<DeviceReading> data = readings == null
                ? new List<DeviceReading>()
                : new List<DeviceReading>(readings);
            data = DevicePriority.Sort(data);
            DeviceReading primary = TrayIconRenderer.Primary(data);

            var card = new Border();
            card.Width = 328;
            card.Background = Theme.Panel;
            card.BorderBrush = Theme.CardBorder;
            card.BorderThickness = new Thickness(1);
            card.CornerRadius = new CornerRadius(22);
            card.Padding = new Thickness(8);
            card.Effect = new DropShadowEffect
            {
                BlurRadius = 32,
                ShadowDepth = 12,
                Direction = 45,
                Opacity = 0.24,
                Color = Color.FromRgb(0xD6, 0xC8, 0xBD)
            };
            TextOptions.SetTextFormattingMode(card, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(card, TextRenderingMode.ClearType);
            card.SnapsToDevicePixels = true;

            var root = new StackPanel();
            root.Children.Add(BuildHeader(data, primary));
            root.Children.Add(BuildSeparator());

            if (data.Count == 0)
            {
                var empty = Text("未发现设备", 11, Theme.Muted, FontWeights.Normal);
                empty.Margin = new Thickness(10, 12, 10, 12);
                root.Children.Add(empty);
            }
            else
            {
                var list = new StackPanel();
                foreach (DeviceReading reading in data)
                {
                    list.Children.Add(BuildDeviceRow(reading, primary, data.Count, open));
                }
                if (data.Count > 5)
                {
                    var scroll = new ScrollViewer();
                    scroll.MaxHeight = 300;
                    scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    scroll.Content = list;
                    root.Children.Add(scroll);
                }
                else
                {
                    root.Children.Add(list);
                }
            }

            root.Children.Add(BuildSeparator());
            root.Children.Add(BuildAction("\uE8A7", "打开主界面", open, false));
            root.Children.Add(BuildAction("\uE72C", "立即刷新", refresh, false));
            root.Children.Add(BuildSeparator());
            root.Children.Add(BuildAction("\uE8BB", "退出", exit, true));

            var hint = Text("双击托盘图标可打开主界面", 8.5, Theme.Muted, FontWeights.Normal);
            hint.Margin = new Thickness(10, 2, 10, 7);
            hint.HorizontalAlignment = HorizontalAlignment.Center;
            root.Children.Add(hint);

            card.Child = root;

            var shadowHost = new Grid();
            shadowHost.Margin = new Thickness(14);
            shadowHost.Children.Add(card);

            var menu = new Window();
            menu.WindowStyle = WindowStyle.None;
            menu.ResizeMode = ResizeMode.NoResize;
            menu.AllowsTransparency = true;
            menu.Background = Brushes.Transparent;
            menu.ShowInTaskbar = false;
            menu.Topmost = true;
            menu.SizeToContent = SizeToContent.WidthAndHeight;
            menu.WindowStartupLocation = WindowStartupLocation.Manual;
            menu.ShowActivated = true;
            menu.Content = shadowHost;
            menu.Opacity = 0;
            menu.Deactivated += (s, e) =>
            {
                if (ReferenceEquals(current, menu)) CloseCurrent();
            };
            menu.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    e.Handled = true;
                    CloseCurrent();
                }
            };
            menu.Closed += (s, e) =>
            {
                if (ReferenceEquals(current, menu)) current = null;
            };

            current = menu;
            menu.Show();
            menu.UpdateLayout();
            Position(menu, screenX, screenY);

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(110));
            menu.BeginAnimation(Window.OpacityProperty, fade);
            menu.Activate();
            menu.Focus();
        }

        private static UIElement BuildHeader(List<DeviceReading> readings, DeviceReading primary)
        {
            var header = new Grid();
            header.Margin = new Thickness(9, 8, 9, 6);
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Color color = primary != null ? TrayIconRenderer.ColorFor(primary) : Theme.MutedColor;
            var dot = new Ellipse();
            dot.Width = 8;
            dot.Height = 8;
            dot.Fill = new SolidColorBrush(color);
            dot.VerticalAlignment = VerticalAlignment.Center;
            dot.Margin = new Thickness(2, 0, 9, 0);
            header.Children.Add(dot);

            var texts = new StackPanel();
            var title = Text("GearPower", 12.5, Theme.Text, FontWeights.SemiBold);
            var subtitleText = readings.Count + " 台设备";
            if (primary != null && primary.Percent.HasValue)
                subtitleText += " · 优先 " + primary.Percent.Value + "%";
            var subtitle = Text(subtitleText, 9.5, Theme.Muted, FontWeights.Normal);
            subtitle.Margin = new Thickness(0, 2, 0, 0);
            texts.Children.Add(title);
            texts.Children.Add(subtitle);
            Grid.SetColumn(texts, 1);
            header.Children.Add(texts);
            return header;
        }

        private static UIElement BuildDeviceRow(DeviceReading reading, DeviceReading primary, int count, Action open)
        {
            Color color = TrayIconRenderer.ColorFor(reading);
            var row = new Border();
            row.CornerRadius = new CornerRadius(9);
            row.Padding = new Thickness(8, 6, 8, 6);
            row.Margin = new Thickness(1, 1, 1, 1);
            row.Background = Brushes.Transparent;
            row.Cursor = Cursors.Hand;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBox = new Border();
            iconBox.Width = 24;
            iconBox.Height = 24;
            iconBox.CornerRadius = new CornerRadius(12);
            iconBox.Background = Theme.WithAlpha(color, 28);
            iconBox.VerticalAlignment = VerticalAlignment.Center;
            var dot = new Ellipse();
            dot.Width = 8;
            dot.Height = 8;
            dot.Fill = new SolidColorBrush(color);
            iconBox.Child = dot;
            grid.Children.Add(iconBox);

            var texts = new StackPanel();
            texts.VerticalAlignment = VerticalAlignment.Center;
            texts.Margin = new Thickness(2, 0, 8, 0);

            var nameRow = new StackPanel();
            nameRow.Orientation = Orientation.Horizontal;
            var name = Text(reading.Name, 11.5, Theme.Text, FontWeights.Normal);
            name.TextTrimming = TextTrimming.CharacterEllipsis;
            name.MaxWidth = 150;
            nameRow.Children.Add(name);

            bool lowest = count > 1 && primary != null && primary.Percent.HasValue &&
                          ReferenceEquals(reading, primary);
            if (lowest)
            {
                var badge = new Border();
                badge.Background = Theme.AccentDim;
                badge.CornerRadius = new CornerRadius(4);
                badge.Padding = new Thickness(4, 1, 4, 1);
                badge.Margin = new Thickness(6, 0, 0, 0);
                badge.VerticalAlignment = VerticalAlignment.Center;
                badge.Child = Text("优先", 8.5, Theme.Accent, FontWeights.SemiBold);
                nameRow.Children.Add(badge);
            }
            texts.Children.Add(nameRow);

            string status;
            if (reading.FromCache) status = "上次读数";
            else if (reading.Asleep) status = "设备休眠";
            else if (!string.IsNullOrEmpty(reading.Protocol)) status = reading.Protocol;
            else status = reading.Transport;
            var statusText = Text(status, 9.2, Theme.Muted, FontWeights.Normal);
            statusText.Margin = new Thickness(0, 2, 0, 0);
            statusText.TextTrimming = TextTrimming.CharacterEllipsis;
            statusText.MaxWidth = 185;
            texts.Children.Add(statusText);
            Grid.SetColumn(texts, 1);
            grid.Children.Add(texts);

            var values = new StackPanel();
            values.VerticalAlignment = VerticalAlignment.Center;
            values.HorizontalAlignment = HorizontalAlignment.Right;
            string value = reading.Percent.HasValue
                ? (reading.FromCache ? "上次 " : "") + reading.Percent.Value + "%"
                : (reading.Asleep ? "休眠" : "—");
            Brush valueBrush = reading.Percent.HasValue
                ? new SolidColorBrush(color)
                : Theme.Muted;
            var valueText = Text(value, 12, valueBrush, FontWeights.SemiBold);
            valueText.HorizontalAlignment = HorizontalAlignment.Right;
            values.Children.Add(valueText);

            string sub;
            if (reading.Charging) sub = "充电中";
            else if (reading.FromCache) sub = "上次";
            else if (reading.Asleep) sub = "休眠";
            else sub = "";
            if (!string.IsNullOrEmpty(sub))
            {
                var subText = Text(sub, 8.8, Theme.Muted, FontWeights.Normal);
                subText.HorizontalAlignment = HorizontalAlignment.Right;
                subText.Margin = new Thickness(0, 2, 0, 0);
                values.Children.Add(subText);
            }
            Grid.SetColumn(values, 2);
            grid.Children.Add(values);

            row.Child = grid;
            row.MouseEnter += (s, e) => row.Background = Theme.GhostHover;
            row.MouseLeave += (s, e) => row.Background = Brushes.Transparent;
            row.MouseLeftButtonUp += (s, e) => Run(open);
            return row;
        }

        private static UIElement BuildAction(string glyph, string label, Action action, bool danger)
        {
            var row = new Border();
            row.CornerRadius = new CornerRadius(8);
            row.Padding = new Thickness(8, 6, 8, 6);
            row.Margin = new Thickness(1, 1, 1, 1);
            row.Background = Brushes.Transparent;
            row.Cursor = Cursors.Hand;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(26) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var icon = new TextBlock();
            icon.Text = glyph;
            icon.FontFamily = Theme.IconFont;
            icon.FontSize = 12.5;
            icon.Foreground = danger ? Theme.Danger : Theme.Muted;
            icon.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(icon);

            var text = Text(label, 11, danger ? Theme.Danger : Theme.Text, FontWeights.Normal);
            text.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(text, 1);
            grid.Children.Add(text);

            row.Child = grid;
            row.MouseEnter += (s, e) =>
                row.Background = danger ? Theme.WithAlpha(Theme.DangerColor, 20) : Theme.GhostHover;
            row.MouseLeave += (s, e) => row.Background = Brushes.Transparent;
            row.MouseLeftButtonUp += (s, e) => Run(action);
            return row;
        }

        private static Border BuildSeparator()
        {
            var separator = new Border();
            separator.Height = 1;
            separator.Background = Theme.PanelBorder;
            separator.Margin = new Thickness(9, 5, 9, 5);
            separator.Opacity = 0.72;
            return separator;
        }

        private static TextBlock Text(string value, double size, Brush brush, FontWeight weight)
        {
            var text = new TextBlock();
            text.Text = value;
            text.FontFamily = Theme.UiFont;
            text.FontSize = size;
            text.Foreground = brush;
            text.FontWeight = weight;
            text.VerticalAlignment = VerticalAlignment.Center;
            return text;
        }

        private static void Run(Action action)
        {
            CloseCurrent();
            if (action != null) action();
        }

        private static void Position(Window menu, int screenX, int screenY)
        {
            try
            {
                Drawing.Rectangle work = WinForms.Screen.FromPoint(new Drawing.Point(screenX, screenY)).WorkingArea;
                DpiScale dpi = VisualTreeHelper.GetDpi(menu);
                double scale = dpi.DpiScaleX <= 0 ? 1 : dpi.DpiScaleX;
                double x = screenX / scale;
                double y = screenY / scale;
                double width = menu.ActualWidth;
                double height = menu.ActualHeight;
                double left = work.Left / scale;
                double top = work.Top / scale;
                double right = work.Right / scale;
                double bottom = work.Bottom / scale;

                if (x + width > right - 6) x = right - width - 6;
                if (x < left + 6) x = left + 6;
                if (y + height > bottom - 6) y = y - height - 6;
                else y += 6;
                if (y < top + 6) y = top + 6;

                menu.Left = x;
                menu.Top = y;
            }
            catch
            {
            }
        }

        private static void CloseCurrent()
        {
            Window menu = current;
            current = null;
            if (menu == null) return;
            try { menu.Close(); } catch { }
        }
    }
}
