using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace GearPower
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HidCaps
    {
        public ushort Usage;
        public ushort UsagePage;
        public ushort InputReportByteLength;
        public ushort OutputReportByteLength;
        public ushort FeatureReportByteLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public ushort[] Reserved;
        public ushort NumberLinkCollectionNodes;
        public ushort NumberInputButtonCaps;
        public ushort NumberInputValueCaps;
        public ushort NumberInputDataIndices;
        public ushort NumberOutputButtonCaps;
        public ushort NumberOutputValueCaps;
        public ushort NumberOutputDataIndices;
        public ushort NumberFeatureButtonCaps;
        public ushort NumberFeatureValueCaps;
        public ushort NumberFeatureDataIndices;
    }

    public sealed class HidValueCap
    {
        public HidReportType ReportType;
        public ushort UsagePage;
        public ushort Usage;
        public byte ReportId;
        public int BitOffset;
        public int BitSize;
        public int ReportCount;
        public int LogicalMin;
        public int LogicalMax;
        public ushort LinkCollection;

        public string Describe()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{0} id={1} usage={2:X4}/{3:X4} bit={4}+{5} count={6} logical={7}..{8}",
                ReportType, ReportId, UsagePage, Usage, BitOffset, BitSize, ReportCount, LogicalMin, LogicalMax);
        }
    }

    public sealed class HidInterfaceInfo
    {
        public string Path;
        public ushort Vid;
        public ushort Pid;
        public ushort Version;
        public string Product;
        public string Manufacturer;
        public string Serial;
        public string FriendlyName;
        public string InstanceId;
        public string ContainerId;
        public ushort UsagePage;
        public ushort Usage;
        public int InterfaceNumber = -1;
        public int InputLength;
        public int OutputLength;
        public int FeatureLength;
        public byte[] ReportDescriptor = new byte[0];
        public HidReportDescriptor Parsed;
        public List<HidValueCap> ValueCaps = new List<HidValueCap>();
        public bool Openable;
        public string OpenError;

        public HidValueCap FindBatteryValueCap()
        {
            HidValueCap input = null;
            foreach (HidValueCap cap in ValueCaps)
            {
                if (cap.UsagePage == 0x06 && cap.Usage == 0x20)
                {
                    if (cap.ReportType == HidReportType.Feature) return cap;
                    if (input == null && cap.ReportType == HidReportType.Input) input = cap;
                }
            }
            return input;
        }

        public string VidPid
        {
            get { return string.Format(CultureInfo.InvariantCulture, "{0:X4}:{1:X4}", Vid, Pid); }
        }

        public string UsageText
        {
            get { return string.Format(CultureInfo.InvariantCulture, "{0:X4}/{1:X4}", UsagePage, Usage); }
        }

        public string ReportsText
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "IN {0} · OUT {1} · F {2}", InputLength, OutputLength, FeatureLength);
            }
        }

        public string PhysicalKey
        {
            get
            {
                if (!string.IsNullOrEmpty(ContainerId)) return "container:" + ContainerId;
                string instance = InstanceId ?? "";
                int cut = instance.IndexOf("&mi_", StringComparison.OrdinalIgnoreCase);
                if (cut > 0) instance = instance.Substring(0, cut);
                if (instance.Length > 0) return "instance:" + instance;
                string serial = string.IsNullOrEmpty(Serial) ? "" : Serial;
                if (serial.Length > 0) return string.Format(CultureInfo.InvariantCulture, "v{0:X4}p{1:X4}:{2}", Vid, Pid, serial);
                return "path:" + Path;
            }
        }
    }

    public sealed class BluetoothBatteryInfo
    {
        public string Name;
        public int Percent;
        public string ContainerId;
        public string InstanceId;
        public DeviceKind Kind;
    }

    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool GetWindowRect(IntPtr hwnd, out Rect32 rect);

        [DllImport("user32.dll")]
        internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

        public const uint DIGCF_PRESENT = 0x00000002;
        public const uint DIGCF_ALLCLASSES = 0x00000004;
        public const uint DIGCF_DEVICEINTERFACE = 0x00000010;

        public const uint GENERIC_READ = 0x80000000;
        public const uint GENERIC_WRITE = 0x40000000;
        public const uint FILE_SHARE_READ = 0x00000001;
        public const uint FILE_SHARE_WRITE = 0x00000002;
        public const uint OPEN_EXISTING = 3;
        public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
        public const int ERROR_IO_PENDING = 997;
        public const int ERROR_INSUFFICIENT_BUFFER = 122;
        public const uint WAIT_OBJECT_0 = 0;
        public const uint WAIT_TIMEOUT = 258;

        public const uint IOCTL_HID_GET_REPORT_DESCRIPTOR = 0x000B0000;
        public const uint IOCTL_HID_GET_FEATURE = 0x000B0192;
        public const uint IOCTL_HID_SET_FEATURE = 0x000B0191;
        public const uint IOCTL_HID_GET_INPUT_REPORT = 0x000B01A2;
        public const uint IOCTL_HID_SET_OUTPUT_REPORT = 0x000B0195;
        public const int ERROR_INVALID_FUNCTION = 1;
        public const int ERROR_NOT_SUPPORTED = 50;

        public static readonly Guid HidInterfaceGuid = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");
        public static readonly Guid BluetoothClassGuid = new Guid("e0cbf06c-cd8b-4647-bb8a-263b43f0f974");

        public static readonly DevPropKey KeyFriendlyName = new DevPropKey(new Guid("A45C254E-DF1C-4EFD-8020-67D146A850E0"), 14);
        public static readonly DevPropKey KeyContainerId = new DevPropKey(new Guid("8C7ED206-3F8A-4827-B3AB-AE9E1FAEFC6C"), 2);
        public static readonly DevPropKey KeyInstanceId = new DevPropKey(new Guid("78C34FC8-104A-4ACA-9EA4-524D52996E57"), 256);
        public static readonly DevPropKey KeyBluetoothBattery = new DevPropKey(new Guid("104EA319-6EE2-4701-BD47-8DDBF425BBE5"), 2);

        [StructLayout(LayoutKind.Sequential)]
        public struct Point32
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect32
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MinMaxInfo
        {
            public Point32 Reserved;
            public Point32 MaxSize;
            public Point32 MaxPosition;
            public Point32 MinTrackSize;
            public Point32 MaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MonitorInfo
        {
            public int cbSize;
            public Rect32 rcMonitor;
            public Rect32 rcWork;
            public uint dwFlags;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint flags);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetMonitorInfoW(IntPtr monitor, ref MonitorInfo info);

        [StructLayout(LayoutKind.Sequential)]
        public struct SpDeviceInterfaceData
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SpDevInfoData
        {
            public int cbSize;
            public Guid ClassGuid;
            public int DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DevPropKey
        {
            public Guid fmtid;
            public uint pid;

            public DevPropKey(Guid id, uint propertyId)
            {
                fmtid = id;
                pid = propertyId;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HidAttributes
        {
            public int Size;
            public ushort VendorID;
            public ushort ProductID;
            public ushort VersionNumber;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Overlapped
        {
            public IntPtr Internal;
            public IntPtr InternalHigh;
            public int Offset;
            public int OffsetHigh;
            public IntPtr hEvent;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevsW(ref Guid classGuid, IntPtr enumerator, IntPtr hwndParent, uint flags);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, IntPtr deviceInfoData, ref Guid interfaceClassGuid, uint memberIndex, ref SpDeviceInterfaceData deviceInterfaceData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetailW(IntPtr deviceInfoSet, ref SpDeviceInterfaceData deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, ref SpDevInfoData deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetailW(IntPtr deviceInfoSet, ref SpDeviceInterfaceData deviceInterfaceData, IntPtr deviceInterfaceDetailData, uint deviceInterfaceDetailDataSize, out uint requiredSize, IntPtr deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(IntPtr deviceInfoSet, uint memberIndex, ref SpDevInfoData deviceInfoData);

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetupDiGetDevicePropertyW(IntPtr deviceInfoSet, ref SpDevInfoData deviceInfoData, ref DevPropKey propertyKey, out uint propertyType, IntPtr propertyBuffer, uint propertyBufferSize, out uint requiredSize, uint flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(IntPtr deviceInfoSet);

        [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
        public static extern uint CM_Get_Device_IDW(uint dnDevInst, StringBuilder buffer, int bufferLen, int ulFlags);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetAttributes(IntPtr hidDeviceObject, ref HidAttributes attributes);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool HidD_GetProductString(IntPtr hidDeviceObject, StringBuilder buffer, int bufferLength);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool HidD_GetManufacturerString(IntPtr hidDeviceObject, StringBuilder buffer, int bufferLength);

        [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool HidD_GetSerialNumberString(IntPtr hidDeviceObject, StringBuilder buffer, int bufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetPreparsedData(IntPtr hidDeviceObject, out IntPtr preparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_FreePreparsedData(IntPtr preparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern int HidP_GetCaps(IntPtr preparsedData, out HidCaps capabilities);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern int HidP_GetValueCaps(int reportType, IntPtr valueCaps, ref ushort valueCapsLength, IntPtr preparsedData);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern int HidP_GetUsageValue(int reportType, ushort usagePage, ushort linkCollection, ushort usage,
            out uint usageValue, IntPtr preparsedData, byte[] report, int reportLength);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct HidpValueCaps
        {
            public ushort UsagePage;
            public byte ReportID;
            public byte IsAlias;
            public ushort BitField;
            public ushort LinkCollection;
            public ushort LinkUsage;
            public ushort LinkUsagePage;
            public byte IsRange;
            public byte IsStringRange;
            public byte IsDesignatorRange;
            public byte IsAbsolute;
            public byte HasNull;
            public byte Reserved;
            public ushort BitSize;
            public ushort ReportCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public ushort[] Reserved2;
            public uint UnitsExp;
            public uint Units;
            public int LogicalMin;
            public int LogicalMax;
            public int PhysicalMin;
            public int PhysicalMax;
            public HidpValueCapsUnion Union;
        }

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        public struct HidpValueCapsUnion
        {
            [FieldOffset(0)] public ushort UsageMin;
            [FieldOffset(2)] public ushort UsageMax;
            [FieldOffset(0)] public ushort Usage;
        }

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetFeature(IntPtr hidDeviceObject, byte[] reportBuffer, int reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_SetFeature(IntPtr hidDeviceObject, byte[] reportBuffer, int reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_GetInputReport(IntPtr hidDeviceObject, byte[] reportBuffer, int reportBufferLength);

        [DllImport("hid.dll", SetLastError = true)]
        public static extern bool HidD_SetOutputReport(IntPtr hidDeviceObject, byte[] reportBuffer, int reportBufferLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFileW(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadFile(IntPtr file, [Out] byte[] buffer, int numberOfBytesToRead, out int numberOfBytesRead, ref Overlapped overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteFile(IntPtr file, [In] byte[] buffer, int numberOfBytesToWrite, out int numberOfBytesWritten, ref Overlapped overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetOverlappedResult(IntPtr file, ref Overlapped overlapped, out int numberOfBytesTransferred, bool wait);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateEventW(IntPtr eventAttributes, bool manualReset, bool initialState, string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ResetEvent(IntPtr eventHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint WaitForSingleObject(IntPtr handle, uint milliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CancelIoEx(IntPtr file, ref Overlapped overlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DeviceIoControl(IntPtr device, uint ioControlCode, IntPtr inBuffer, uint inBufferSize,
            [Out] byte[] outBuffer, uint outBufferSize, out uint bytesReturned, IntPtr overlapped);

        [DllImport("kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
        public static extern bool DeviceIoControlAsync(IntPtr device, uint ioControlCode, byte[] inBuffer, int inBufferSize,
            byte[] outBuffer, int outBufferSize, out int bytesReturned, ref Overlapped overlapped);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern int SetWindowTheme(IntPtr hwnd, string subAppName, IntPtr subIdList);
    }

    public sealed class HidHandle : IDisposable
    {
        private IntPtr handle;
        private IntPtr readEvent;
        private bool disposed;

        public string Path { get; private set; }

        public IntPtr HandleForQuery
        {
            get { return handle; }
        }

        private HidHandle(IntPtr h, IntPtr ev, string path)
        {
            handle = h;
            readEvent = ev;
            Path = path;
        }

        public static HidHandle Open(string path, bool readWrite, out string error)
        {
            error = null;
            uint access = readWrite ? (NativeMethods.GENERIC_READ | NativeMethods.GENERIC_WRITE) : 0;
            IntPtr h = NativeMethods.CreateFileW(path, access,
                NativeMethods.FILE_SHARE_READ | NativeMethods.FILE_SHARE_WRITE,
                IntPtr.Zero, NativeMethods.OPEN_EXISTING, NativeMethods.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
            if (h == IntPtr.Zero || h == new IntPtr(-1))
            {
                int e = Marshal.GetLastWin32Error();
                error = new Win32Exception(e).Message + " (" + e + ")";
                return null;
            }
            IntPtr ev = NativeMethods.CreateEventW(IntPtr.Zero, true, false, null);
            if (ev == IntPtr.Zero)
            {
                int e = Marshal.GetLastWin32Error();
                error = new Win32Exception(e).Message;
                NativeMethods.CloseHandle(h);
                return null;
            }
            return new HidHandle(h, ev, path);
        }

        public bool GetFeature(byte[] buffer)
        {
            return GetFeature(buffer, 900);
        }

        public bool GetFeature(byte[] buffer, int timeoutMs)
        {
            if (disposed) return false;
            int error;
            if (Ioctl(NativeMethods.IOCTL_HID_GET_FEATURE, null, 0, buffer, buffer.Length, timeoutMs, out error)) return true;
            // A mouse/keyboard top-level collection is often opened by Windows
            // without write sharing, so even a query-only handle can be denied
            // the IOCTL. HidD_* works on the same zero-access handle.
            return NativeMethods.HidD_GetFeature(handle, buffer, buffer.Length);
        }

        public bool SetFeature(byte[] buffer)
        {
            return SetFeature(buffer, 900);
        }

        public bool SetFeature(byte[] buffer, int timeoutMs)
        {
            if (disposed) return false;
            int error;
            if (Ioctl(NativeMethods.IOCTL_HID_SET_FEATURE, buffer, buffer.Length, null, 0, timeoutMs, out error)) return true;
            return NativeMethods.HidD_SetFeature(handle, buffer, buffer.Length);
        }

        public bool GetInputReport(byte[] buffer)
        {
            if (disposed) return false;
            int error;
            if (Ioctl(NativeMethods.IOCTL_HID_GET_INPUT_REPORT, null, 0, buffer, buffer.Length, 700, out error)) return true;
            if (error == NativeMethods.ERROR_INVALID_FUNCTION || error == NativeMethods.ERROR_NOT_SUPPORTED)
                return NativeMethods.HidD_GetInputReport(handle, buffer, buffer.Length);
            return false;
        }

        public bool SetOutputReport(byte[] buffer)
        {
            if (disposed) return false;
            int error;
            if (Ioctl(NativeMethods.IOCTL_HID_SET_OUTPUT_REPORT, buffer, buffer.Length, null, 0, 700, out error)) return true;
            if (error == NativeMethods.ERROR_INVALID_FUNCTION || error == NativeMethods.ERROR_NOT_SUPPORTED)
                return NativeMethods.HidD_SetOutputReport(handle, buffer, buffer.Length);
            return false;
        }

        private bool Ioctl(uint code, byte[] inBuffer, int inSize, byte[] outBuffer, int outSize, int timeoutMs, out int error)
        {
            error = 0;
            var ov = new NativeMethods.Overlapped();
            ov.hEvent = readEvent;
            NativeMethods.ResetEvent(readEvent);
            int bytes;
            bool ok = NativeMethods.DeviceIoControlAsync(handle, code, inBuffer, inSize, outBuffer, outSize, out bytes, ref ov);
            if (ok) return true;
            error = Marshal.GetLastWin32Error();
            if (error != NativeMethods.ERROR_IO_PENDING) return false;

            uint wait = NativeMethods.WaitForSingleObject(readEvent, (uint)Math.Max(1, timeoutMs));
            if (wait != NativeMethods.WAIT_OBJECT_0)
            {
                NativeMethods.CancelIoEx(handle, ref ov);
                error = 1460;
                return false;
            }
            if (!NativeMethods.GetOverlappedResult(handle, ref ov, out bytes, false))
            {
                error = Marshal.GetLastWin32Error();
                return false;
            }
            return true;
        }

        public byte[] Read(int length, int timeoutMs)
        {
            if (disposed || length <= 0) return null;
            var buffer = new byte[length];
            var ov = new NativeMethods.Overlapped();
            ov.hEvent = readEvent;
            NativeMethods.ResetEvent(readEvent);
            int read;
            bool ok = NativeMethods.ReadFile(handle, buffer, length, out read, ref ov);
            if (!ok)
            {
                int e = Marshal.GetLastWin32Error();
                if (e != NativeMethods.ERROR_IO_PENDING) return null;
                uint wait = NativeMethods.WaitForSingleObject(readEvent, (uint)Math.Max(1, timeoutMs));
                if (wait != NativeMethods.WAIT_OBJECT_0)
                {
                    NativeMethods.CancelIoEx(handle, ref ov);
                    return null;
                }
                if (!NativeMethods.GetOverlappedResult(handle, ref ov, out read, false)) return null;
            }
            if (read <= 0) return null;
            if (read == length) return buffer;
            var trimmed = new byte[read];
            Buffer.BlockCopy(buffer, 0, trimmed, 0, read);
            return trimmed;
        }

        public bool Write(byte[] buffer, int timeoutMs)
        {
            if (disposed || buffer == null || buffer.Length == 0) return false;
            var ov = new NativeMethods.Overlapped();
            ov.hEvent = readEvent;
            NativeMethods.ResetEvent(readEvent);
            int written;
            bool ok = NativeMethods.WriteFile(handle, buffer, buffer.Length, out written, ref ov);
            if (ok) return written > 0;
            int e = Marshal.GetLastWin32Error();
            if (e != NativeMethods.ERROR_IO_PENDING) return false;
            uint wait = NativeMethods.WaitForSingleObject(readEvent, (uint)Math.Max(1, timeoutMs));
            if (wait != NativeMethods.WAIT_OBJECT_0)
            {
                NativeMethods.CancelIoEx(handle, ref ov);
                return false;
            }
            return NativeMethods.GetOverlappedResult(handle, ref ov, out written, false) && written > 0;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (handle != IntPtr.Zero && handle != new IntPtr(-1)) NativeMethods.CloseHandle(handle);
            if (readEvent != IntPtr.Zero) NativeMethods.CloseHandle(readEvent);
            handle = IntPtr.Zero;
            readEvent = IntPtr.Zero;
        }
    }

    public static class HidEnumerator
    {
        public static List<HidInterfaceInfo> Enumerate()
        {
            var list = new List<HidInterfaceInfo>();
            Guid guid = NativeMethods.HidInterfaceGuid;
            IntPtr set = NativeMethods.SetupDiGetClassDevsW(ref guid, IntPtr.Zero, IntPtr.Zero,
                NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_DEVICEINTERFACE);
            if (set == IntPtr.Zero || set == new IntPtr(-1)) return list;

            try
            {
                uint index = 0;
                while (true)
                {
                    var did = new NativeMethods.SpDeviceInterfaceData();
                    did.cbSize = Marshal.SizeOf(typeof(NativeMethods.SpDeviceInterfaceData));
                    if (!NativeMethods.SetupDiEnumDeviceInterfaces(set, IntPtr.Zero, ref guid, index, ref did)) break;
                    index++;

                    HidInterfaceInfo info = ReadInterface(set, ref did);
                    if (info != null) list.Add(info);
                }
            }
            finally
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(set);
            }
            return list;
        }

        private static HidInterfaceInfo ReadInterface(IntPtr set, ref NativeMethods.SpDeviceInterfaceData did)
        {
            uint required;
            NativeMethods.SetupDiGetDeviceInterfaceDetailW(set, ref did, IntPtr.Zero, 0, out required, IntPtr.Zero);
            if (required == 0) return null;

            IntPtr detail = Marshal.AllocHGlobal((int)required);
            try
            {
                Marshal.WriteInt32(detail, IntPtr.Size == 8 ? 8 : 6);
                var devInfo = new NativeMethods.SpDevInfoData();
                devInfo.cbSize = Marshal.SizeOf(typeof(NativeMethods.SpDevInfoData));
                if (!NativeMethods.SetupDiGetDeviceInterfaceDetailW(set, ref did, detail, required, out required, ref devInfo))
                    return null;

                string path = Marshal.PtrToStringUni(new IntPtr(detail.ToInt64() + 4));
                if (string.IsNullOrEmpty(path)) return null;

                var info = new HidInterfaceInfo();
                info.Path = path;
                info.FriendlyName = GetStringProperty(set, ref devInfo, NativeMethods.KeyFriendlyName);
                info.ContainerId = GetGuidProperty(set, ref devInfo, NativeMethods.KeyContainerId);
                info.InstanceId = GetStringProperty(set, ref devInfo, NativeMethods.KeyInstanceId);
                if (string.IsNullOrEmpty(info.InstanceId)) info.InstanceId = GetInstanceId(devInfo.DevInst);
                info.InterfaceNumber = ParseInterfaceNumber(path);

                string openError;
                HidHandle handle = HidHandle.Open(path, true, out openError);
                if (handle == null)
                {
                    handle = HidHandle.Open(path, false, out openError);
                    info.Openable = false;
                }
                else
                {
                    info.Openable = true;
                }
                using (handle)
                {
                    if (handle == null)
                    {
                        info.OpenError = openError;
                        return info;
                    }
                    var attrs = new NativeMethods.HidAttributes();
                    attrs.Size = Marshal.SizeOf(typeof(NativeMethods.HidAttributes));
                    if (NativeMethods.HidD_GetAttributes(handle.HandleForQuery, ref attrs))
                    {
                        info.Vid = attrs.VendorID;
                        info.Pid = attrs.ProductID;
                        info.Version = attrs.VersionNumber;
                    }
                    info.Product = GetHidString(handle.HandleForQuery, true);
                    info.Manufacturer = GetHidString(handle.HandleForQuery, false);
                    info.Serial = GetHidSerial(handle.HandleForQuery);
                    info.ReportDescriptor = new byte[0];
                    info.Parsed = HidReportDescriptor.Parse(info.ReportDescriptor);
                    ReadCaps(handle.HandleForQuery, info);
                }
                if (info.Parsed != null && info.UsagePage == 0)
                {
                    info.UsagePage = info.Parsed.TopUsagePage;
                    info.Usage = info.Parsed.TopUsage;
                }
                return info;
            }
            finally
            {
                Marshal.FreeHGlobal(detail);
            }
        }

        private static int ParseInterfaceNumber(string path)
        {
            string p = path.ToLowerInvariant();
            int i = p.IndexOf("mi_");
            if (i < 0 || i + 5 > p.Length) return -1;
            string hex = p.Substring(i + 3, 2);
            int value;
            if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value)) return value;
            return -1;
        }

        private static string GetHidString(IntPtr handle, bool product)
        {
            var sb = new StringBuilder(256);
            bool ok = product
                ? NativeMethods.HidD_GetProductString(handle, sb, 512)
                : NativeMethods.HidD_GetManufacturerString(handle, sb, 512);
            return ok ? sb.ToString().Trim() : "";
        }

        private static string GetHidSerial(IntPtr handle)
        {
            var sb = new StringBuilder(256);
            return NativeMethods.HidD_GetSerialNumberString(handle, sb, 512) ? sb.ToString().Trim() : "";
        }

        private static byte[] GetDescriptor(IntPtr handle)
        {
            var buffer = new byte[8192];
            uint returned;
            if (!NativeMethods.DeviceIoControl(handle, NativeMethods.IOCTL_HID_GET_REPORT_DESCRIPTOR,
                IntPtr.Zero, 0, buffer, (uint)buffer.Length, out returned, IntPtr.Zero))
                return new byte[0];
            if (returned == 0) return new byte[0];
            var result = new byte[returned];
            Buffer.BlockCopy(buffer, 0, result, 0, (int)returned);
            return result;
        }

        private static bool TryGetCaps(IntPtr handle, out HidCaps caps)
        {
            caps = new HidCaps();
            IntPtr preparsed;
            if (!NativeMethods.HidD_GetPreparsedData(handle, out preparsed)) return false;
            try
            {
                return NativeMethods.HidP_GetCaps(preparsed, out caps) >= 0;
            }
            finally
            {
                NativeMethods.HidD_FreePreparsedData(preparsed);
            }
        }

        private static void ReadCaps(IntPtr handle, HidInterfaceInfo info)
        {
            IntPtr preparsed;
            if (!NativeMethods.HidD_GetPreparsedData(handle, out preparsed)) return;
            try
            {
                HidCaps caps;
                if (NativeMethods.HidP_GetCaps(preparsed, out caps) < 0) return;
                info.Usage = caps.Usage;
                info.UsagePage = caps.UsagePage;
                info.InputLength = caps.InputReportByteLength;
                info.OutputLength = caps.OutputReportByteLength;
                info.FeatureLength = caps.FeatureReportByteLength;
                ReadValueCaps(preparsed, 0, info.ValueCaps);
                ReadValueCaps(preparsed, 2, info.ValueCaps);
            }
            finally
            {
                NativeMethods.HidD_FreePreparsedData(preparsed);
            }
        }

        private static void ReadValueCaps(IntPtr preparsed, int reportType, List<HidValueCap> output)
        {
            int structSize = Marshal.SizeOf(typeof(NativeMethods.HidpValueCaps));
            const int capacity = 512;
            IntPtr buffer = Marshal.AllocHGlobal(structSize * capacity);
            try
            {
                ushort length = capacity;
                int status = NativeMethods.HidP_GetValueCaps(reportType, buffer, ref length, preparsed);
                if (status < 0) return;
                for (int i = 0; i < length; i++)
                {
                    IntPtr item = new IntPtr(buffer.ToInt64() + i * structSize);
                    NativeMethods.HidpValueCaps caps = (NativeMethods.HidpValueCaps)Marshal.PtrToStructure(item, typeof(NativeMethods.HidpValueCaps));
                    var value = new HidValueCap();
                    value.ReportType = reportType == 0 ? HidReportType.Input : HidReportType.Feature;
                    value.UsagePage = caps.UsagePage;
                    value.Usage = caps.IsRange != 0 ? caps.Union.UsageMin : caps.Union.Usage;
                    value.ReportId = caps.ReportID;
                    value.BitOffset = caps.BitField;
                    value.BitSize = caps.BitSize;
                    value.ReportCount = caps.ReportCount;
                    value.LogicalMin = caps.LogicalMin;
                    value.LogicalMax = caps.LogicalMax;
                    value.LinkCollection = caps.LinkCollection;
                    output.Add(value);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static string GetInstanceId(int devInst)
        {
            var sb = new StringBuilder(512);
            return NativeMethods.CM_Get_Device_IDW((uint)devInst, sb, sb.Capacity, 0) == 0 ? sb.ToString() : "";
        }

        private static string GetStringProperty(IntPtr set, ref NativeMethods.SpDevInfoData devInfo, NativeMethods.DevPropKey key)
        {
            IntPtr buffer = Marshal.AllocHGlobal(2048);
            try
            {
                uint type;
                uint required;
                if (!NativeMethods.SetupDiGetDevicePropertyW(set, ref devInfo, ref key, out type, buffer, 2048, out required, 0))
                    return "";
                if (type != 18) return "";
                return Marshal.PtrToStringUni(buffer) ?? "";
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static string GetGuidProperty(IntPtr set, ref NativeMethods.SpDevInfoData devInfo, NativeMethods.DevPropKey key)
        {
            IntPtr buffer = Marshal.AllocHGlobal(32);
            try
            {
                uint type;
                uint required;
                if (!NativeMethods.SetupDiGetDevicePropertyW(set, ref devInfo, ref key, out type, buffer, 32, out required, 0))
                    return "";
                if (type != 13) return "";
                Guid g = (Guid)Marshal.PtrToStructure(buffer, typeof(Guid));
                return g.ToString();
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static List<BluetoothBatteryInfo> EnumerateBluetoothBattery()
        {
            var result = new List<BluetoothBatteryInfo>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var deviceNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var deviceAddresses = new Dictionary<string, ulong>(StringComparer.OrdinalIgnoreCase);
            Dictionary<ulong, bool> connections = BluetoothConnection.Snapshot();
            // HFP battery properties can live on a System-class BTHENUM node,
            // not on the Bluetooth-class device shown in Settings.
            Guid guid = Guid.Empty;
            IntPtr set = NativeMethods.SetupDiGetClassDevsW(ref guid, IntPtr.Zero, IntPtr.Zero,
                NativeMethods.DIGCF_PRESENT | NativeMethods.DIGCF_ALLCLASSES);
            if (set == IntPtr.Zero || set == new IntPtr(-1)) return result;
            try
            {
                uint index = 0;
                while (true)
                {
                    var devInfo = new NativeMethods.SpDevInfoData();
                    devInfo.cbSize = Marshal.SizeOf(typeof(NativeMethods.SpDevInfoData));
                    if (!NativeMethods.SetupDiEnumDeviceInfo(set, index, ref devInfo)) break;
                    index++;

                    string instance = GetStringProperty(set, ref devInfo, NativeMethods.KeyInstanceId);
                    if (string.IsNullOrEmpty(instance)) instance = GetInstanceId(devInfo.DevInst);
                    if (!WindowsBatteryProperty.IsBluetoothInstance(instance)) continue;
                    string name = GetStringProperty(set, ref devInfo, NativeMethods.KeyFriendlyName);
                    string container = GetGuidProperty(set, ref devInfo, NativeMethods.KeyContainerId);
                    if (instance.StartsWith("BTHENUM\\DEV_", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(container))
                    {
                        if (!string.IsNullOrEmpty(name)) deviceNames[container] = name;
                        ulong address;
                        if (BluetoothConnection.TryParseAddress(instance, out address)) deviceAddresses[container] = address;
                    }

                    int percent;
                    if (!TryGetBattery(set, ref devInfo, out percent)) continue;
                    if (percent < 0 || percent > 100) continue;

                    string key = string.IsNullOrEmpty(container) ? instance : container;
                    if (!string.IsNullOrEmpty(key) && !seen.Add(key)) continue;

                    var info = new BluetoothBatteryInfo();
                    info.Name = string.IsNullOrEmpty(name) ? "蓝牙设备" : name;
                    info.Percent = percent;
                    info.ContainerId = container;
                    info.InstanceId = instance;
                    info.Kind = L10n.KindFromName(name);
                    if (info.Kind == DeviceKind.Other)
                    {
                        if (WindowsBatteryProperty.IsAudioInstance(instance)) info.Kind = DeviceKind.Headset;
                    }
                    result.Add(info);
                }
            }
            finally
            {
                NativeMethods.SetupDiDestroyDeviceInfoList(set);
            }
            foreach (BluetoothBatteryInfo info in result)
            {
                string deviceName;
                if (!string.IsNullOrEmpty(info.ContainerId) && deviceNames.TryGetValue(info.ContainerId, out deviceName))
                    info.Name = deviceName;
            }
            result.RemoveAll(info => !BluetoothConnection.CanUseCachedBattery(
                WindowsBatteryProperty.IsClassicBluetoothInstance(info.InstanceId),
                BluetoothConnection.State(info.ContainerId, deviceAddresses, connections)));
            return result;
        }

        private static bool TryGetBattery(IntPtr set, ref NativeMethods.SpDevInfoData devInfo, out int percent)
        {
            percent = -1;
            // {2BD67D8B-8BEB-48D5-87E0-6CDA3428040A},2 is a Bluetooth
            // service GUID on this stack, not a second battery property.
            NativeMethods.DevPropKey key = NativeMethods.KeyBluetoothBattery;
            IntPtr buffer = Marshal.AllocHGlobal(64);
            try
            {
                uint type;
                uint required;
                if (!NativeMethods.SetupDiGetDevicePropertyW(set, ref devInfo, ref key, out type, buffer, 64, out required, 0))
                    return false;
                if (required == 0 || required > 64) return false;
                var data = new byte[required];
                Marshal.Copy(buffer, data, 0, data.Length);
                int? value = WindowsBatteryProperty.Parse(type, data);
                if (!value.HasValue) return false;
                percent = value.Value;
                return true;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public static void SetTitleBarTheme(IntPtr hwnd, bool dark)
        {
            try
            {
                try
                {
                    if (dark)
                        NativeMethods.SetWindowTheme(hwnd, "DarkMode_Explorer", IntPtr.Zero);
                }
                catch
                {
                }

                int enabled = dark ? 1 : 0;
                int hr1 = NativeMethods.DwmSetWindowAttribute(hwnd, 20, ref enabled, 4);
                int hr2 = NativeMethods.DwmSetWindowAttribute(hwnd, 19, ref enabled, 4);
                int rounded = 2;
                int hr3 = NativeMethods.DwmSetWindowAttribute(hwnd, 33, ref rounded, 4);

                // Some systems use the accent color on title bars. Explicit caption
                // colors keep the title bar consistent with the app theme.
                int caption = dark ? 0x0019110B : 0x00FCF9F7;
                int captionText = dark ? 0x00F8F0E9 : 0x002B2017;
                int hr4 = NativeMethods.DwmSetWindowAttribute(hwnd, 35, ref caption, 4);
                int hr5 = NativeMethods.DwmSetWindowAttribute(hwnd, 36, ref captionText, 4);
                int hr6 = 0;

                Log.Debug("DWM title dark=" + dark + " hwnd=0x" + hwnd.ToString("X") + " hr=" + hr1 + "/" + hr2 + " round=" + hr3
                    + " cap=" + hr4 + "/" + hr5 + "/" + hr6);
            }
            catch (Exception ex)
            {
                Log.Debug("DWM title failed: " + ex.Message);
            }
        }

        public static void UseDarkTitleBar(IntPtr hwnd)
        {
            SetTitleBarTheme(hwnd, true);
        }
    }
}

