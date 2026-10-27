using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GearPower
{
    // Independent implementation of the 05AC:0255 battery quirk documented by
    // Linux v6.6 hid-input.c: GET_REPORT(Feature), with the returned byte already
    // expressed as a percentage despite the Input descriptor's logical 0..255.
    // Windows may reject Feature 0x47 if its cached HID descriptor omits it.
    // Do not rewrite that descriptor or change the user's driver/pairing.
    public sealed class AppleWirelessKeyboardProtocol : HidProtocol
    {
        public override string Name { get { return "Apple Wireless Keyboard 电量"; } }

        public override bool Matches(DeviceGroup group)
        {
            return group.Bluetooth && group.Interfaces.Any(IsModel);
        }

        private static bool IsModel(HidInterfaceInfo iface)
        {
            return iface.Vid == 0x05AC && iface.Pid == 0x0255;
        }

        public static bool HasBatteryReport(HidInterfaceInfo iface)
        {
            return iface.ValueCaps.Any(c => c.UsagePage == 0x06 && c.Usage == 0x20
                && c.ReportId == 0x47 && c.BitSize == 8 && c.ReportCount == 1);
        }

        public static int? ParseBattery(byte[] report)
        {
            if (report == null || report.Length < 2 || report[0] != 0x47 || report[1] > 100)
                return null;
            return report[1];
        }

        public override ProbeResult Probe(DeviceGroup group)
        {
            if (!Matches(group)) return null;
            var attempts = new List<string>();
            var candidates = group.Interfaces.Where(i => IsModel(i) && i.UsagePage == 0x0C)
                .OrderByDescending(HasBatteryReport).ThenByDescending(i => i.FeatureLength);
            foreach (HidInterfaceInfo iface in candidates)
            {
                // Call the documented user-mode HidD APIs on a synchronous,
                // zero-access handle. The shared IOCTL wrapper stops on error
                // 87 without invoking HidD_GetInputReport, so it cannot tell us
                // whether the API itself is supported by this Bluetooth stack.
                IntPtr handle = NativeMethods.CreateFileW(iface.Path, 0,
                    NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                    IntPtr.Zero, NativeMethods.OPEN_EXISTING, 0, IntPtr.Zero);
                int openError = Marshal.GetLastWin32Error();
                try
                {
                    string label = "IN " + iface.InputLength + "/F " + iface.FeatureLength
                        + (HasBatteryReport(iface) ? " 电量接口" : " 控制接口");
                    if (handle == IntPtr.Zero || handle == new IntPtr(-1))
                    {
                        attempts.Add(label + " 打开失败: " + openError);
                        continue;
                    }
                    // Try the actual two-byte wire report even when the only
                    // declared Feature on this collection is the unrelated 0x09.
                    foreach (int length in new int[] { 2, Math.Max(2, iface.FeatureLength) }.Distinct())
                    {
                        byte[] report = new byte[length];
                        report[0] = 0x47;
                        report[1] = 0xFF; // An untouched buffer must never become 0%.
                        bool ok = NativeMethods.HidD_GetFeature(handle, report, report.Length);
                        int nativeError = Marshal.GetLastWin32Error();
                        int? percent = ok ? ParseBattery(report) : null;
                        if (percent.HasValue)
                            return Reading(percent.Value, "只读 Feature 0x47 · " + label);
                        attempts.Add(label + " HidD Feature 0x47/" + length + (ok
                            ? " 无有效电量: " + BitConverter.ToString(report)
                            : " 失败 (" + nativeError + ")"));
                    }
                    // Some Windows drivers implement the descriptor's Input
                    // form instead. Only query the collection declaring 0x47;
                    // never read/consume the keyboard's interrupt input stream.
                    if (HasBatteryReport(iface) && iface.InputLength >= 2)
                    {
                        byte[] report = new byte[iface.InputLength];
                        report[0] = 0x47;
                        report[1] = 0xFF;
                        bool ok = NativeMethods.HidD_GetInputReport(handle, report, report.Length);
                        int nativeError = Marshal.GetLastWin32Error();
                        int? percent = ok ? ParseBattery(report) : null;
                        if (percent.HasValue)
                            return Reading(percent.Value, "只读 Input 0x47 · " + label);
                        attempts.Add(label + " HidD Input 0x47" + (ok
                            ? " 无有效电量: " + BitConverter.ToString(report)
                            : " 失败 (" + nativeError + ")"));
                    }
                }
                finally
                {
                    if (handle != IntPtr.Zero && handle != new IntPtr(-1)) NativeMethods.CloseHandle(handle);
                }
            }
            string detail = string.Join("；", attempts.ToArray());
            Log.Debug("Apple Wireless Keyboard 05AC:0255: " + detail);
            return new ProbeResult
            {
                DeviceDetected = true,
                Kind = DeviceKind.Keyboard,
                Protocol = Name,
                Note = "test2：已识别 05AC:0255，未读到电量。Windows 可能未开放 Feature 0x47；"
                    + "未修改驱动、蓝牙配对或 HID 描述符。" + detail
            };
        }

        private ProbeResult Reading(int percent, string note)
        {
            // This replaceable-battery keyboard has no charging/full signal.
            ProbeResult result = Result(percent, false, false, false, Name, note);
            result.DeviceDetected = true;
            result.Kind = DeviceKind.Keyboard;
            return result;
        }
    }
}
