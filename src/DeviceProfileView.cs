using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace GearPower
{
    public sealed class DeviceProfileView : UserControl, IDisposable
    {
        private sealed class DeviceChoice
        {
            public InventoryRow Row;
            public override string ToString()
            {
                return Row.VidPid + "  " + Row.Usage + "  " + Row.Name;
            }
        }

        private readonly Window owner;
        private readonly Func<List<InventoryRow>> inventoryProvider;
        private readonly Action rescan;
        private readonly bool demo;
        private readonly ListBox profiles = new ListBox();
        private readonly ComboBox devices = new ComboBox();
        private readonly TextBox editor = new TextBox();
        private readonly TextBox output = new TextBox();
        private readonly TextBlock status = new TextBlock();
        private readonly List<Button> actionButtons = new List<Button>();
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private bool testing;
        private bool disposed;
        private string selectedText = "";

        public DeviceProfileView(Window owner, Func<List<InventoryRow>> inventoryProvider, Action rescan, bool demo)
        {
            this.owner = owner;
            this.inventoryProvider = inventoryProvider;
            this.rescan = rescan;
            this.demo = demo;
            SetResourceReference(ForegroundProperty, "TextPrimaryBrush");
            FontFamily = Theme.UiFont;

            var root = new DockPanel();
            root.Margin = new Thickness(24, 20, 24, 20);
            Content = root;

            var heading = new StackPanel();
            heading.Margin = new Thickness(0, 0, 0, 12);
            DockPanel.SetDock(heading, Dock.Top);
            root.Children.Add(heading);
            var kicker = new TextBlock();
            kicker.Text = "PROFILES / LOCAL";
            kicker.FontFamily = Theme.LabelFont;
            kicker.FontSize = 9;
            kicker.SetResourceReference(TextBlock.ForegroundProperty, "TextMutedBrush");
            heading.Children.Add(kicker);
            var title = new TextBlock();
            title.Text = "设备适配";
            title.FontSize = 24;
            title.FontWeight = FontWeights.SemiBold;
            title.Margin = new Thickness(0, 4, 0, 0);
            title.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");
            heading.Children.Add(title);
            var subtitle = new TextBlock();
            subtitle.Text = "从接口生成草稿 → 填写查询和字段 → 测试 → 导入启用。导入后无需重新编译。";
            subtitle.FontSize = 11;
            subtitle.SetResourceReference(TextBlock.ForegroundProperty, "TextMutedBrush");
            subtitle.TextWrapping = TextWrapping.Wrap;
            subtitle.Margin = new Thickness(0, 4, 0, 0);
            heading.Children.Add(subtitle);

            var bottom = new StackPanel();
            bottom.Margin = new Thickness(0, 10, 0, 0);
            DockPanel.SetDock(bottom, Dock.Bottom);
            root.Children.Add(bottom);
            var buttons = new WrapPanel();
            bottom.Children.Add(buttons);
            AddButton(buttons, "导入文件", ImportFile);
            AddButton(buttons, "测试当前配置", TestEditor);
            AddButton(buttons, "导入并启用", delegate { ImportEditor(); });
            AddButton(buttons, "导出 JSON", ExportEditor);
            AddButton(buttons, "移除用户配置", RemoveSelected);
            AddButton(buttons, "打开配置目录", delegate { OpenDirectory(); });
            AddButton(buttons, "配置说明", delegate { OpenHelp(); });
            AddButton(buttons, "刷新配置", delegate { RefreshProfiles(); });
            status.FontSize = 10;
            status.SetResourceReference(TextBlock.ForegroundProperty, "TextMutedBrush");
            status.TextWrapping = TextWrapping.Wrap;
            status.Margin = new Thickness(0, 7, 0, 0);
            bottom.Children.Add(status);

            var grid = new Grid();
            root.Children.Add(grid);
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(230) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var left = new DockPanel();
            grid.Children.Add(left);
            var leftTitle = new TextBlock();
            leftTitle.Text = "已加载的配置";
            leftTitle.FontSize = 12;
            leftTitle.FontWeight = FontWeights.SemiBold;
            leftTitle.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");
            leftTitle.Margin = new Thickness(0, 0, 0, 7);
            DockPanel.SetDock(leftTitle, Dock.Top);
            left.Children.Add(leftTitle);
            profiles.SetResourceReference(BackgroundProperty, "InputBrush");
            profiles.SetResourceReference(ForegroundProperty, "TextPrimaryBrush");
            profiles.SetResourceReference(BorderBrushProperty, "InputBorderBrush");
            profiles.BorderThickness = new Thickness(1);
            profiles.FontSize = 11;
            profiles.SelectionChanged += delegate
            {
                DeviceProfile selected = profiles.SelectedItem as DeviceProfile;
                if (selected != null)
                {
                    editor.Text = ProfileJson.Pretty(selected);
                    selectedText = editor.Text;
                }
            };
            left.Children.Add(profiles);

            var right = new Grid();
            Grid.SetColumn(right, 2);
            grid.Children.Add(right);
            right.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            right.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 100 });
            right.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8) });
            right.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.55, GridUnitType.Star), MinHeight = 70 });

            var generate = new DockPanel();
            generate.Margin = new Thickness(0, 0, 0, 8);
            right.Children.Add(generate);
            Button draft = MakeButton("从接口生成草稿");
            draft.Click += GenerateDraft;
            DockPanel.SetDock(draft, Dock.Right);
            generate.Children.Add(draft);
            devices.SetResourceReference(BackgroundProperty, "InputBrush");
            devices.SetResourceReference(ForegroundProperty, "TextPrimaryBrush");
            devices.SetResourceReference(BorderBrushProperty, "InputBorderBrush");
            devices.BorderThickness = new Thickness(1);
            devices.FontSize = 11;
            devices.Margin = new Thickness(0, 0, 8, 0);
            devices.MinWidth = 160;
            generate.Children.Add(devices);

            ConfigureBox(editor);
            editor.AcceptsReturn = true;
            editor.AcceptsTab = true;
            editor.MaxLength = ProfileJson.MaxBytes;
            editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Grid.SetRow(editor, 1);
            right.Children.Add(editor);

            var splitter = new GridSplitter();
            splitter.Height = 6;
            splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            splitter.VerticalAlignment = VerticalAlignment.Center;
            splitter.ResizeDirection = GridResizeDirection.Rows;
            splitter.ResizeBehavior = GridResizeBehavior.PreviousAndNext;
            splitter.Background = Theme.InputBorder;
            splitter.SetResourceReference(BackgroundProperty, "InputBorderBrush");
            Grid.SetRow(splitter, 2);
            right.Children.Add(splitter);

            ConfigureBox(output);
            output.IsReadOnly = true;
            output.AcceptsReturn = true;
            output.TextWrapping = TextWrapping.Wrap;
            Grid.SetRow(output, 3);
            right.Children.Add(output);

            RefreshProfiles();
        }

        public void RefreshProfiles()
        {
            if (testing || disposed) return;
            DeviceProfile selected = profiles.SelectedItem as DeviceProfile;
            string id = selected != null ? selected.id : null;
            string current = editor.Text;
            bool modified = current.Length > 0 && current != selectedText;
            List<string> errors;
            List<DeviceProfile> loaded = ProfileStore.Load(out errors);
            profiles.ItemsSource = loaded;
            profiles.SelectedItem = loaded.FirstOrDefault(p => p.id == id) ?? loaded.FirstOrDefault();
            status.Text = loaded.Count + " 份配置 · 用户版本优先于同 id 的随附配置。";
            if (errors.Count > 0) status.Text += "  " + errors.Count + " 份未加载，详见输出。";
            if (errors.Count > 0) output.Text = "以下配置未加载：\n" + string.Join("\n", errors.Take(6));
            if (modified) editor.Text = current;
            ReloadDevices();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            lifetime.Cancel();
            lifetime.Dispose();
        }

        private void ReloadDevices()
        {
            if (disposed) return;
            string selected = devices.SelectedItem is DeviceChoice
                ? ((DeviceChoice)devices.SelectedItem).Row.Path : null;
            List<InventoryRow> rows = inventoryProvider != null ? inventoryProvider() : new List<InventoryRow>();
            List<DeviceChoice> choices = rows
                .Where(row => row.UsagePage >= 0xFF00 && (row.InputLength > 0 || row.OutputLength > 0 || row.FeatureLength > 0))
                .Select(row => new DeviceChoice { Row = row }).ToList();
            devices.ItemsSource = choices;
            DeviceChoice chosen = choices.FirstOrDefault(choice => choice.Row.Path == selected);
            devices.SelectedItem = chosen ?? (choices.Count > 0 ? choices[0] : null);
        }

        private void ConfigureBox(TextBox box)
        {
            box.FontFamily = new FontFamily("Consolas");
            box.FontSize = 11.5;
            box.SetResourceReference(BackgroundProperty, "InputBrush");
            box.SetResourceReference(ForegroundProperty, "TextPrimaryBrush");
            box.SetResourceReference(BorderBrushProperty, "InputBorderBrush");
            box.BorderThickness = new Thickness(1);
            box.Padding = new Thickness(9);
            box.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        private Button MakeButton(string text)
        {
            var button = new Button();
            button.Content = text;
            button.FontSize = 10;
            button.Height = 32;
            button.Padding = new Thickness(10, 4, 10, 4);
            button.Margin = new Thickness(0, 0, 6, 6);
            try
            {
                Style style = owner != null ? owner.TryFindResource("NeoButton") as Style : null;
                if (style != null) button.Style = style;
            }
            catch
            {
            }
            actionButtons.Add(button);
            return button;
        }

        private void AddButton(Panel parent, string text, RoutedEventHandler handler)
        {
            Button button = MakeButton(text);
            button.Click += handler;
            parent.Children.Add(button);
        }

        private void GenerateDraft(object sender, RoutedEventArgs e)
        {
            DeviceChoice choice = devices.SelectedItem as DeviceChoice;
            if (choice == null)
            {
                output.Text = "请先选择一个厂商 HID 接口。";
                return;
            }
            InventoryRow row = choice.Row;
            bool feature = row.OutputLength == 0 && row.FeatureLength > 0;
            var draft = new DeviceProfile();
            draft.id = "custom-" + row.Vid.ToString("x4") + "-" + row.Pid.ToString("x4") + "-" +
                Guid.NewGuid().ToString("N").Substring(0, 6);
            draft.name = string.IsNullOrEmpty(row.Name) ? "我的设备 " + row.VidPid : row.Name;
            draft.vendor = row.Vendor;
            draft.kind = row.KindId;
            draft.matches = new ProfileMatch[]
            {
                new ProfileMatch
                {
                    vid = row.Vid.ToString("X4"),
                    pid = row.Pid.ToString("X4"),
                    usagePage = row.UsagePage.ToString("X4"),
                    usage = row.UsageValue.ToString("X4"),
                    inputLength = row.InputLength,
                    outputLength = row.OutputLength,
                    featureLength = row.FeatureLength
                }
            };
            draft.steps = new ProfileStep[]
            {
                new ProfileStep
                {
                    id = "battery",
                    method = feature ? "feature" : row.OutputLength > 0 ? "output" : "input",
                    reportId = 0,
                    response = new ProfileResponse
                    {
                        length = feature ? row.FeatureLength : row.InputLength,
                        prefix = "00"
                    }
                }
            };
            draft.result = new ProfileResult
            {
                percent = new ProfilePercent { step = "battery", offset = -1 },
                note = "通过设备配置读取"
            };
            profiles.SelectedIndex = -1;
            editor.Text = ProfileJson.Pretty(draft);
            output.Text = "草稿尚未启用。VID/PID、Usage 和长度已填好；请按协议填写 request、response.prefix、percent.offset 和校验规则。\n"
                + "偏移从 0 开始，包含 Report ID；offset: -1 是待填写标记。";
        }

        private void ImportFile(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "导入设备配置";
            dialog.Filter = "设备配置 (*.json)|*.json";
            if (dialog.ShowDialog(owner) != true) return;
            try
            {
                if (new FileInfo(dialog.FileName).Length > ProfileJson.MaxBytes)
                    throw new FormatException("配置文件超过 128 KB。");
                DeviceProfile profile = ProfileJson.Parse(File.ReadAllText(dialog.FileName, Encoding.UTF8));
                editor.Text = ProfileJson.Pretty(profile);
                ImportEditor();
            }
            catch (Exception ex)
            {
                output.Text = "导入失败：" + ex.Message;
            }
        }

        private void ImportEditor()
        {
            try
            {
                DeviceProfile profile = ProfileJson.Parse(editor.Text);
                string path = ProfileStore.UserPath(profile.id);
                if (File.Exists(path) && MessageBox.Show(owner,
                    "替换已导入的配置 “" + profile.id + "”？", "替换设备配置",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                profile.enabled = true;
                ProfileStore.Import(ProfileJson.Serialize(profile));
                RefreshProfiles();
                output.Text = "已导入并启用：" + profile.name + "\n已触发重新扫描。";
                if (rescan != null) rescan();
            }
            catch (Exception ex)
            {
                output.Text = "配置未启用：" + ex.Message;
            }
        }

        private void ExportEditor(object sender, RoutedEventArgs e)
        {
            try
            {
                DeviceProfile profile = ProfileJson.Parse(editor.Text);
                var dialog = new SaveFileDialog();
                dialog.Title = "导出设备配置";
                dialog.Filter = "设备配置 (*.json)|*.json";
                dialog.FileName = profile.id + ".json";
                dialog.AddExtension = true;
                if (dialog.ShowDialog(owner) != true) return;
                File.WriteAllText(dialog.FileName, ProfileJson.Pretty(profile), Encoding.UTF8);
                output.Text = "已导出：" + dialog.FileName;
            }
            catch (Exception ex)
            {
                output.Text = "导出失败：" + ex.Message;
            }
        }

        private void RemoveSelected(object sender, RoutedEventArgs e)
        {
            DeviceProfile profile = profiles.SelectedItem as DeviceProfile;
            if (profile == null || !profile.UserFile)
            {
                output.Text = "请选择一份用户配置；随附配置不会在这里删除。";
                return;
            }
            if (MessageBox.Show(owner, "移除用户配置 “" + profile.id + "”？", "移除设备配置",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            try
            {
                ProfileStore.Remove(profile.id);
                RefreshProfiles();
                output.Text = "已移除用户配置。";
                if (rescan != null) rescan();
            }
            catch (Exception ex)
            {
                output.Text = "移除失败：" + ex.Message;
            }
        }

        private async void TestEditor(object sender, RoutedEventArgs e)
        {
            if (testing || disposed) return;
            if (demo)
            {
                output.Text = "演示模式不访问硬件。";
                return;
            }
            DeviceProfile profile;
            try
            {
                profile = ProfileJson.Parse(editor.Text);
            }
            catch (Exception ex)
            {
                output.Text = "配置校验失败：" + ex.Message;
                return;
            }

            testing = true;
            foreach (Button button in actionButtons) button.IsEnabled = false;
            profiles.IsEnabled = devices.IsEnabled = editor.IsEnabled = false;
            output.Text = "正在按当前配置读取设备…";
            try
            {
                string result = await Task.Run(() => ProfileRunner.TestProfile(profile), lifetime.Token);
                if (!disposed) output.Text = result;
            }
            catch (OperationCanceledException)
            {
                if (!disposed) output.Text = "已取消设备测试。";
            }
            catch (Exception ex)
            {
                if (!disposed) output.Text = "测试失败：" + ex.Message;
            }
            finally
            {
                testing = false;
                if (!disposed)
                {
                    foreach (Button button in actionButtons) button.IsEnabled = true;
                    profiles.IsEnabled = devices.IsEnabled = editor.IsEnabled = true;
                }
            }
        }

        private void OpenDirectory()
        {
            try
            {
                Directory.CreateDirectory(ProfileStore.UserDirectory);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ProfileStore.UserDirectory,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                output.Text = ex.Message;
            }
        }

        private void OpenHelp()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DEVICE_PROFILES.md");
            try
            {
                if (!File.Exists(path))
                {
                    output.Text = "请查看程序目录中的 DEVICE_PROFILES.md。";
                    return;
                }
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                output.Text = ex.Message;
            }
        }
    }
}
