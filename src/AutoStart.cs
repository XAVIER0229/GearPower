using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace GearPower
{
    /// <summary>
    /// Per-user login startup via HKCU Run. No elevation and no scheduled task.
    /// The entry intentionally has no special argument: a login launch shows
    /// the same main window, tray icon and taskbar bar as a normal launch.
    /// </summary>
    public static class AutoStart
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "GearPower";

        public static string ExecutablePath
        {
            get
            {
                try
                {
                    string path = Process.GetCurrentProcess().MainModule.FileName;
                    if (!string.IsNullOrEmpty(path)) return path;
                }
                catch
                {
                }
                try
                {
                    string path = Assembly.GetExecutingAssembly().Location;
                    if (!string.IsNullOrEmpty(path)) return path;
                }
                catch
                {
                }
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "GearPower.exe");
            }
        }

        public static string Command
        {
            get { return "\"" + ExecutablePath + "\""; }
        }

        public static bool IsEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey))
                {
                    if (key == null) return false;
                    return !string.IsNullOrEmpty(key.GetValue(ValueName) as string);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Auto-start read failed: " + ex.Message);
                return false;
            }
        }

        public static bool SetEnabled(bool enabled)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey))
                {
                    if (key == null) return false;
                    if (enabled) key.SetValue(ValueName, Command, RegistryValueKind.String);
                    else key.DeleteValue(ValueName, false);
                }
                Log.Debug("Auto-start " + (enabled ? "enabled: " + Command : "disabled"));
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Auto-start update failed: " + ex);
                return false;
            }
        }

        /// <summary>Keep a moved portable exe pointing at its current path.</summary>
        public static void RepairIfEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RunKey, true))
                {
                    if (key == null) return;
                    string existing = key.GetValue(ValueName) as string;
                    if (string.IsNullOrEmpty(existing)) return;
                    if (!string.Equals(existing.Trim(), Command, StringComparison.OrdinalIgnoreCase))
                        key.SetValue(ValueName, Command, RegistryValueKind.String);
                }
            }
            catch (Exception ex)
            {
                Log.Debug("Auto-start path repair failed: " + ex.Message);
            }
        }
    }
}
