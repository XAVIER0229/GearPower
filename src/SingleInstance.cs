using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace GearPower
{
    /// <summary>
    /// Keeps one UI instance per Windows session. Diagnostic commands such as
    /// --selftest and --dump intentionally bypass this guard.
    /// </summary>
    public static class SingleInstance
    {
        private const string MutexName = @"Local\GearPower.SingleInstance.8";
        private const string EventName = @"Local\GearPower.Activate.8";

        private static Mutex instanceMutex;
        private static EventWaitHandle activateEvent;
        private static RegisteredWaitHandle waitHandle;

        public static bool Acquire()
        {
            try
            {
                instanceMutex = new Mutex(false, MutexName);
                try
                {
                    return instanceMutex.WaitOne(0);
                }
                catch (AbandonedMutexException)
                {
                    // Previous instance crashed; take ownership.
                    return true;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // The running instance is at a different integrity level.
                // Do not create a second copy; the activation fallback may
                // still be able to bring its window forward.
                Log.Debug("Single-instance mutex access denied: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Log.Debug("Single-instance mutex unavailable: " + ex.Message);
                return true;
            }
        }

        public static void Listen(Action activate)
        {
            if (activate == null) return;
            try
            {
                activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);
                waitHandle = ThreadPool.RegisterWaitForSingleObject(
                    activateEvent,
                    (state, timedOut) =>
                    {
                        if (timedOut) return;
                        try
                        {
                            activate();
                        }
                        catch
                        {
                        }
                    },
                    null,
                    Timeout.Infinite,
                    false);
            }
            catch (Exception ex)
            {
                Log.Debug("Single-instance activation listener unavailable: " + ex.Message);
            }
        }

        public static void SignalExisting()
        {
            try
            {
                using (EventWaitHandle ev = EventWaitHandle.OpenExisting(EventName))
                {
                    ev.Set();
                }
            }
            catch
            {
            }

            // Fallback for a different integrity level, where the named event
            // may not be writable: show the existing top-level window directly.
            IntPtr hwnd = FindWindow(null, "GearPower");
            if (hwnd != IntPtr.Zero)
            {
                ShowWindow(hwnd, 9);
                SetForegroundWindow(hwnd);
            }
        }

        public static bool ReleaseForRestart()
        {
            if (instanceMutex == null) return false;
            try
            {
                instanceMutex.ReleaseMutex();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void ReacquireAfterFailedRestart()
        {
            if (instanceMutex == null) return;
            try
            {
                instanceMutex.WaitOne(0);
            }
            catch (AbandonedMutexException)
            {
            }
            catch
            {
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
