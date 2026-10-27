using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace GearPower
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            Log.Info("GearPower " + Assembly.GetExecutingAssembly().GetName().Version + " starting");
            Log.Info("OS: " + Environment.OSVersion + "; OS64=" + Environment.Is64BitOperatingSystem +
                     "; Process64=" + Environment.Is64BitProcess + "; CLR=" + Environment.Version);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                Log.Error("Fatal unhandled exception: " + e.ExceptionObject);

            bool demo = args != null && args.Any(a => string.Equals(a, "--demo", StringComparison.OrdinalIgnoreCase));
            bool selftest = args != null && args.Any(a => string.Equals(a, "--selftest", StringComparison.OrdinalIgnoreCase));
            bool dump = args != null && args.Any(a => string.Equals(a, "--dump", StringComparison.OrdinalIgnoreCase));
            bool trayPreview = args != null && args.Any(a => string.Equals(a, "--tray-preview", StringComparison.OrdinalIgnoreCase));

            if (!selftest && !dump && !trayPreview)
            {
                if (!SingleInstance.Acquire())
                {
                    SingleInstance.SignalExisting();
                    return 0;
                }
            }

            if (trayPreview)
            {
                TrayPreview.Run();
                return 0;
            }

            if (dump)
            {
                Dump.Write();
                return 0;
            }

            if (selftest)
            {
                bool ok = SelfTest.Run();
                Environment.ExitCode = ok ? 0 : 1;
                return Environment.ExitCode;
            }

            var app = new Application();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            app.DispatcherUnhandledException += (s, e) =>
            {
                Log.Error("Unhandled UI exception: " + e.Exception);
                MessageBox.Show("出现未处理的错误：\n" + e.Exception.Message + "\n\n日志：" + Log.LogPath,
                    "GearPower", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true;
            };

            Window window;
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                string xaml;
                using (Stream stream = asm.GetManifestResourceStream("ui.xaml"))
                {
                    if (stream == null) throw new InvalidOperationException("缺少内嵌的 ui.xaml 资源");
                    using (var reader = new StreamReader(stream))
                    {
                        xaml = reader.ReadToEnd();
                    }
                }
                window = (Window)XamlReader.Parse(xaml);
            }
            catch (Exception ex)
            {
                Log.Error("Window creation failed: " + ex);
                MessageBox.Show("无法创建界面：\n" + ex.Message, "GearPower", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }

            var controller = new UiController(window, demo);
            GC.KeepAlive(controller);
            SingleInstance.Listen(() => controller.ActivateFromSecondInstance());
            app.SessionEnding += (s, e) => controller.PrepareSystemExit();

            try
            {
                app.Run(window);
            }
            catch (Exception ex)
            {
                Log.Error("Application failed: " + ex);
                return 1;
            }
            return 0;
        }
    }
}
