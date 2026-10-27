using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.Win32;

namespace GearPower
{
    /// <summary>
    /// One-time orientation shown after the first scan. The guide is skipped for
    /// existing installs that already have history/cache data, and can be reopened
    /// from Settings.
    /// </summary>
    public static class FirstRunGuide
    {
        private const string KeyPath = @"Software\GearPower";
        private const string ValueName = "GuideCompleted";

        private sealed class Page
        {
            public string Glyph;
            public string Title;
            public string Subtitle;
            public string[] Bullets;
        }

        private static readonly Page[] Pages =
        {
            new Page
            {
                Glyph = "\uE83F",
                Title = "欢迎使用 GearPower",
                Subtitle = "无线外设电量，一屏看清",
                Bullets = new[]
                {
                    "插上 2.4G 接收器或连接蓝牙设备，电量卡片会自动出现。",
                    "支持键盘、鼠标、耳机、手柄、麦克风等设备。",
                    "所有读取都在本机完成，不上传设备数据。"
                }
            },
            new Page
            {
                Glyph = "\uE7F4",
                Title = "电量卡片与历史",
                Subtitle = "设备休眠、关机也不会马上消失",
                Bullets = new[]
                {
                    "环形电量、百分比和充电/低电量状态一眼可见。",
                    "设备休眠时保留上次读数，并标注「上次读数」。",
                    "连接过的设备会保留历史卡片，未连接卡片可以单独删除。"
                }
            },
            new Page
            {
                Glyph = "\uE7F8",
                Title = "托盘与任务栏",
                Subtitle = "不打开主窗口也能看电量",
                Bullets = new[]
                {
                    "托盘使用 iPhone 风格电池；鼠标悬停可查看全部设备。",
                    "任务栏左侧电量条会优先显示未充电设备，全屏时自动隐藏。",
                    "首页右上角的「蓝牙」开关可随时关闭蓝牙设备电量。"
                }
            },
            new Page
            {
                Glyph = "\uE70F",
                Title = "按你的习惯使用",
                Subtitle = "侧栏导航、重命名和设备适配",
                Bullets = new[]
                {
                    "卡片右上角铅笔或右键菜单可以重命名设备。",
                    "设置页集中管理刷新频率、托盘、启动和任务栏工具。",
                    "遇到新设备，可在「设备适配」页添加 JSON 配置。"
                }
            }
        };

        public static bool ShouldShow()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(KeyPath))
                {
                    if (key != null && key.GetValue(ValueName) is int && (int)key.GetValue(ValueName) != 0)
                        return false;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Guide state read failed: " + ex.Message);
            }
            // Upgrades already have history/cache. Do not interrupt those users.
            try
            {
                string root = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GearPower");
                if (File.Exists(Path.Combine(root, "device-history.tsv")) ||
                    File.Exists(Path.Combine(root, "battery-cache.tsv")))
                {
                    MarkDone();
                    return false;
                }
            }
            catch
            {
            }
            return true;
        }

        public static void ShowIfNeeded(Window owner)
        {
            if (ShouldShow()) Show(owner);
        }

        public static void MarkDone()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyPath))
                {
                    if (key != null) key.SetValue(ValueName, 1, RegistryValueKind.DWord);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Guide state save failed: " + ex.Message);
            }
        }

        public static void Show(Window owner)
        {
            int index = 0;
            bool finished = false;

            var card = new Border();
            card.Width = 680;
            card.Background = Theme.Panel;
            card.BorderBrush = Theme.CardBorder;
            card.BorderThickness = new Thickness(1);
            card.CornerRadius = new CornerRadius(28);
            card.Padding = new Thickness(32);
            card.Effect = new DropShadowEffect
            {
                BlurRadius = 38,
                ShadowDepth = 14,
                Direction = 45,
                Opacity = 0.25,
                Color = Color.FromRgb(0xD6, 0xC8, 0xBD)
            };
            TextOptions.SetTextFormattingMode(card, TextFormattingMode.Display);

            var root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new StackPanel();
            header.Orientation = Orientation.Horizontal;
            var iconBorder = new Border();
            iconBorder.Width = 54;
            iconBorder.Height = 54;
            iconBorder.CornerRadius = new CornerRadius(16);
            iconBorder.Background = Theme.WithAlpha(Theme.AccentColor, 30);
            iconBorder.BorderBrush = Theme.WithAlpha(Theme.AccentColor, 65);
            iconBorder.BorderThickness = new Thickness(1);
            var icon = new TextBlock();
            icon.FontFamily = Theme.IconFont;
            icon.FontSize = 25;
            icon.Foreground = Theme.Accent;
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            icon.VerticalAlignment = VerticalAlignment.Center;
            iconBorder.Child = icon;
            header.Children.Add(iconBorder);

            var heading = new StackPanel();
            heading.Margin = new Thickness(15, 0, 0, 0);
            heading.VerticalAlignment = VerticalAlignment.Center;
            var title = new TextBlock();
            title.FontFamily = Theme.UiFont;
            title.FontSize = 24;
            title.FontWeight = FontWeights.SemiBold;
            title.Foreground = Theme.Text;
            var subtitle = new TextBlock();
            subtitle.FontFamily = Theme.UiFont;
            subtitle.FontSize = 11.5;
            subtitle.Foreground = Theme.Secondary;
            subtitle.Margin = new Thickness(0, 5, 0, 0);
            heading.Children.Add(title);
            heading.Children.Add(subtitle);
            header.Children.Add(heading);
            root.Children.Add(header);

            var body = new StackPanel();
            body.Margin = new Thickness(0, 22, 0, 12);
            Grid.SetRow(body, 1);
            root.Children.Add(body);

            var footer = new Grid();
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            var dots = new StackPanel();
            dots.Orientation = Orientation.Horizontal;
            dots.VerticalAlignment = VerticalAlignment.Center;
            footer.Children.Add(dots);

            var actions = new StackPanel();
            actions.Orientation = Orientation.Horizontal;
            actions.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(actions, 2);
            footer.Children.Add(actions);

            var skip = MakeButton(owner, "跳过引导", "GhostButton");
            skip.Foreground = Theme.Muted;
            skip.Click += (s, e) => CloseGuide(owner, finished);
            actions.Children.Add(skip);

            var back = MakeButton(owner, "上一步", "GhostButton");
            back.Margin = new Thickness(8, 0, 0, 0);
            actions.Children.Add(back);

            var next = MakeButton(owner, "下一步", "PrimaryButton");
            next.Margin = new Thickness(8, 0, 0, 0);
            next.FontWeight = FontWeights.SemiBold;
            actions.Children.Add(next);

            Action render = () =>
            {
                Page page = Pages[index];
                icon.Text = page.Glyph;
                title.Text = page.Title;
                subtitle.Text = page.Subtitle;
                body.Children.Clear();
                foreach (string text in page.Bullets)
                {
                    var row = new Grid();
                    row.Margin = new Thickness(0, 0, 0, 13);
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    var dot = new TextBlock();
                    dot.Text = "•";
                    dot.FontSize = 16;
                    dot.Foreground = Theme.Accent;
                    dot.VerticalAlignment = VerticalAlignment.Top;
                    row.Children.Add(dot);
                    var textBlock = new TextBlock();
                    textBlock.Text = text;
                    textBlock.FontFamily = Theme.UiFont;
                    textBlock.FontSize = 12;
                    textBlock.Foreground = Theme.Text;
                    textBlock.TextWrapping = TextWrapping.Wrap;
                    textBlock.LineHeight = 21;
                    Grid.SetColumn(textBlock, 1);
                    row.Children.Add(textBlock);
                    body.Children.Add(row);
                }

                dots.Children.Clear();
                for (int i = 0; i < Pages.Length; i++)
                {
                    var dot = new Border();
                    dot.Width = i == index ? 18 : 6;
                    dot.Height = 6;
                    dot.CornerRadius = new CornerRadius(3);
                    dot.Margin = new Thickness(0, 0, 5, 0);
                    dot.Background = i == index ? Theme.Accent : Theme.PanelBorder;
                    dots.Children.Add(dot);
                }
                back.Visibility = index == 0 ? Visibility.Collapsed : Visibility.Visible;
                next.Content = index == Pages.Length - 1 ? "开始使用" : "下一步";
            };

            back.Click += (s, e) =>
            {
                if (index > 0)
                {
                    index--;
                    render();
                }
            };
            next.Click += (s, e) =>
            {
                if (index < Pages.Length - 1)
                {
                    index++;
                    render();
                }
                else
                {
                    finished = true;
                    MarkDone();
                    CloseGuide(owner, true);
                }
            };
            render();
            card.Child = root;

            var host = new Grid();
            host.Margin = new Thickness(20);
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
                    CloseGuide(owner, finished);
                }
                else if (e.Key == Key.Right && index < Pages.Length - 1)
                {
                    index++;
                    render();
                }
                else if (e.Key == Key.Left && index > 0)
                {
                    index--;
                    render();
                }
            };
            dialog.Closed += (s, e) => MarkDone();
            owner.Tag = dialog;
            dialog.ShowDialog();
            owner.Tag = null;
        }

        private static void CloseGuide(Window owner, bool finished)
        {
            if (!finished) MarkDone();
            var dialog = owner.Tag as Window;
            if (dialog != null) dialog.Close();
        }

        private static Button MakeButton(Window owner, string text, string styleName)
        {
            var button = new Button();
            button.Content = text;
            button.FontFamily = Theme.UiFont;
            button.FontSize = 10.5;
            button.Padding = new Thickness(14, 7, 14, 7);
            Style style = owner.TryFindResource(styleName) as Style;
            if (style != null) button.Style = style;
            return button;
        }
    }
}
