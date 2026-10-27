using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;
using Drawing = System.Drawing;
using WinForms = System.Windows.Forms;

namespace GearPower
{
    public sealed class UiController
    {
        private readonly Window window;
        private readonly bool demoMode;

        private readonly TextBlock summaryText;
        private readonly TextBlock summaryDetail;
        private readonly TextBlock scanTimeText;
        private readonly Ellipse summaryDot;
        private readonly WrapPanel cardPanel;
        private readonly ScrollViewer cardScroll;
        private readonly StackPanel emptyState;
        private readonly Button emptyRescan;
        private readonly Border inventoryView;
        private readonly StackPanel inventoryPanel;
        private readonly TextBlock inventoryCount;
        private readonly Button copyDiagButton;
        private readonly Button deviceRequestButton;
        private readonly Button batteryTab;
        private readonly Button deviceTab;
        private readonly Button settingsTab;
        private readonly Button refreshButton;
        private readonly Border settingsView;
        private readonly Border profileView;
        private readonly Grid profileHost;
        private readonly Button profileTab;
        private DeviceProfileView profileController;
        private readonly Button themeSystemOption;
        private readonly Button themeLightOption;
        private readonly Button themeDarkOption;
        private readonly Button accentGreenOption;
        private readonly Button accentPinkOption;
        private readonly Button accentYellowOption;
        private readonly Button accentBlueOption;
        private readonly Button refresh30Option;
        private readonly Button refresh60Option;
        private readonly Button refresh300Option;
        private readonly Button refreshOffOption;
        private readonly ToggleButton trayEnabledSwitch;
        private readonly ToggleButton hoverEnabledSwitch;
        private readonly ToggleButton bluetoothEnabledSwitch;
        private readonly ToggleButton autoStartSwitch;
        private readonly Button guideButton;
        private readonly Button closeAskOption;
        private readonly Button closeTrayOption;
        private readonly Button closeExitOption;
        private readonly ToggleButton taskbarEnabledSwitch;
        private readonly ToggleButton taskbarContrastSwitch;
        private readonly Button taskbarNormalColorButton;
        private readonly Button taskbarWarnColorButton;
        private readonly Button taskbarDangerColorButton;
        private readonly Button taskbarColorResetButton;
        private readonly TextBlock taskbarStatusText;
        private readonly Button settingsElevateButton;
        private readonly Button minButton;
        private readonly Button maxButton;
        private readonly Button closeButton;
        private readonly TextBlock refreshGlyph;
        private readonly TextBlock footerStatus;
        private readonly TextBlock footerTime;
        private readonly Ellipse footerDot;
        private readonly Border demoBadge;
        private readonly Border toast;
        private readonly TextBlock toastText;

        private readonly DispatcherTimer refreshTimer;
        private readonly DispatcherTimer toastTimer;
        private readonly DispatcherTimer themeTimer;
        private TrayController tray;
        private bool exiting;
        private bool restarting;
        private readonly bool lowIntegrity;
        private HwndSource hwndSource;
        private int refreshSeconds = 60;
        private bool trayEnabled = true;
        private bool hoverEnabled = true;
        private int closeAction;
        private int taskbarMode;
        private bool taskbarHighContrast = true;
        private Color taskbarNormalColor = Color.FromRgb(0x4A, 0xDE, 0x80);
        private Color taskbarWarnColor = Color.FromRgb(0xFB, 0xBF, 0x24);
        private Color taskbarDangerColor = Color.FromRgb(0xF8, 0x71, 0x71);
        private TaskbarWidgetController taskbarWidget;
        private bool scanning;
        private bool showingDevices;
        private bool showingSettings;
        private bool adjustingCards;
        private DateTime lastUpdate = DateTime.MinValue;
        private List<DeviceReading> lastReadings = new List<DeviceReading>();
        private List<DeviceReading> lastConnectedReadings = new List<DeviceReading>();
        private int lastHidCount;
        private List<InventoryRow> lastInventory = new List<InventoryRow>();

        public UiController(Window window, bool demo)
        {
            this.window = window;
            this.demoMode = demo;
            window.MinWidth = 980;
            window.MinHeight = 620;
            window.SizeChanged += Window_SizeChanged;
            lowIntegrity = ProcessIntegrity.IsLow;
            Log.Info("Process integrity: " + ProcessIntegrity.LevelName);

            ThemeManager.Initialize(window);

            summaryText = Find<TextBlock>("SummaryText");
            summaryDetail = Find<TextBlock>("SummaryDetail");
            scanTimeText = Find<TextBlock>("ScanTimeText");
            summaryDot = Find<Ellipse>("SummaryDot");
            cardPanel = Find<WrapPanel>("CardPanel");
            cardScroll = Find<ScrollViewer>("CardScroll");
            cardScroll.SizeChanged += (s, e) => UpdateCardLayout();
            emptyState = Find<StackPanel>("EmptyState");
            emptyRescan = Find<Button>("EmptyRescan");
            inventoryView = Find<Border>("InventoryView");
            inventoryPanel = Find<StackPanel>("InventoryPanel");
            inventoryCount = Find<TextBlock>("InventoryCount");
            copyDiagButton = Find<Button>("CopyDiagButton");
            deviceRequestButton = Find<Button>("DeviceRequestButton");
            batteryTab = Find<Button>("BatteryTab");
            deviceTab = Find<Button>("DeviceTab");
            profileTab = Find<Button>("ProfileTab");
            settingsTab = Find<Button>("SettingsTab");
            settingsView = Find<Border>("SettingsView");
            profileView = Find<Border>("ProfileView");
            profileHost = Find<Grid>("ProfileHost");
            themeSystemOption = Find<Button>("ThemeSystemOption");
            themeLightOption = Find<Button>("ThemeLightOption");
            themeDarkOption = Find<Button>("ThemeDarkOption");
            accentGreenOption = Find<Button>("AccentGreenOption");
            accentPinkOption = Find<Button>("AccentPinkOption");
            accentYellowOption = Find<Button>("AccentYellowOption");
            accentBlueOption = Find<Button>("AccentBlueOption");
            refresh30Option = Find<Button>("Refresh30Option");
            refresh60Option = Find<Button>("Refresh60Option");
            refresh300Option = Find<Button>("Refresh300Option");
            refreshOffOption = Find<Button>("RefreshOffOption");
            trayEnabledSwitch = Find<ToggleButton>("TrayEnabledSwitch");
            hoverEnabledSwitch = Find<ToggleButton>("HoverEnabledSwitch");
            bluetoothEnabledSwitch = Find<ToggleButton>("BluetoothEnabledSwitch");
            autoStartSwitch = Find<ToggleButton>("AutoStartSwitch");
            guideButton = Find<Button>("GuideButton");
            closeAskOption = Find<Button>("CloseAskOption");
            closeTrayOption = Find<Button>("CloseTrayOption");
            closeExitOption = Find<Button>("CloseExitOption");
            taskbarEnabledSwitch = Find<ToggleButton>("TaskbarEnabledSwitch");
            taskbarContrastSwitch = Find<ToggleButton>("TaskbarContrastSwitch");
            taskbarNormalColorButton = Find<Button>("TaskbarNormalColorButton");
            taskbarWarnColorButton = Find<Button>("TaskbarWarnColorButton");
            taskbarDangerColorButton = Find<Button>("TaskbarDangerColorButton");
            taskbarColorResetButton = Find<Button>("TaskbarColorResetButton");
            taskbarStatusText = Find<TextBlock>("TaskbarStatusText");
            settingsElevateButton = Find<Button>("SettingsElevateButton");
            refreshButton = Find<Button>("RefreshButton");
            minButton = Find<Button>("MinButton");
            maxButton = Find<Button>("MaxButton");
            closeButton = Find<Button>("CloseButton");
            refreshGlyph = Find<TextBlock>("RefreshGlyph");
            footerStatus = Find<TextBlock>("FooterStatus");
            footerTime = Find<TextBlock>("FooterTime");
            footerDot = Find<Ellipse>("FooterDot");
            demoBadge = Find<Border>("DemoBadge");
            toast = Find<Border>("Toast");
            toastText = Find<TextBlock>("ToastText");

            refreshTimer = new DispatcherTimer();
            refreshTimer.Tick += (s, e) => { if (!scanning) Scan(); };
            toastTimer = new DispatcherTimer();
            toastTimer.Interval = TimeSpan.FromSeconds(2.2);
            toastTimer.Tick += (s, e) => { toastTimer.Stop(); HideToast(); };
            themeTimer = new DispatcherTimer();
            themeTimer.Interval = TimeSpan.FromSeconds(2);
            themeTimer.Tick += (s, e) => ThemeManager.RefreshSystem();

            LoadSettings();

            refreshButton.Click += (s, e) => Scan();
            emptyRescan.Click += (s, e) => Scan();
            minButton.Click += (s, e) => window.WindowState = WindowState.Minimized;
            maxButton.Click += (s, e) => ToggleMaximize();
            closeButton.Click += (s, e) => window.Close();
            window.StateChanged += (s, e) => UpdateMaximizeGlyph();
            window.Closing += Window_Closing;
            copyDiagButton.Click += (s, e) => CopyDiagnostics();
            deviceRequestButton.Click += (s, e) => OpenDeviceRequest();
            Find<Button>("SettingsLogButton").Click += (s, e) => OpenLog();
            Find<Button>("SettingsLiveLogButton").Click += (s, e) => LogWindow.Show(window);
            Find<Button>("SettingsTrayHelpButton").Click += (s, e) => ShowTrayHelp();
            Find<Button>("SettingsFolderButton").Click += (s, e) => OpenExeFolder();
            Find<Button>("SettingsElevateButton").Click += (s, e) => { RestartAsAdministrator(); };
            Find<Button>("SettingsExitButton").Click += (s, e) => ExitApplication();
            batteryTab.Click += (s, e) => ShowTab(false);
            deviceTab.Click += (s, e) => ShowTab(true);
            profileTab.Click += (s, e) => ShowProfile();
            settingsTab.Click += (s, e) => ShowSettings();
            themeSystemOption.Click += (s, e) => SetThemeMode(ThemeMode.System);
            themeLightOption.Click += (s, e) => SetThemeMode(ThemeMode.Light);
            themeDarkOption.Click += (s, e) => SetThemeMode(ThemeMode.Dark);
            accentGreenOption.Click += (s, e) => SetAccentPreset(AccentPreset.Green);
            accentPinkOption.Click += (s, e) => SetAccentPreset(AccentPreset.Pink);
            accentYellowOption.Click += (s, e) => SetAccentPreset(AccentPreset.Yellow);
            accentBlueOption.Click += (s, e) => SetAccentPreset(AccentPreset.Blue);
            refresh30Option.Click += (s, e) => SetRefreshSeconds(30);
            refresh60Option.Click += (s, e) => SetRefreshSeconds(60);
            refresh300Option.Click += (s, e) => SetRefreshSeconds(300);
            refreshOffOption.Click += (s, e) => SetRefreshSeconds(0);
            trayEnabledSwitch.Click += (s, e) => SetTrayEnabled(trayEnabledSwitch.IsChecked == true);
            hoverEnabledSwitch.Click += (s, e) => SetHoverEnabled(hoverEnabledSwitch.IsChecked == true);
            bluetoothEnabledSwitch.Click += (s, e) => SetBluetoothEnabled(bluetoothEnabledSwitch.IsChecked == true);
            autoStartSwitch.Click += (s, e) => SetAutoStart(autoStartSwitch.IsChecked == true);
            guideButton.Click += (s, e) => FirstRunGuide.Show(window);
            closeAskOption.Click += (s, e) => SetCloseAction(0);
            closeTrayOption.Click += (s, e) => SetCloseAction(1);
            closeExitOption.Click += (s, e) => SetCloseAction(2);
            taskbarEnabledSwitch.Click += (s, e) => SetTaskbarMode(taskbarEnabledSwitch.IsChecked == true ? 1 : 0);
            taskbarNormalColorButton.Click += (s, e) => PickTaskbarColor(0);
            taskbarWarnColorButton.Click += (s, e) => PickTaskbarColor(1);
            taskbarDangerColorButton.Click += (s, e) => PickTaskbarColor(2);
            taskbarColorResetButton.Click += (s, e) => ResetTaskbarColors();
            taskbarContrastSwitch.Click += (s, e) => ToggleTaskbarContrast();
            ThemeManager.Changed += (s, e) => OnThemeChanged();
            window.KeyDown += Window_KeyDown;
            window.Loaded += Window_Loaded;
            window.SourceInitialized += (s, e) =>
            {
                try
                {
                    // During a UAC restart / rapid recreate WPF can raise
                    // SourceInitialized while the HWND is already gone.
                    // HwndSource.FromHwnd(IntPtr.Zero) throws
                    // "The Hwnd of a Window cannot be zero", so guard it.
                    IntPtr hwnd = new WindowInteropHelper(window).Handle;
                    if (hwnd == IntPtr.Zero)
                    {
                        Log.Debug("SourceInitialized with zero HWND; skipping hook");
                        return;
                    }
                    hwndSource = HwndSource.FromHwnd(hwnd);
                    if (hwndSource != null) hwndSource.AddHook(WndProc);
                }
                catch (Exception ex)
                {
                    Log.Error("Window source initialization failed: " + ex.Message);
                }
            };
            window.Closed += (s, e) =>
            {
                if (hwndSource != null)
                {
                    try { hwndSource.RemoveHook(WndProc); } catch { }
                    hwndSource = null;
                }
                refreshTimer.Stop();
                toastTimer.Stop();
                themeTimer.Stop();
                if (tray != null)
                {
                    tray.Dispose();
                    tray = null;
                }
                if (taskbarWidget != null)
                {
                    taskbarWidget.Dispose();
                    taskbarWidget = null;
                }
                if (profileController != null)
                {
                    profileController.Dispose();
                    profileController = null;
                }
                if (!demoMode) BatteryCache.Save();
            };

            demoBadge.Visibility = demo ? Visibility.Visible : Visibility.Collapsed;
            profileController = new DeviceProfileView(window, () => new List<InventoryRow>(lastInventory), Scan, demo);
            if (profileHost != null) profileHost.Children.Add(profileController);
            ShowTab(false);
            UpdateMaximizeGlyph();
            RefreshSettingsState();

            themeTimer.Start();
            if (demo)
            {
                footerStatus.Text = "演示模式 · 示例数据";
                footerDot.Fill = Theme.Warn;
            }
        }

        private T Find<T>(string name) where T : class
        {
            return window.FindName(name) as T;
        }

        private void CreateTray()
        {
            if (tray != null || !trayEnabled || lowIntegrity) return;
            try
            {
                tray = new TrayController();
                tray.HoverEnabled = hoverEnabled;
                tray.OpenRequested += (s, e) => ShowMainWindow();
                tray.MenuRequested += (s, e) => ShowTrayMenu(e.ScreenX, e.ScreenY, e.Readings);
                tray.Update(lastConnectedReadings);
                tray.EnsureVisible();
            }
            catch (Exception ex)
            {
                Log.Error("Tray icon creation failed: " + ex);
                tray = null;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (window.WindowState != WindowState.Normal) return;
            if (window.ActualWidth < 979.5) window.Width = 980;
            if (window.ActualHeight < 619.5) window.Height = 620;
            UpdateCardLayout();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThemeManager.ApplyTitleBar();
            if (lowIntegrity)
            {
                if (!demoMode) footerStatus.Text = "低权限运行 · 托盘不可用";
                if (!demoMode)
                {
                    bool elevate = ElevationPromptWindow.Show(window);
                    if (elevate && RestartAsAdministrator()) return;
                }
            }
            else
            {
                if (trayEnabled) CreateTray();
                else if (!demoMode) footerStatus.Text = "托盘已关闭 · 关闭窗口将退出";
            }
            if (tray != null)
            {
                tray.EnsureVisible();
                var hintTimer = new DispatcherTimer();
                hintTimer.Interval = TimeSpan.FromMilliseconds(800);
                hintTimer.Tick += (s, args) =>
                {
                    hintTimer.Stop();
                    if (tray != null)
                    {
                        tray.ShowStartupHint();
                        ShowToast("托盘后台已启用 · 若任务栏没有图标，请点 ^ 拖出");
                    }
                    else
                    {
                        ShowToast("托盘创建失败：关闭窗口将直接退出");
                    }
                };
                hintTimer.Start();
            }
            if (taskbarMode != 0)
            {
                if (taskbarWidget == null) taskbarWidget = new TaskbarWidgetController(SetTaskbarStatus);
                ApplyTaskbarColors();
                taskbarWidget.Update(lastConnectedReadings);
                taskbarWidget.SetMode(taskbarMode);
            }
            bool showGuide = !demoMode && FirstRunGuide.ShouldShow();
            if (demoMode)
            {
                ApplyResult(BuildDemoResult());
                return;
            }
            Scan();
            if (refreshSeconds > 0) refreshTimer.Start();
            if (showGuide)
            {
                var guideTimer = new DispatcherTimer();
                guideTimer.Interval = TimeSpan.FromMilliseconds(1200);
                guideTimer.Tick += (s, args) =>
                {
                    guideTimer.Stop();
                    FirstRunGuide.Show(window);
                };
                guideTimer.Start();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                Scan();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && (showingDevices || showingSettings ||
                (profileView != null && profileView.Visibility == Visibility.Visible)))
            {
                ShowTab(false);
                e.Handled = true;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_GETMINMAXINFO = 0x0024;
            const int WM_SIZE = 0x0005;
            if (msg == WM_SIZE && wParam.ToInt32() != 1 && wParam.ToInt32() != 2)
            {
                double dpi = 1.0;
                PresentationSource source = PresentationSource.FromVisual(window);
                if (source != null && source.CompositionTarget != null)
                    dpi = source.CompositionTarget.TransformToDevice.M11;
                int minWidth = (int)Math.Ceiling(980 * dpi);
                int minHeight = (int)Math.Ceiling(620 * dpi);
                NativeMethods.Rect32 rect;
                if (NativeMethods.GetWindowRect(hwnd, out rect))
                {
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;
                    if (width < minWidth || height < minHeight)
                    {
                        NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, 0, 0,
                            Math.Max(width, minWidth), Math.Max(height, minHeight),
                            0x0002 | 0x0004 | 0x0010);
                    }
                }
            }
            if (msg == WM_GETMINMAXINFO)
            {
                IntPtr monitor = NativeMethods.MonitorFromWindow(hwnd, 2);
                var info = new NativeMethods.MonitorInfo();
                info.cbSize = Marshal.SizeOf(typeof(NativeMethods.MonitorInfo));
                if (NativeMethods.GetMonitorInfoW(monitor, ref info))
                {
                    var mmi = (NativeMethods.MinMaxInfo)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MinMaxInfo));
                    int width = info.rcWork.Right - info.rcWork.Left;
                    int height = info.rcWork.Bottom - info.rcWork.Top;
                    mmi.MaxPosition.X = info.rcWork.Left - info.rcMonitor.Left;
                    mmi.MaxPosition.Y = info.rcWork.Top - info.rcMonitor.Top;
                    mmi.MaxSize.X = width;
                    mmi.MaxSize.Y = height;
                    mmi.MaxTrackSize.X = width;
                    mmi.MaxTrackSize.Y = height;
                    double scale = 1.0;
                    PresentationSource source = PresentationSource.FromVisual(window);
                    if (source != null && source.CompositionTarget != null)
                        scale = source.CompositionTarget.TransformToDevice.M11;
                    mmi.MinTrackSize.X = Math.Max(mmi.MinTrackSize.X, (int)Math.Ceiling(980 * scale));
                    mmi.MinTrackSize.Y = Math.Max(mmi.MinTrackSize.Y, (int)Math.Ceiling(620 * scale));
                    Marshal.StructureToPtr(mmi, lParam, false);
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void ToggleMaximize()
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void UpdateMaximizeGlyph()
        {
            bool maximized = window.WindowState == WindowState.Maximized;
            maxButton.Content = maximized ? "\uE923" : "\uE922";
            maxButton.ToolTip = maximized ? "还原" : "最大化";
        }


        private void ShowTrayMenu(int screenX, int screenY, List<DeviceReading> readings)
        {
            TrayMenuWindow.Show(screenX, screenY, readings, ShowMainWindow, Scan, ExitApplication);
        }

        public void ActivateFromSecondInstance()
        {
            if (window == null || exiting || restarting) return;
            try
            {
                window.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (exiting || restarting) return;
                    ShowMainWindow();
                }));
            }
            catch
            {
            }
        }

        private void ShowMainWindow()
        {
            if (window == null || exiting) return;
            try
            {
                window.Show();
                if (window.WindowState == WindowState.Minimized) window.WindowState = WindowState.Normal;
                window.Activate();
                window.Topmost = true;
                window.Topmost = false;
                window.Focus();
            }
            catch (Exception ex)
            {
                Log.Debug("Show main window failed: " + ex.Message);
            }
        }

        private void HideToTray()
        {
            window.Hide();
            if (tray != null) tray.ShowBalloonOnce();
            if (taskbarWidget != null) taskbarWidget.OnMainWindowHidden();
        }

        private bool RestartAsAdministrator()
        {
            restarting = true;
            bool released = SingleInstance.ReleaseForRestart();
            try
            {
                string exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var info = new ProcessStartInfo();
                info.FileName = exe;
                info.WorkingDirectory = System.IO.Path.GetDirectoryName(exe);
                info.UseShellExecute = true;
                info.Verb = "runas";

                string[] args = Environment.GetCommandLineArgs();
                var parts = new List<string>();
                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i];
                    if (arg.IndexOf(' ') >= 0) arg = "\"" + arg.Replace("\"", "\\\"") + "\"";
                    parts.Add(arg);
                }
                if (parts.Count > 0) info.Arguments = string.Join(" ", parts.ToArray());

                Process.Start(info);
                Log.Info("Restarted with administrator rights");
                ExitApplication();
                return true;
            }
            catch (Win32Exception ex)
            {
                restarting = false;
                if (released) SingleInstance.ReacquireAfterFailedRestart();
                if (ex.NativeErrorCode == 1223)
                {
                    Log.Info("Administrator restart canceled by user");
                    ShowToast("已取消管理员启动，继续低权限模式");
                }
                else
                {
                    Log.Error("Administrator restart failed: " + ex.Message);
                    ShowToast("无法以管理员身份重启：" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                restarting = false;
                if (released) SingleInstance.ReacquireAfterFailedRestart();
                Log.Error("Administrator restart failed: " + ex);
                ShowToast("无法以管理员身份重启");
            }
            return false;
        }

        private void ExitApplication()
        {
            if (exiting) return;
            exiting = true;
            try
            {
                if (tray != null)
                {
                    tray.Dispose();
                    tray = null;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Tray dispose during exit failed: " + ex.Message);
            }
            try
            {
                if (taskbarWidget != null)
                {
                    taskbarWidget.Dispose();
                    taskbarWidget = null;
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Taskbar dispose during exit failed: " + ex.Message);
            }
            try
            {
                if (window != null) window.Close();
            }
            catch (Exception ex)
            {
                Log.Debug("Window close during exit failed: " + ex.Message);
            }
            try
            {
                if (Application.Current != null) Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                Log.Debug("Application shutdown failed: " + ex.Message);
            }
        }

        public void PrepareSystemExit()
        {
            exiting = true;
            if (tray != null)
            {
                tray.Dispose();
                tray = null;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (exiting || tray == null || !trayEnabled)
            {
                if (tray != null)
                {
                    tray.Dispose();
                    tray = null;
                }
                return;
            }
            if (closeAction == 1)
            {
                e.Cancel = true;
                HideToTray();
                return;
            }
            if (closeAction == 2)
            {
                exiting = true;
                if (tray != null)
                {
                    tray.Dispose();
                    tray = null;
                }
                e.Cancel = false;
                return;
            }

            e.Cancel = true;
            bool remember;
            CloseChoice choice = CloseConfirmWindow.Show(window, out remember);
            if (choice == CloseChoice.Cancel) return;
            if (remember)
            {
                closeAction = choice == CloseChoice.Tray ? 1 : 2;
                SaveSettings();
                RefreshSettingsState();
            }
            if (choice == CloseChoice.Tray)
            {
                HideToTray();
            }
            else
            {
                exiting = true;
                if (tray != null)
                {
                    tray.Dispose();
                    tray = null;
                }
                e.Cancel = false;
            }
        }

        private void OnThemeChanged()
        {
            if (tray != null)
            {
                tray.Update(lastConnectedReadings);
            }
            if (taskbarWidget != null) taskbarWidget.RefreshTheme();
            if (showingSettings) ShowSettings();
            else ShowTab(showingDevices);
            RenderCards(lastReadings);
            RenderInventory(lastInventory);
            UpdateThemeDots();
            RefreshSettingsState();
        }

        private void SetThemeMode(ThemeMode mode)
        {
            ThemeManager.SetMode(mode);
            RefreshSettingsState();
            ShowToast(mode == ThemeMode.System ? "外观：跟随系统" :
                mode == ThemeMode.Light ? "外观：浅色" : "外观：深色");
        }

        private void SetAccentPreset(AccentPreset preset)
        {
            ThemeManager.SetAccent(preset);
            RefreshSettingsState();
            ShowToast("强调色：" + AccentLabel(preset));
        }

        private static string AccentLabel(AccentPreset preset)
        {
            switch (preset)
            {
                case AccentPreset.Pink: return "粉色";
                case AccentPreset.Yellow: return "黄色";
                case AccentPreset.Blue: return "蓝色";
                default: return "绿色";
            }
        }

        private static void SetOptionState(Button button, bool active)
        {
            ApplyOptionState(button, active);
        }

        private static void SetAccentOptionState(Button button, bool active)
        {
            ApplyOptionState(button, active);
        }

        private static void ApplyOptionState(Button button, bool active)
        {
            button.Background = active ? Theme.AccentDim : Theme.GhostHover;
            button.Foreground = active ? Theme.Accent : Theme.Secondary;
            button.FontWeight = active ? FontWeights.SemiBold : FontWeights.Normal;
            button.BorderBrush = active ? Theme.Accent : Theme.OptionBorder;
            button.BorderThickness = new Thickness(1);
        }

        private void UpdateThemeDots()
        {
            summaryDot.Fill = SummaryDotBrush();
            footerDot.Fill = lastConnectedReadings.Count == 0 ? Theme.Muted : Theme.Accent;
        }

        private Brush SummaryDotBrush()
        {
            if (lastConnectedReadings.Count == 0) return Theme.Muted;
            var withLevel = lastConnectedReadings.Where(r => r.Percent.HasValue).ToList();
            if (withLevel.Count > 1)
            {
                int min = withLevel.Min(r => r.Percent.Value);
                return min <= 20 ? Theme.Danger : min <= 40 ? Theme.Warn : Theme.Accent;
            }
            if (withLevel.Count == 1)
            {
                int only = withLevel[0].Percent.Value;
                return only <= 20 ? Theme.Danger : only <= 40 ? Theme.Warn : Theme.Accent;
            }
            return lastConnectedReadings.All(r => r.Asleep) ? Theme.Warn : Theme.Muted;
        }

        private void LoadSettings()
        {
            AutoStart.RepairIfEnabled();
            object value = ReadRefreshSeconds(@"Software\GearPower");
            if (value == null) value = ReadRefreshSeconds(@"Software\BatteryPeek");
            if (value is int)
            {
                int seconds = (int)value;
                if (seconds == 0 || seconds == 30 || seconds == 60 || seconds == 300)
                    refreshSeconds = seconds;
            }
            if (refreshSeconds > 0)
            {
                refreshTimer.Interval = TimeSpan.FromSeconds(refreshSeconds);
            }
            object trayValue = ReadSetting("TrayEnabled");
            if (trayValue is int) trayEnabled = (int)trayValue != 0;
            object hoverValue = ReadSetting("TrayHover");
            if (hoverValue is int) hoverEnabled = (int)hoverValue != 0;
            object closeValue = ReadSetting("CloseAction");
            if (closeValue is int)
            {
                int action = (int)closeValue;
                if (action >= 0 && action <= 2) closeAction = action;
            }
            if (lowIntegrity || !trayEnabled) closeAction = 2;
            object taskbarValue = ReadSetting("TaskbarMode");
            if (taskbarValue is int)
            {
                int mode = (int)taskbarValue;
                if (mode >= 0 && mode <= 1) taskbarMode = mode;
            }
            object normalColor = ReadSetting("TaskbarNormalColor");
            if (normalColor is int) taskbarNormalColor = FromArgb((int)normalColor);
            object warnColor = ReadSetting("TaskbarWarnColor");
            if (warnColor is int) taskbarWarnColor = FromArgb((int)warnColor);
            object dangerColor = ReadSetting("TaskbarDangerColor");
            if (dangerColor is int) taskbarDangerColor = FromArgb((int)dangerColor);
            object contrast = ReadSetting("TaskbarHighContrast");
            if (contrast is int) taskbarHighContrast = (int)contrast != 0;
            object bluetooth = ReadSetting("BluetoothBattery");
            if (bluetooth is int) DisplaySettings.BluetoothEnabled = (int)bluetooth != 0;
        }

        private static int ToArgb(Color color)
        {
            return (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
        }

        private static Color FromArgb(int argb)
        {
            unchecked
            {
                return Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
            }
        }

        private static object ReadSetting(string name)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\GearPower"))
                {
                    if (key != null) return key.GetValue(name);
                }
            }
            catch
            {
            }
            return null;
        }

        private static object ReadRefreshSeconds(string path)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(path))
                {
                    if (key != null) return key.GetValue("RefreshSeconds");
                }
            }
            catch
            {
            }
            return null;
        }

        private void SaveSettings()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\GearPower"))
                {
                    if (key != null)
                    {
                        key.SetValue("RefreshSeconds", refreshSeconds);
                        key.SetValue("TrayEnabled", trayEnabled ? 1 : 0);
                        key.SetValue("TrayHover", hoverEnabled ? 1 : 0);
                        key.SetValue("CloseAction", closeAction);
                        key.SetValue("TaskbarMode", taskbarMode);
                        key.SetValue("TaskbarNormalColor", ToArgb(taskbarNormalColor));
                        key.SetValue("TaskbarWarnColor", ToArgb(taskbarWarnColor));
                        key.SetValue("TaskbarDangerColor", ToArgb(taskbarDangerColor));
                        key.SetValue("BluetoothBattery", DisplaySettings.BluetoothEnabled ? 1 : 0);
                        key.SetValue("TaskbarHighContrast", taskbarHighContrast ? 1 : 0);
                    }
                }
            }
            catch
            {
            }
        }

        private void SetRefreshSeconds(int seconds)
        {
            refreshSeconds = seconds;
            SaveSettings();
            refreshTimer.Stop();
            if (refreshSeconds > 0)
            {
                refreshTimer.Interval = TimeSpan.FromSeconds(refreshSeconds);
                refreshTimer.Start();
            }
            RefreshSettingsState();
            UpdateFooterStatus();
            ShowToast(refreshSeconds == 0 ? "已关闭自动刷新" : "自动刷新：" + AutoLabel());
        }

        private void SetTrayEnabled(bool enabled)
        {
            if (lowIntegrity)
            {
                ShowToast("低权限运行，托盘不可用");
                return;
            }
            trayEnabled = enabled;
            if (!enabled && closeAction != 2) closeAction = 2;
            SaveSettings();
            if (!enabled)
            {
                if (tray != null)
                {
                    tray.Dispose();
                    tray = null;
                }
            }
            else
            {
                CreateTray();
            }
            RefreshSettingsState();
            UpdateFooterStatus();
            ShowToast(enabled ? "托盘图标已开启" : "托盘图标已关闭");
        }

        private void UpdateFooterStatus()
        {
            if (demoMode) return;
            string refreshText = refreshSeconds == 0 ? "自动刷新已关闭" : "自动刷新 " + AutoLabel();
            if (lowIntegrity) footerStatus.Text = "低权限运行 · 托盘不可用";
            else if (!trayEnabled) footerStatus.Text = "托盘已关闭 · " + refreshText;
            else footerStatus.Text = tray == null ? "托盘不可用 · " + refreshText : "托盘后台 · " + refreshText;
        }

        private void SetHoverEnabled(bool enabled)
        {
            hoverEnabled = enabled;
            SaveSettings();
            if (tray != null) tray.HoverEnabled = enabled;
            RefreshSettingsState();
            ShowToast(enabled ? "悬停卡片已开启" : "悬停卡片已关闭");
        }

        private void SetBluetoothEnabled(bool enabled)
        {
            DisplaySettings.BluetoothEnabled = enabled;
            SaveSettings();
            RefreshSettingsState();
            Scan();
            ShowToast(enabled ? "蓝牙设备电量显示已开启" : "蓝牙设备电量显示已关闭");
        }

        private void SetAutoStart(bool enabled)
        {
            if (!AutoStart.SetEnabled(enabled))
            {
                ShowToast("开机自动启动设置失败，请检查当前用户权限");
                RefreshSettingsState();
                return;
            }
            RefreshSettingsState();
            ShowToast(enabled ? "已开启开机自动启动" : "已关闭开机自动启动");
        }

        private void SetCloseAction(int action)
        {
            if (action == 1 && (!trayEnabled || lowIntegrity))
            {
                ShowToast("托盘不可用，无法选择关闭到托盘");
                return;
            }
            closeAction = action;
            SaveSettings();
            RefreshSettingsState();
            ShowToast(action == 0 ? "关闭窗口时：每次询问" :
                action == 1 ? "关闭窗口时：关闭到托盘" : "关闭窗口时：直接退出");
        }

        private void RefreshSettingsState()
        {
            if (themeSystemOption == null) return;
            SetOptionState(themeSystemOption, ThemeManager.Mode == ThemeMode.System);
            SetOptionState(themeLightOption, ThemeManager.Mode == ThemeMode.Light);
            SetOptionState(themeDarkOption, ThemeManager.Mode == ThemeMode.Dark);
            SetAccentOptionState(accentGreenOption, ThemeManager.Accent == AccentPreset.Green);
            SetAccentOptionState(accentPinkOption, ThemeManager.Accent == AccentPreset.Pink);
            SetAccentOptionState(accentYellowOption, ThemeManager.Accent == AccentPreset.Yellow);
            SetAccentOptionState(accentBlueOption, ThemeManager.Accent == AccentPreset.Blue);
            SetOptionState(refresh30Option, refreshSeconds == 30);
            SetOptionState(refresh60Option, refreshSeconds == 60);
            SetOptionState(refresh300Option, refreshSeconds == 300);
            SetOptionState(refreshOffOption, refreshSeconds == 0);

            if (lowIntegrity)
            {
                trayEnabledSwitch.IsChecked = false;
                trayEnabledSwitch.IsEnabled = false;
                trayEnabledSwitch.ToolTip = "低权限运行，托盘不可用";
            }
            else
            {
                trayEnabledSwitch.IsEnabled = true;
                trayEnabledSwitch.IsChecked = trayEnabled;
                trayEnabledSwitch.ToolTip = "显示或隐藏系统托盘电池图标";
            }
            bool hoverAvailable = trayEnabled && !lowIntegrity;
            hoverEnabledSwitch.IsEnabled = hoverAvailable;
            hoverEnabledSwitch.IsChecked = hoverEnabled && hoverAvailable;
            hoverEnabledSwitch.ToolTip = hoverAvailable ? "悬停显示电量卡片" : "需要先开启托盘图标";
            bluetoothEnabledSwitch.IsChecked = DisplaySettings.BluetoothEnabled;
            bluetoothEnabledSwitch.ToolTip = "读取并显示蓝牙耳机、键盘等设备的电量";
            autoStartSwitch.IsChecked = AutoStart.IsEnabled();
            autoStartSwitch.ToolTip = "登录 Windows 后正常显示主窗口、托盘和任务栏电量条";

            bool closeChoices = trayEnabled && !lowIntegrity;
            if (!closeChoices) closeAction = 2;
            SetOptionState(closeAskOption, closeAction == 0);
            SetOptionState(closeTrayOption, closeAction == 1);
            SetOptionState(closeExitOption, closeAction == 2);
            closeAskOption.IsEnabled = closeChoices;
            closeTrayOption.IsEnabled = closeChoices;
            closeExitOption.IsEnabled = true;

            taskbarEnabledSwitch.IsChecked = taskbarMode != 0;
            if (taskbarMode == 0) taskbarStatusText.Text = "已关闭";
            settingsElevateButton.Visibility = lowIntegrity ? Visibility.Visible : Visibility.Collapsed;
            RefreshTaskbarColorButtons();
        }

        private void SetTaskbarMode(int mode)
        {
            taskbarMode = mode;
            SaveSettings();
            if (taskbarWidget == null)
            {
                taskbarWidget = new TaskbarWidgetController(SetTaskbarStatus);
            }
            ApplyTaskbarColors();
            taskbarWidget.Update(lastConnectedReadings);
            taskbarWidget.SetMode(mode);
            RefreshSettingsState();
            ShowToast(mode == 0 ? "任务栏电量条已关闭" : "任务栏电量条：自动");
        }

        private void SetTaskbarStatus(string text)
        {
            if (taskbarStatusText != null) taskbarStatusText.Text = text;
        }

        private void ApplyTaskbarColors()
        {
            if (taskbarWidget != null)
            {
                taskbarWidget.SetHighContrast(taskbarHighContrast);
                taskbarWidget.SetColors(taskbarNormalColor, taskbarWarnColor, taskbarDangerColor);
            }
            RefreshTaskbarColorButtons();
        }

        private void RefreshTaskbarColorButtons()
        {
            if (taskbarNormalColorButton == null) return;
            StyleColorButton(taskbarNormalColorButton, taskbarNormalColor);
            StyleColorButton(taskbarWarnColorButton, taskbarWarnColor);
            StyleColorButton(taskbarDangerColorButton, taskbarDangerColor);
            taskbarContrastSwitch.IsChecked = taskbarHighContrast;
        }

        private void ToggleTaskbarContrast()
        {
            taskbarHighContrast = !taskbarHighContrast;
            SaveSettings();
            ApplyTaskbarColors();
            ShowToast(taskbarHighContrast ? "任务栏已切换为高对比度" : "任务栏已跟随主题");
        }

        private static void StyleColorButton(Button button, Color color)
        {
            button.Background = new SolidColorBrush(color);
            button.BorderBrush = new SolidColorBrush(DarkenColor(color, 0.78));
            button.BorderThickness = new Thickness(1);
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
            button.Foreground = luminance > 0.55 ? Brushes.Black : Brushes.White;
        }

        private static Color DarkenColor(Color color, double factor)
        {
            return Color.FromRgb(
                (byte)Math.Max(0, Math.Min(255, color.R * factor)),
                (byte)Math.Max(0, Math.Min(255, color.G * factor)),
                (byte)Math.Max(0, Math.Min(255, color.B * factor)));
        }

        private void PickTaskbarColor(int kind)
        {
            Color current = kind == 0 ? taskbarNormalColor : kind == 1 ? taskbarWarnColor : taskbarDangerColor;
            using (var dialog = new WinForms.ColorDialog())
            {
                dialog.FullOpen = true;
                dialog.AnyColor = true;
                dialog.Color = Drawing.Color.FromArgb(current.R, current.G, current.B);
                if (dialog.ShowDialog() != WinForms.DialogResult.OK) return;
                Color picked = Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);
                if (kind == 0) taskbarNormalColor = picked;
                else if (kind == 1) taskbarWarnColor = picked;
                else taskbarDangerColor = picked;
                SaveSettings();
                ApplyTaskbarColors();
                ShowToast(kind == 0 ? "任务栏正常电量颜色已更新" :
                    kind == 1 ? "任务栏偏低电量颜色已更新" : "任务栏低电量颜色已更新");
            }
        }

        private void ResetTaskbarColors()
        {
            taskbarNormalColor = Color.FromRgb(0x4A, 0xDE, 0x80);
            taskbarWarnColor = Color.FromRgb(0xFB, 0xBF, 0x24);
            taskbarDangerColor = Color.FromRgb(0xF8, 0x71, 0x71);
            SaveSettings();
            ApplyTaskbarColors();
            ShowToast("任务栏状态颜色已恢复默认");
        }

        private string AutoLabel()
        {
            if (refreshSeconds == 30) return "30s";
            if (refreshSeconds == 60) return "60s";
            if (refreshSeconds == 300) return "5m";
            return "关闭";
        }

        private void Scan()
        {
            if (scanning) return;
            scanning = true;
            refreshButton.IsEnabled = false;
            refreshGlyph.Text = "\uE895";
            summaryText.Text = "正在扫描设备…";
            summaryDetail.Text = "读取 HID 描述符与电量报告";
            summaryDot.Fill = Theme.Muted;
            footerStatus.Text = "正在扫描…";
            footerDot.Fill = Theme.Warn;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    return Scanner.Scan();
                }
                catch (Exception ex)
                {
                    Log.Error("Scan crashed: " + ex);
                    var failed = new ScanResult();
                    failed.Error = ex.Message;
                    return failed;
                }
            }).ContinueWith(t =>
            {
                window.Dispatcher.BeginInvoke(new Action(() =>
                {
                    ApplyResult(t.Result);
                    if (refreshSeconds > 0 && !refreshTimer.IsEnabled) refreshTimer.Start();
                }));
            });
        }

        private void ApplyResult(ScanResult result)
        {
            scanning = false;
            refreshButton.IsEnabled = true;
            refreshGlyph.Text = "\uE72C";

            if (result == null)
            {
                ShowToast("扫描失败");
                return;
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                summaryText.Text = "扫描失败";
                summaryDetail.Text = result.Error;
                summaryDot.Fill = Theme.Danger;
                footerStatus.Text = "扫描失败，详见日志";
                footerDot.Fill = Theme.Danger;
                ShowToast("扫描失败：" + result.Error);
                return;
            }

            lastUpdate = DateTime.Now;
            lastHidCount = result.HidCount;
            lastConnectedReadings = result.Readings;
            if (!demoMode)
            {
                foreach (DeviceReading reading in result.Readings)
                {
                    BatteryCache.Apply(reading);
                }
                BatteryCache.Save();
            }

            List<DeviceReading> display = result.Readings;
            if (!demoMode)
            {
                try
                {
                    display = DeviceHistory.Merge(result.Readings);
                }
                catch (Exception ex)
                {
                    Log.Error("Device history merge failed: " + ex);
                    display = result.Readings;
                }
            }

            if (!DisplaySettings.BluetoothEnabled)
                display = display.Where(r => !r.IsBluetooth).ToList();
            NameOverrides.Apply(display);

            lastReadings = display;
            lastInventory = result.Inventory;
            if (tray != null) tray.Update(result.Readings);
            if (taskbarWidget != null) taskbarWidget.Update(result.Readings);

            RenderCards(display);
            RenderInventory(result.Inventory);
            if (profileView != null && profileView.Visibility == Visibility.Visible && profileController != null)
                profileController.RefreshProfiles();
            UpdateSummary();

            scanTimeText.Text = string.Format(CultureInfo.InvariantCulture,
                "扫描 {0} 个接口 · {1:0.0}s", result.HidCount + result.BluetoothCount, result.Seconds);
            footerTime.Text = "上次更新 " + lastUpdate.ToString("HH:mm:ss");
            footerDot.Fill = result.Readings.Count == 0 ? Theme.Muted : Theme.Accent;
            if (!demoMode)
            {
                string refreshText = refreshSeconds == 0 ? "自动刷新已关闭" : "自动刷新 " + AutoLabel();
                if (lowIntegrity) footerStatus.Text = "低权限运行 · 托盘不可用";
                else if (!trayEnabled) footerStatus.Text = "托盘已关闭 · " + refreshText;
                else footerStatus.Text = tray == null ? "托盘不可用 · " + refreshText : "托盘后台 · " + refreshText;
            }
        }

        private void UpdateSummary()
        {
            int connectedCount = lastConnectedReadings.Count;
            int historyCount = Math.Max(0, lastReadings.Count - connectedCount);
            if (connectedCount == 0)
            {
                if (historyCount > 0)
                {
                    summaryText.Text = "当前没有设备连接";
                    summaryDetail.Text = "保留 " + historyCount + " 台历史设备";
                    summaryDot.Fill = Theme.Muted;
                }
                else
                {
                    summaryText.Text = "未发现电量设备";
                    summaryDetail.Text = lastHidCount == 0
                        ? "没有检测到 HID 设备，请检查接收器"
                        : "已扫描 " + lastHidCount + " 个 HID 接口，可在「设备清单」查看";
                    summaryDot.Fill = Theme.Muted;
                }
                return;
            }

            summaryText.Text = "已发现 " + connectedCount + " 台设备";
            int withLevel = lastConnectedReadings.Count(r => r.Percent.HasValue);
            int asleepCount = lastConnectedReadings.Count(r => r.Asleep);
            int staleCount = lastConnectedReadings.Count(r => r.FromCache);
            if (withLevel > 1)
            {
                int min = lastConnectedReadings.Where(r => r.Percent.HasValue).Min(r => r.Percent.Value);
                double avg = lastConnectedReadings.Where(r => r.Percent.HasValue).Average(r => r.Percent.Value);
                summaryDetail.Text = "最低 " + min + "% · 平均 " + Math.Round(avg) + "%";
                if (asleepCount > 0) summaryDetail.Text += " · " + asleepCount + " 台休眠";
                if (staleCount > 0) summaryDetail.Text += "（含上次读数）";
                summaryDot.Fill = min <= 20 ? Theme.Danger : min <= 40 ? Theme.Warn : Theme.Accent;
            }
            else if (withLevel == 1)
            {
                int only = lastConnectedReadings.Where(r => r.Percent.HasValue).First().Percent.Value;
                summaryDetail.Text = (staleCount >= withLevel ? "上次读数 " : "当前读数 ") + only + "%";
                if (asleepCount > 0) summaryDetail.Text += " · " + asleepCount + " 台休眠";
                if (staleCount > 0) summaryDetail.Text += "（上次读数）";
                summaryDot.Fill = only <= 20 ? Theme.Danger : only <= 40 ? Theme.Warn : Theme.Accent;
            }
            else if (asleepCount == connectedCount)
            {
                summaryDetail.Text = "接收器在线，设备均处于休眠";
                summaryDot.Fill = Theme.Warn;
            }
            else
            {
                summaryDetail.Text = "设备在线，但暂未读到电量";
                summaryDot.Fill = Theme.Warn;
            }
            if (historyCount > 0) summaryDetail.Text += " · " + historyCount + " 台历史设备";
        }

        private void RenderCards(List<DeviceReading> readings)
        {
            cardPanel.Children.Clear();
            int index = 0;
            foreach (DeviceReading reading in readings)
            {
                Border card = BuildCard(reading);
                cardPanel.Children.Add(card);
                card.Opacity = 0;
                var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(240));
                animation.BeginTime = TimeSpan.FromMilliseconds(index * 45);
                card.BeginAnimation(UIElement.OpacityProperty, animation);
                index++;
            }

            bool empty = readings.Count == 0;
            if (!showingDevices)
            {
                emptyState.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
                cardScroll.Visibility = empty ? Visibility.Collapsed : Visibility.Visible;
            }
            UpdateCardLayout();
            cardScroll.Dispatcher.BeginInvoke(new Action(UpdateCardLayout));
        }

        private void UpdateCardLayout()
        {
            if (adjustingCards || cardPanel == null || cardScroll == null) return;
            int count = cardPanel.Children.Count;
            if (count == 0) return;

            double available = cardScroll.ActualWidth;
            if (double.IsNaN(available) || available <= 0) available = cardScroll.ViewportWidth;
            if (double.IsNaN(available) || available <= 0) return;
            available -= 4;
            if (cardScroll.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                available -= SystemParameters.VerticalScrollBarWidth;

            const double gap = 16;
            const double minCardWidth = 275;
            const double maxCardWidth = 380;
            int maxColumns = Math.Min(3, count);
            int columns = 1;
            for (int candidate = maxColumns; candidate >= 1; candidate--)
            {
                double width = (available - gap * candidate) / candidate;
                if (width >= minCardWidth)
                {
                    columns = candidate;
                    break;
                }
            }

            double cardWidth = (available - gap * columns) / columns;
            if (cardWidth > maxCardWidth) cardWidth = maxCardWidth;
            if (cardWidth < 1) return;

            adjustingCards = true;
            try
            {
                foreach (UIElement child in cardPanel.Children)
                {
                    var card = child as Border;
                    if (card != null) card.Width = cardWidth;
                }
                cardPanel.HorizontalAlignment = count < 3 && cardWidth >= maxCardWidth
                    ? HorizontalAlignment.Center
                    : HorizontalAlignment.Left;
            }
            finally
            {
                adjustingCards = false;
            }
        }

        private Border BuildCard(DeviceReading reading)
        {
            bool disconnected = reading.Disconnected;
            Color accent = disconnected ? Theme.MutedColor : Theme.LevelColor(reading.Percent, reading.Charging);
            Brush accentBrush = disconnected ? Theme.Muted : Theme.LevelBrush(reading.Percent, reading.Charging);
            if (reading.FromCache)
            {
                Color level = disconnected ? Theme.MutedColor : Theme.LevelColor(reading.Percent, reading.Charging);
                var faded = new SolidColorBrush(Color.FromArgb(175, level.R, level.G, level.B));
                faded.Freeze();
                accentBrush = faded;
            }

            var card = new Border();
            card.Width = 330;
            card.Height = 196;
            card.CornerRadius = new CornerRadius(18);
            card.Background = Theme.CardBackground;
            card.BorderBrush = Theme.CardBorder;
            card.BorderThickness = new Thickness(1);
            card.Margin = new Thickness(0, 0, 16, 16);
            card.ToolTip = BuildCardTooltip(reading);
            ToolTipService.SetInitialShowDelay(card, 350);
            ToolTipService.SetShowDuration(card, 60000);

            Brush normalBorder = Theme.CardBorder;
            card.MouseEnter += (s, e) =>
            {
                card.Background = Theme.CardHoverBackground;
                card.BorderBrush = Theme.WithAlpha(accent, 110);
            };
            card.MouseLeave += (s, e) =>
            {
                card.Background = Theme.CardBackground;
                card.BorderBrush = normalBorder;
            };

            var root = new Grid();
            root.Margin = new Thickness(18, 15, 18, 13);
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header
            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(42) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBorder = new Border();
            iconBorder.Width = 40;
            iconBorder.Height = 40;
            iconBorder.CornerRadius = new CornerRadius(12);
            iconBorder.Background = Theme.WithAlpha(accent, 28);
            iconBorder.BorderBrush = Theme.WithAlpha(accent, 55);
            iconBorder.BorderThickness = new Thickness(1);
            var icon = new TextBlock();
            icon.Text = KindNames.Glyph(reading.Kind);
            icon.FontFamily = Theme.IconFont;
            icon.FontSize = 19;
            icon.Foreground = accentBrush;
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            icon.VerticalAlignment = VerticalAlignment.Center;
            iconBorder.Child = icon;
            header.Children.Add(iconBorder);

            var titleStack = new StackPanel();
            titleStack.Margin = new Thickness(11, 0, 6, 0);
            titleStack.VerticalAlignment = VerticalAlignment.Center;
            var name = new TextBlock();
            name.Text = reading.Name;
            name.FontSize = 14.5;
            name.FontWeight = FontWeights.SemiBold;
            name.TextTrimming = TextTrimming.CharacterEllipsis;
            name.ToolTip = reading.Name;
            var sub = new TextBlock();
            sub.Text = reading.Vendor + " · " + reading.Transport;
            sub.FontSize = 10;
            sub.Foreground = Theme.Muted;
            sub.Margin = new Thickness(0, 3, 0, 0);
            sub.TextTrimming = TextTrimming.CharacterEllipsis;
            titleStack.Children.Add(name);
            titleStack.Children.Add(sub);
            Grid.SetColumn(titleStack, 1);
            header.Children.Add(titleStack);

            var actions = new StackPanel();
            actions.Orientation = Orientation.Horizontal;
            actions.VerticalAlignment = VerticalAlignment.Center;
            actions.HorizontalAlignment = HorizontalAlignment.Right;
            Grid.SetColumn(actions, 2);
            header.Children.Add(actions);

            var rename = new Button();
            Style ghost = window.TryFindResource("GhostButton") as Style;
            if (ghost != null) rename.Style = ghost;
            rename.Content = "\uE70F";
            rename.FontFamily = Theme.IconFont;
            rename.FontSize = 11;
            rename.Width = 24;
            rename.Height = 24;
            rename.Padding = new Thickness(0);
            rename.Foreground = Theme.Muted;
            rename.BorderBrush = Theme.PanelBorder;
            rename.BorderThickness = new Thickness(1);
            rename.ToolTip = "重命名设备（只改显示名称）";
            System.Windows.Automation.AutomationProperties.SetName(rename, "重命名设备");
            rename.Click += (s, e) => RenameDevice(reading);
            actions.Children.Add(rename);

            if (disconnected)
            {
                if (lastConnectedReadings.Any(r => DeviceIdentity.CanConfirmMerge(reading, r)))
                {
                    var merge = new Button();
                    if (ghost != null) merge.Style = ghost;
                    merge.Content = "合并";
                    merge.Padding = new Thickness(6, 2, 6, 2);
                    merge.Margin = new Thickness(6, 0, 0, 0);
                    merge.Foreground = Theme.Muted;
                    merge.BorderBrush = Theme.PanelBorder;
                    merge.BorderThickness = new Thickness(1);
                    merge.ToolTip = "确认这条旧记录与某个当前设备属于同一物理设备";
                    merge.Click += (s, e) => ShowHistoryMergeMenu(reading, merge);
                    actions.Children.Add(merge);
                }
                var delete = new Button();
                if (ghost != null) delete.Style = ghost;
                delete.Content = "删除";
                delete.FontSize = 9;
                delete.Padding = new Thickness(7, 3, 7, 3);
                delete.Margin = new Thickness(6, 0, 0, 0);
                delete.Foreground = Theme.Muted;
                delete.BorderBrush = Theme.PanelBorder;
                delete.BorderThickness = new Thickness(1);
                delete.VerticalAlignment = VerticalAlignment.Center;
                delete.ToolTip = "从历史设备中删除这张卡片";
                delete.Click += (s, e) => DeleteHistoryCard(reading);
                actions.Children.Add(delete);
            }
            else
            {
                string badgeText = null;
                Color badgeColor = accent;
                if (reading.FromCache) { badgeText = null; }
                else if (reading.Charging) { badgeText = "充电中"; badgeColor = Theme.AccentColor; }
                else if (reading.Percent.HasValue && reading.Percent.Value <= 15) { badgeText = "电量偏低"; badgeColor = Theme.DangerColor; }
                if (badgeText != null)
                {
                    var badge = new Border();
                    badge.CornerRadius = new CornerRadius(6);
                    badge.Background = Theme.WithAlpha(badgeColor, 24);
                    badge.BorderBrush = Theme.WithAlpha(badgeColor, 50);
                    badge.BorderThickness = new Thickness(1);
                    badge.Padding = new Thickness(6, 2, 6, 2);
                    badge.VerticalAlignment = VerticalAlignment.Center;
                    badge.Margin = new Thickness(6, 0, 0, 0);
                    var badgeLabel = new TextBlock();
                    badgeLabel.Text = badgeText;
                    badgeLabel.FontSize = 9;
                    badgeLabel.Foreground = new SolidColorBrush(badgeColor);
                    badge.Child = badgeLabel;
                    actions.Children.Add(badge);
                }
            }
            root.Children.Add(header);

            // Body
            var body = new Grid();
            body.Margin = new Thickness(0, 7, 0, 0);
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(88) });
            body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Grid.SetRow(body, 1);

            var ringGrid = new Grid();
            ringGrid.Width = 86;
            ringGrid.Height = 86;
            ringGrid.HorizontalAlignment = HorizontalAlignment.Left;
            ringGrid.VerticalAlignment = VerticalAlignment.Center;

            var ring = new BatteryRing();
            ring.Percent = reading.Percent;
            ring.Accent = accent;
            ring.Charging = reading.Charging && !reading.FromCache;
            ring.Asleep = reading.Asleep;
            ring.Stale = reading.FromCache;
            ring.Width = 86;
            ring.Height = 86;
            ringGrid.Children.Add(ring);

            var center = new StackPanel();
            center.HorizontalAlignment = HorizontalAlignment.Center;
            center.VerticalAlignment = VerticalAlignment.Center;
            var number = new TextBlock();
            number.Text = reading.Percent.HasValue ? reading.Percent.Value.ToString(CultureInfo.InvariantCulture) : "—";
            number.FontFamily = Theme.NumberFont;
            number.FontSize = 24;
            number.FontWeight = FontWeights.SemiBold;
            number.Foreground = accentBrush;
            number.HorizontalAlignment = HorizontalAlignment.Center;
            var unit = new TextBlock();
            unit.Text = reading.Percent.HasValue ? "%" : "";
            unit.FontSize = 9;
            unit.Foreground = Theme.Muted;
            unit.HorizontalAlignment = HorizontalAlignment.Center;
            unit.Margin = new Thickness(0, -3, 0, 0);
            center.Children.Add(number);
            center.Children.Add(unit);
            ringGrid.Children.Add(center);
            body.Children.Add(ringGrid);

            var info = new StackPanel();
            info.Margin = new Thickness(13, 0, 0, 0);
            info.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(info, 1);

            var status = new TextBlock();
            status.Text = StatusText(reading);
            status.FontSize = 13;
            status.FontWeight = FontWeights.SemiBold;
            status.Foreground = reading.Asleep ? Theme.Secondary : accentBrush;
            var hint = new TextBlock();
            hint.Text = HintText(reading);
            hint.FontSize = 10;
            hint.Foreground = Theme.Secondary;
            hint.Margin = new Thickness(0, 4, 0, 0);
            hint.TextWrapping = TextWrapping.Wrap;
            hint.MaxWidth = 170;
            info.Children.Add(status);
            info.Children.Add(hint);

            var chip = new Border();
            chip.CornerRadius = new CornerRadius(6);
            chip.Background = Theme.Input;
            chip.BorderBrush = Theme.InputBorder;
            chip.BorderThickness = new Thickness(1);
            chip.Padding = new Thickness(6, 2, 6, 2);
            chip.HorizontalAlignment = HorizontalAlignment.Left;
            chip.Margin = new Thickness(0, 7, 0, 0);
            var chipText = new TextBlock();
            chipText.Text = reading.Protocol;
            chipText.FontSize = 8.5;
            chipText.Foreground = Theme.Muted;
            chip.Child = chipText;
            info.Children.Add(chip);
            body.Children.Add(info);
            root.Children.Add(body);

            // Footer
            var footer = new Grid();
            footer.Margin = new Thickness(0, 6, 0, 0);
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetRow(footer, 2);

            var updated = new TextBlock();
            updated.Text = reading.Disconnected
                ? "上次连接 " + LastSeenText(reading)
                : (reading.FromCache ? "上次 " : "更新于 ") + reading.Updated.ToString("HH:mm");
            updated.FontSize = 9;
            updated.Foreground = Theme.Muted;
            updated.VerticalAlignment = VerticalAlignment.Center;
            footer.Children.Add(updated);

            var transport = new TextBlock();
            transport.Text = reading.Transport;
            transport.FontSize = 9;
            transport.Foreground = Theme.Muted;
            transport.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(transport, 1);
            footer.Children.Add(transport);
            root.Children.Add(footer);

            card.Child = root;
            var menu = new ContextMenu();
            var renameItem = new MenuItem();
            renameItem.Header = "重命名设备…";
            renameItem.Click += (s, e) => RenameDevice(reading);
            menu.Items.Add(renameItem);
            if (disconnected)
            {
                menu.Items.Add(new Separator());
                var deleteItem = new MenuItem();
                deleteItem.Header = "从历史设备中删除";
                deleteItem.Click += (s, e) => DeleteHistoryCard(reading);
                menu.Items.Add(deleteItem);
            }
            card.ContextMenu = menu;
            return card;
        }

        private void RenameDevice(DeviceReading reading)
        {
            if (reading == null || demoMode) return;
            string renamed = RenameWindow.Show(window, reading.Name, reading.Id);
            if (renamed == null) return;
            if (renamed.Length == 0) NameOverrides.Remove(reading.Id);
            else NameOverrides.Set(reading.Id, renamed);

            NameOverrides.Apply(lastReadings);
            NameOverrides.Apply(lastConnectedReadings);
            RenderCards(lastReadings);
            if (tray != null) tray.Update(lastConnectedReadings);
            if (taskbarWidget != null) taskbarWidget.Update(lastConnectedReadings);
            ShowToast(renamed.Length == 0 ? "已恢复默认设备名称" : "设备已重命名为 " + renamed);
        }

        private void DeleteHistoryCard(DeviceReading reading)
        {
            if (reading == null || !reading.Disconnected) return;
            DeviceHistory.Remove(reading.Id);
            NameOverrides.Remove(reading.Id);
            lastReadings.RemoveAll(r => string.Equals(r.Id, reading.Id, StringComparison.OrdinalIgnoreCase));
            RenderCards(lastReadings);
            UpdateSummary();
            ShowToast("已从历史设备中删除 " + reading.Name);
        }

        private void ShowHistoryMergeMenu(DeviceReading history, Button button)
        {
            var candidates = lastConnectedReadings.Where(r => DeviceIdentity.CanConfirmMerge(history, r)).ToList();
            if (candidates.Count == 1) { MergeHistoryCard(history, candidates[0]); return; }
            var menu = new ContextMenu();
            foreach (DeviceReading candidate in candidates)
            {
                DeviceReading target = candidate;
                var item = new MenuItem();
                item.Header = target.Name + " · " + target.Id;
                item.Click += (s, e) => MergeHistoryCard(history, target);
                menu.Items.Add(item);
            }
            menu.PlacementTarget = button;
            menu.IsOpen = true;
        }

        private void MergeHistoryCard(DeviceReading history, DeviceReading connected)
        {
            if (demoMode || !lastConnectedReadings.Contains(connected)
                || lastConnectedReadings.Any(r => string.Equals(r.Id, history.Id, StringComparison.OrdinalIgnoreCase))) return;
            if (MessageBox.Show(window,
                "确认这条历史记录和当前连接的「" + connected.Name + "」是同一台设备？\n\n"
                + "确认后只保留当前设备卡片和最新电量，保留当前自定义名称；"
                + "如果当前没有自定义名称，则继承旧名称。原始历史文件会先备份。"
                + "\n\n如果是两台同型号设备，请选择“否”。",
                "合并同一设备的历史记录", MessageBoxButton.YesNo, MessageBoxImage.Question,
                MessageBoxResult.No) != MessageBoxResult.Yes) return;
            try
            {
                if (!DeviceHistory.MergeConfirmed(history.Id, connected)) return;
                lastReadings = DeviceHistory.Merge(lastConnectedReadings);
                if (!DisplaySettings.BluetoothEnabled) lastReadings = lastReadings.Where(r => !r.IsBluetooth).ToList();
                NameOverrides.Apply(lastReadings);
                RenderCards(lastReadings);
                UpdateSummary();
                if (tray != null) tray.Update(lastConnectedReadings);
                if (taskbarWidget != null) taskbarWidget.Update(lastConnectedReadings);
                ShowToast("已合并同一设备的历史记录");
            }
            catch (Exception ex)
            {
                Log.Error("History merge failed: " + ex);
                MessageBox.Show(window, "合并未完成：" + ex.Message, "历史设备", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string LastSeenText(DeviceReading reading)
        {
            return reading.Updated.Date == DateTime.Today
                ? reading.Updated.ToString("HH:mm")
                : reading.Updated.ToString("MM-dd HH:mm");
        }

        private static string StatusText(DeviceReading reading)
        {
            if (reading.Disconnected) return "未连接";
            if (reading.Asleep) return "设备休眠";
            if (reading.Charging) return "充电中";
            if (reading.Full) return "已充满";
            if (!reading.Percent.HasValue) return "未读到电量";
            if (reading.Percent.Value <= 15) return "电量偏低";
            return "正在使用";
        }

        private static string HintText(DeviceReading reading)
        {
            if (reading.Disconnected)
            {
                string hint = "最后连接 " + reading.Updated.ToString("yyyy-MM-dd HH:mm");
                if (reading.Percent.HasValue) hint += " · 上次电量 " + reading.Percent.Value + "%";
                return hint;
            }
            if (reading.Asleep)
            {
                if (reading.Percent.HasValue)
                {
                    return "上次读数 " + reading.Percent.Value + "% · " + reading.Updated.ToString("HH:mm");
                }
                return "接收器在线，设备未唤醒";
            }
            if (reading.Charging) return "已接入电源，正在补充电量";
            if (reading.Full) return "电池已充满，可随时使用";
            if (!reading.Percent.HasValue) return "设备已连接，但没有返回电量字段";
            if (reading.Percent.Value <= 15) return "建议尽快充电";
            if (reading.Percent.Value <= 35) return "电量不多，留意充电";
            return "无线连接正常";
        }

        private static string BuildCardTooltip(DeviceReading reading)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(reading.Name);
            sb.AppendLine(reading.Vendor + " · " + reading.Transport);
            sb.AppendLine("协议：" + reading.Protocol);
            string state = "状态：" + StatusText(reading);
            if (reading.Percent.HasValue)
            {
                if (reading.Disconnected)
                    state += "（最后连接 " + reading.Updated.ToString("yyyy-MM-dd HH:mm") + "，上次电量 " + reading.Percent.Value + "%）";
                else if (reading.FromCache)
                    state += "（上次读数 " + reading.Percent.Value + "%，" + reading.Updated.ToString("HH:mm") + "）";
                else
                    state += "（" + reading.Percent.Value + "%）";
            }
            sb.AppendLine(state);
            if (reading.VoltageMv.HasValue) sb.AppendLine("电压：" + reading.VoltageMv.Value + " mV");
            if (!string.IsNullOrEmpty(reading.Note)) sb.AppendLine("说明：" + reading.Note);
            sb.AppendLine((reading.Disconnected ? "最后连接：" : reading.FromCache ? "上次更新：" : "更新：") + reading.Updated.ToString("yyyy-MM-dd HH:mm:ss"));
            if (reading.Vid != 0)
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "VID:PID {0:X4}:{1:X4}", reading.Vid, reading.Pid));
            return sb.ToString().TrimEnd();
        }

        private void RenderInventory(List<InventoryRow> rows)
        {
            inventoryPanel.Children.Clear();
            List<InventoryCategory> categories = InventoryGroups.Build(rows);
            int deviceCount = categories.Sum(category => category.Devices.Count);
            inventoryCount.Text = "· " + rows.Count + " 个接口 · " + deviceCount + " 台设备";

            if (rows.Count == 0)
            {
                var empty = new TextBlock();
                empty.Text = "没有发现 HID 设备。请确认接收器已插好，或试试重新插拔。";
                empty.Foreground = Theme.Muted;
                empty.FontSize = 12;
                empty.Margin = new Thickness(12, 30, 0, 0);
                inventoryPanel.Children.Add(empty);
                return;
            }

            foreach (InventoryCategory category in categories)
            {
                var categoryHeader = new TextBlock();
                categoryHeader.Text = category.Name + "  ·  " + category.Devices.Count + " 台 / " + category.InterfaceCount + " 个接口";
                categoryHeader.FontSize = 10.5;
                categoryHeader.FontWeight = FontWeights.SemiBold;
                categoryHeader.Foreground = Theme.Muted;
                categoryHeader.Margin = new Thickness(8, 12, 8, 4);
                inventoryPanel.Children.Add(categoryHeader);

                int index = 0;
                foreach (InventoryDeviceGroup device in category.Devices)
                {
                    inventoryPanel.Children.Add(BuildInventoryGroup(device, index == 0));
                    index++;
                }
            }
        }

        private Border BuildInventoryGroup(InventoryDeviceGroup device, bool expanded)
        {
            var outer = new Border();
            outer.Margin = new Thickness(6, 2, 6, 2);
            outer.Background = Theme.Panel;
            outer.BorderBrush = Theme.RowBorder;
            outer.BorderThickness = new Thickness(1);
            outer.CornerRadius = new CornerRadius(10);
            outer.ClipToBounds = true;

            var root = new StackPanel();
            var header = new Border();
            header.Height = 52;
            header.Padding = new Thickness(10, 0, 8, 0);
            header.Background = device.Supported ? Theme.RowSupported : Brushes.Transparent;
            header.Cursor = Cursors.Hand;

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(18) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.2, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var chevron = new TextBlock();
            chevron.Text = expanded ? "\uE70E" : "\uE70D";
            chevron.FontFamily = Theme.IconFont;
            chevron.FontSize = 10;
            chevron.Foreground = Theme.Muted;
            chevron.VerticalAlignment = VerticalAlignment.Center;
            grid.Children.Add(chevron);

            var name = new TextBlock();
            name.Text = device.Name;
            name.FontSize = 12;
            name.FontWeight = FontWeights.SemiBold;
            name.Foreground = device.Supported ? Theme.Text : Theme.Secondary;
            name.TextTrimming = TextTrimming.CharacterEllipsis;
            name.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(name, 1);
            grid.Children.Add(name);

            var vid = new TextBlock();
            vid.Text = device.VidPid;
            vid.FontSize = 10;
            vid.Foreground = Theme.Muted;
            vid.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(vid, 2);
            grid.Children.Add(vid);

            var badge = new TextBlock();
            badge.Text = device.Supported ? "已支持" : "未适配";
            badge.FontSize = 9.5;
            badge.Foreground = device.Supported ? Theme.Accent : Theme.Muted;
            badge.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(badge, 3);
            grid.Children.Add(badge);

            var actions = new StackPanel();
            actions.Orientation = Orientation.Horizontal;
            actions.VerticalAlignment = VerticalAlignment.Center;
            Button agent = MakeInventoryAction("复制给 Agent");
            agent.Click += (s, e) =>
            {
                CopyText(DeviceAgentPrompt.Build(GroupDiagnostic(device)),
                    "已复制设备诊断和 Agent 适配提示词");
            };
            actions.Children.Add(agent);
            Button diagnostic = MakeInventoryAction("复制诊断");
            diagnostic.Margin = new Thickness(4, 0, 0, 0);
            diagnostic.Click += (s, e) => CopyText(GroupDiagnostic(device), "已复制该设备组的诊断信息");
            actions.Children.Add(diagnostic);
            Grid.SetColumn(actions, 4);
            grid.Children.Add(actions);

            header.Child = grid;
            root.Children.Add(header);

            var content = new StackPanel();
            content.Visibility = expanded ? Visibility.Visible : Visibility.Collapsed;
            foreach (InventoryRow row in device.Interfaces)
            {
                content.Children.Add(BuildInventoryRow(row));
            }
            root.Children.Add(content);

            header.MouseLeftButtonUp += (s, e) =>
            {
                bool visible = content.Visibility == Visibility.Visible;
                content.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
                chevron.Text = visible ? "\uE70D" : "\uE70E";
                e.Handled = true;
            };
            header.MouseEnter += (s, e) => header.Background = Theme.RowHover;
            header.MouseLeave += (s, e) => header.Background = device.Supported ? Theme.RowSupported : Brushes.Transparent;

            outer.Child = root;
            return outer;
        }

        private static Button MakeInventoryAction(string text)
        {
            var button = new Button();
            button.Content = text;
            button.FontSize = 9.5;
            button.Padding = new Thickness(7, 3, 7, 3);
            button.MinWidth = 0;
            button.Foreground = Theme.Secondary;
            button.Background = Theme.GhostHover;
            button.BorderBrush = Theme.OptionBorder;
            button.BorderThickness = new Thickness(1);
            return button;
        }

        private static string GroupDiagnostic(InventoryDeviceGroup device)
        {
            var sb = new StringBuilder();
            sb.AppendLine(device.Name + "  ·  " + device.VidPid);
            sb.AppendLine("共 " + device.Interfaces.Count + " 个接口");
            sb.AppendLine("========================================");
            foreach (InventoryRow row in device.Interfaces)
            {
                sb.AppendLine(row.Detail);
                sb.AppendLine("----------------------------------------");
            }
            return sb.ToString().TrimEnd();
        }

        private Border BuildInventoryRow(InventoryRow row)
        {
            var border = new Border();
            border.Height = 44;
            border.Padding = new Thickness(8, 0, 8, 0);
            border.BorderBrush = Theme.RowBorder;
            border.BorderThickness = new Thickness(0, 0, 0, 1);
            border.Background = row.IsSupported ? Theme.RowSupported : Brushes.Transparent;
            border.Cursor = Cursors.Hand;
            border.ToolTip = row.Detail;
            ToolTipService.SetInitialShowDelay(border, 400);
            ToolTipService.SetShowDuration(border, 60000);
            border.MouseEnter += (s, e) => border.Background = Theme.RowHover;
            border.MouseLeave += (s, e) => border.Background = row.IsSupported ? Theme.RowSupported : Brushes.Transparent;
            border.MouseLeftButtonUp += (s, e) => CopyText(row.Detail, "已复制该设备的诊断信息");

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.65, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.95, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.4, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.35, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.9, GridUnitType.Star) });

            var nameGrid = new Grid();
            nameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
            nameGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            var glyph = new TextBlock();
            glyph.Text = "\uE83F";
            glyph.FontFamily = Theme.IconFont;
            glyph.FontSize = 11;
            glyph.Foreground = row.IsSupported ? Theme.Accent : Theme.Muted;
            glyph.VerticalAlignment = VerticalAlignment.Center;
            nameGrid.Children.Add(glyph);
            var name = new TextBlock();
            name.Text = row.Name;
            name.FontSize = 11.5;
            name.Foreground = row.IsSupported ? Theme.Text : Theme.Secondary;
            name.TextTrimming = TextTrimming.CharacterEllipsis;
            name.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(name, 1);
            nameGrid.Children.Add(name);
            grid.Children.Add(nameGrid);

            AddCell(grid, row.Kind, 1, 10.5, Theme.Secondary);
            AddCell(grid, row.VidPid, 2, 10.5, Theme.Secondary);
            AddCell(grid, row.Usage, 3, 10, Theme.Muted);
            AddCell(grid, row.Interface, 4, 10, Theme.Muted);
            AddCell(grid, row.Reports, 5, 9.5, Theme.Muted);
            AddCell(grid, row.Status, 6, 10, row.IsSupported ? Theme.Accent : Theme.Muted);

            border.Child = grid;
            return border;
        }

        private static void AddCell(Grid grid, string text, int column, double fontSize, Brush color)
        {
            var block = new TextBlock();
            block.Text = text;
            block.FontSize = fontSize;
            block.Foreground = color;
            block.TextTrimming = TextTrimming.CharacterEllipsis;
            block.VerticalAlignment = VerticalAlignment.Center;
            block.Margin = new Thickness(4, 0, 8, 0);
            Grid.SetColumn(block, column);
            grid.Children.Add(block);
        }

        private void ShowTab(bool devices)
        {
            showingDevices = devices;
            showingSettings = false;
            settingsView.Visibility = Visibility.Collapsed;
            profileView.Visibility = Visibility.Collapsed;
            SetTabState(batteryTab, !devices);
            SetTabState(deviceTab, devices);
            SetTabState(profileTab, false);
            SetTabState(settingsTab, false);
            if (devices)
            {
                inventoryView.Visibility = Visibility.Visible;
                cardScroll.Visibility = Visibility.Collapsed;
                emptyState.Visibility = Visibility.Collapsed;
            }
            else
            {
                inventoryView.Visibility = Visibility.Collapsed;
                bool empty = lastReadings.Count == 0;
                cardScroll.Visibility = empty ? Visibility.Collapsed : Visibility.Visible;
                emptyState.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ShowProfile()
        {
            showingDevices = false;
            showingSettings = false;
            profileView.Visibility = Visibility.Visible;
            inventoryView.Visibility = Visibility.Collapsed;
            settingsView.Visibility = Visibility.Collapsed;
            cardScroll.Visibility = Visibility.Collapsed;
            emptyState.Visibility = Visibility.Collapsed;
            SetTabState(batteryTab, false);
            SetTabState(deviceTab, false);
            SetTabState(profileTab, true);
            SetTabState(settingsTab, false);
            if (profileController != null) profileController.RefreshProfiles();
        }

        private void ShowSettings()
        {
            showingSettings = true;
            showingDevices = false;
            settingsView.Visibility = Visibility.Visible;
            profileView.Visibility = Visibility.Collapsed;
            inventoryView.Visibility = Visibility.Collapsed;
            cardScroll.Visibility = Visibility.Collapsed;
            emptyState.Visibility = Visibility.Collapsed;
            SetTabState(batteryTab, false);
            SetTabState(deviceTab, false);
            SetTabState(profileTab, false);
            SetTabState(settingsTab, true);
            RefreshSettingsState();
        }

        private static void SetTabState(Button tab, bool active)
        {
            tab.Background = active ? Theme.TabActive : Brushes.Transparent;
            tab.Foreground = active ? Theme.TabActiveForeground : Theme.TabInactiveForeground;
        }

        private void CopyDiagnostics()
        {
            if (lastInventory.Count == 0)
            {
                ShowToast("还没有可复制的设备信息");
                return;
            }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("GearPower 诊断信息");
            sb.AppendLine("时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("系统：" + Environment.OSVersion.VersionString + " / " + (Environment.Is64BitOperatingSystem ? "x64" : "x86"));
            sb.AppendLine("接口数：" + lastInventory.Count);
            sb.AppendLine("========================================");
            foreach (InventoryRow row in lastInventory)
            {
                sb.AppendLine(row.Detail);
                sb.AppendLine("----------------------------------------");
            }
            CopyText(sb.ToString(), "诊断信息已复制，可发给开发者适配新设备");
        }

        private void OpenDeviceRequest()
        {
            const string url = "https://github.com/XAVIER0229/GearPower/issues/new?title=%5BDevice%5D%20";
            try
            {
                var info = new ProcessStartInfo();
                info.FileName = url;
                info.UseShellExecute = true;
                Process.Start(info);
                ShowToast("已打开设备支持表单，请附上诊断信息");
            }
            catch (Exception ex)
            {
                Log.Error("Open device request failed: " + ex.Message);
                ShowToast("无法打开浏览器，请手动访问 GitHub Issues");
            }
        }

        private void CopyText(string text, string message)        {
            try
            {
                Clipboard.SetText(text ?? "");
                ShowToast(message);
            }
            catch (Exception ex)
            {
                Log.Error("Clipboard failed: " + ex.Message);
                ShowToast("复制失败，请打开日志查看");
            }
        }

        private void ShowTrayHelp()
        {
            string text;
            if (lowIntegrity)
            {
                text = "当前程序以低权限（Low Integrity）运行。\n\n" +
                       "Windows 会阻止低权限进程注册系统托盘图标，所以你看不到 GearPower。\n\n" +
                       "请点击「设置 → 工具 → 以管理员身份重启」，或关闭后以管理员身份运行 GearPower.exe。\n" +
                       "这样托盘图标才能显示。";
            }
            else if (!trayEnabled)
            {
                text = "托盘图标已在「设置 → 系统托盘」中关闭。\n\n" +
                       "开启后，关闭主窗口会隐藏到托盘；关闭状态下关闭窗口会直接退出。";
            }
            else if (tray == null)
            {
                text = "托盘图标创建失败，关闭窗口会直接退出。\n\n请点击「设置 → 工具 → 打开日志」查看 app.log，或尝试以管理员身份运行。";
            }
            else
            {
                tray.EnsureVisible();
                tray.ShowStartupHint();
                text = "托盘图标已启用。\n\n" +
                       "Windows 11 默认会把新图标放进任务栏右下角的 ^ 隐藏图标区。\n\n" +
                       "1. 点击任务栏的 ^\n" +
                       "2. 找到 GearPower 的电池图标\n" +
                       "3. 把它拖到任务栏上\n\n" +
                       "也可以打开：设置 → 个性化 → 任务栏 → 其他系统托盘图标，\n" +
                       "在列表里打开 GearPower。";
            }
            MessageBox.Show(window, text, "托盘图标", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OpenExeFolder()
        {
            try
            {
                string exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
                Process.Start("explorer.exe", "/select,\"" + exe + "\"");
            }
            catch (Exception ex)
            {
                Log.Error("Open exe folder failed: " + ex.Message);
                ShowToast("无法打开文件夹");
            }
        }

        private void OpenLog()
        {
            try
            {
                if (!string.IsNullOrEmpty(Log.LogPath) && File.Exists(Log.LogPath))
                    Process.Start("explorer.exe", "/select,\"" + Log.LogPath + "\"");
                else
                    Process.Start("explorer.exe", System.IO.Path.GetDirectoryName(Log.LogPath));
            }
            catch (Exception ex)
            {
                Log.Error("Open log failed: " + ex.Message);
                ShowToast("无法打开日志目录");
            }
        }

        private void ShowToast(string message)
        {
            toastText.Text = message;
            toast.BeginAnimation(UIElement.OpacityProperty, null);
            toast.Opacity = 1;
            toastTimer.Stop();
            toastTimer.Start();
        }

        private void HideToast()
        {
            var animation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(260));
            animation.Completed += (s, e) => { toast.Opacity = 0; };
            toast.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private ScanResult BuildDemoResult()
        {
            var result = new ScanResult();
            result.Seconds = 0.62;
            result.HidCount = 9;
            result.BluetoothCount = 1;

            result.Readings.Add(DemoReading("VXE R1 Pro Max", "VXE / VGN", DeviceKind.Mouse, "2.4G 接收器", "ATK / VXE 私有协议", 12, false, false, false));
            result.Readings.Add(DemoReading("ROG Delta II", "ASUS ROG", DeviceKind.Headset, "2.4G 接收器", "ASUS HID", 23, false, false, false));
            result.Readings.Add(DemoReading("Razer Viper V3 Pro", "Razer 雷蛇", DeviceKind.Mouse, "2.4G 接收器", "Razer HID", 46, true, false, false));
            result.Readings.Add(DemoReading("HyperX Cloud III Wireless", "HyperX", DeviceKind.Headset, "2.4G 接收器", "HyperX HID", 67, false, false, false));
            result.Readings.Add(DemoReading("ATK F1 Ultimate", "ATK", DeviceKind.Mouse, "2.4G 接收器", "ATK / VXE 私有协议", 82, false, false, false));
            result.Readings.Add(DemoReading("Lamzu Maya X", "兰族 Lamzu", DeviceKind.Mouse, "2.4G 接收器", "Lamzu HID", 91, false, false, false));

            AddDemoInventory(result, "VXE R1 Pro Max", "鼠标", "3554:F58A", "FF02/0002", "2", "IN 17 · OUT 17 · F 0", "电量 12% · ATK / VXE 私有协议", true);
            AddDemoInventory(result, "ROG Delta II", "耳机", "0B05:1B6A", "FF01/0001", "2", "IN 64 · OUT 64 · F 65", "电量 23% · ASUS HID", true);
            AddDemoInventory(result, "Razer Viper V3 Pro", "鼠标", "1532:00C1", "FF00/0002", "2", "IN 64 · OUT 64 · F 91", "电量 46% · Razer HID", true);
            AddDemoInventory(result, "HyperX Cloud III Wireless", "耳机", "03F0:05B7", "FF00/0001", "2", "IN 62 · OUT 62 · F 0", "电量 67% · HyperX HID", true);
            AddDemoInventory(result, "ATK F1 Ultimate", "鼠标", "373B:1031", "FF02/0002", "2", "IN 17 · OUT 17 · F 0", "电量 82% · ATK / VXE 私有协议", true);
            AddDemoInventory(result, "Lamzu Maya X", "鼠标", "373E:001E", "FFFF/0000", "2", "IN 65 · OUT 65 · F 65", "电量 91% · Lamzu HID", true);
            AddDemoInventory(result, "ROG OMNI Receiver", "设备", "0B05:1ACE", "FF00/0001", "2", "IN 64 · OUT 64 · F 65", "—", false);
            AddDemoInventory(result, "Bluetooth 耳机", "耳机", "蓝牙", "BLE / 经典蓝牙", "—", "—", "电量 58% · Windows 蓝牙电量", true);

            return result;
        }

        private static DeviceReading DemoReading(string name, string vendor, DeviceKind kind, string transport, string protocol, int percent, bool charging, bool full, bool asleep)
        {
            var reading = new DeviceReading();
            reading.Id = "demo:" + name;
            reading.Name = name;
            reading.Vendor = vendor;
            reading.Kind = kind;
            reading.Transport = transport;
            reading.Protocol = protocol;
            reading.Percent = percent;
            reading.Charging = charging;
            reading.Full = full;
            reading.Asleep = asleep;
            reading.Updated = DateTime.Now;
            reading.IsDemo = true;
            return reading;
        }

        private static void AddDemoInventory(ScanResult result, string name, string kind, string vidpid, string usage, string iface, string reports, string status, bool supported)
        {
            var row = new InventoryRow();
            row.Name = name + (iface != "—" ? "  ·  IF" + iface : "");
            row.Kind = kind;
            row.VidPid = vidpid;
            row.Usage = usage;
            row.Interface = iface;
            row.Reports = reports;
            row.Status = status;
            row.IsSupported = supported;
            row.Path = @"\\?\hid#demo&" + name.Replace(" ", "");
            row.Detail = name + Environment.NewLine + "VID:PID  " + vidpid + Environment.NewLine +
                         "用途页  " + usage + Environment.NewLine + "报告    " + reports + Environment.NewLine +
                         "状态    " + status + Environment.NewLine + Environment.NewLine + "(演示数据)";
            result.Inventory.Add(row);
        }
    }
}

