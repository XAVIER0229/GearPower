using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GearPower
{
    public static class SelfTest
    {
        public static bool Run()
        {
            var lines = new List<string>();
            int passed = 0;
            int failed = 0;

            Action<string, bool> check = (name, condition) =>
            {
                if (condition)
                {
                    passed++;
                    lines.Add("PASS  " + name);
                }
                else
                {
                    failed++;
                    lines.Add("FAIL  " + name);
                }
            };

            try
            {
                check("bluetooth byte battery 90", WindowsBatteryProperty.Parse(3, new byte[] { 90 }) == 90);
                check("bluetooth zero battery retained", WindowsBatteryProperty.Parse(3, new byte[] { 0 }) == 0);
                check("bluetooth full battery retained", WindowsBatteryProperty.Parse(3, new byte[] { 100 }) == 100);
                check("bluetooth byte outside percent rejected", !WindowsBatteryProperty.Parse(3, new byte[] { 101 }).HasValue);
                check("bluetooth uint32 battery", WindowsBatteryProperty.Parse(7, BitConverter.GetBytes((uint)90)) == 90);
                check("bluetooth uint16 battery", WindowsBatteryProperty.Parse(5, BitConverter.GetBytes((ushort)90)) == 90);
                check("bluetooth signed negative rejected", !WindowsBatteryProperty.Parse(6, BitConverter.GetBytes(-1)).HasValue);
                check("bluetooth truncated uint32 rejected", !WindowsBatteryProperty.Parse(7, new byte[] { 90 }).HasValue);
                check("bluetooth service GUID rejected", !WindowsBatteryProperty.Parse(13, new byte[16]).HasValue);
                check("bluetooth boolean not a level", !WindowsBatteryProperty.Parse(17, new byte[] { 1 }).HasValue);
                check("bluetooth security descriptor not a level", !WindowsBatteryProperty.Parse(19, new byte[4]).HasValue);
                check("bluetooth unicode percent", WindowsBatteryProperty.Parse(18, Encoding.Unicode.GetBytes("90%\0")) == 90);
                check("bluetooth odd unicode buffer rejected", !WindowsBatteryProperty.Parse(18, new byte[] { 90 }).HasValue);
                check("bluetooth missing data rejected", !WindowsBatteryProperty.Parse(3, null).HasValue);
                check("bluetooth HFP node included", WindowsBatteryProperty.IsBluetoothInstance("BTHENUM\\HFP_SERVICE"));
                check("bluetooth handsfree child included", WindowsBatteryProperty.IsBluetoothInstance("BTHHFENUM\\BTHHFPAUDIO"));
                check("bluetooth LE node included", WindowsBatteryProperty.IsBluetoothInstance("BTHLEDEVICE\\TEST"));
                check("non-bluetooth battery node excluded", !WindowsBatteryProperty.IsBluetoothInstance("ACPI\\PNP0C0A"));
                check("HFP service recognized as headset", WindowsBatteryProperty.IsAudioInstance("BTHENUM\\{0000111e-0000-1000-8000-00805f9b34fb}"));
                check("A2DP sink recognized as headset", WindowsBatteryProperty.IsAudioInstance("BTHENUM\\{0000110b-0000-1000-8000-00805f9b34fb}"));
                check("HID service not an audio device", !WindowsBatteryProperty.IsAudioInstance("BTHENUM\\{00001124-0000-1000-8000-00805f9b34fb}"));
                check("apple input90 level", AppleKeyboardProtocol.ParseBattery(new byte[] { 0x90, 0, 85 }) == 85);
                check("apple empty battery remains zero", AppleKeyboardProtocol.ParseBattery(new byte[] { 0x90, 0, 0 }) == 0);
                check("apple rejects wrong report", !AppleKeyboardProtocol.ParseBattery(new byte[] { 0x47, 0, 85 }).HasValue);
                check("apple rejects invalid level", !AppleKeyboardProtocol.ParseBattery(new byte[] { 0x90, 0, 255 }).HasValue);
                check("apple rejects truncated report", !AppleKeyboardProtocol.ParseBattery(new byte[] { 0x90, 85 }).HasValue);
                var appleGroup = new DeviceGroup();
                appleGroup.Interfaces.Add(new HidInterfaceInfo { Vid = 0x004C, Pid = 0x026C });
                check("apple matches bluetooth numeric keyboard", new AppleKeyboardProtocol().Matches(appleGroup));
                appleGroup.Interfaces[0].Vid = 0x05AC;
                check("apple matches USB numeric keyboard", new AppleKeyboardProtocol().Matches(appleGroup));
                appleGroup.Interfaces[0].Pid = 0x0340;
                check("apple excludes built-in keyboard", !new AppleKeyboardProtocol().Matches(appleGroup));
                appleGroup.Interfaces[0].Vid = 0x046D;
                appleGroup.Interfaces[0].Pid = 0x026C;
                check("apple excludes other vendors", !new AppleKeyboardProtocol().Matches(appleGroup));
                byte[] descriptor =
                {
                    0x05, 0x06, 0x09, 0x01, 0x15, 0x00, 0x25, 0x64, 0x75, 0x08, 0x95, 0x01, 0x85, 0x03, 0xB1, 0x02,
                    0x05, 0x06, 0x09, 0x20, 0x15, 0x00, 0x25, 0x64, 0x75, 0x08, 0x95, 0x01, 0x85, 0x03, 0xB1, 0x02
                };
                HidReportDescriptor parsed = HidReportDescriptor.Parse(descriptor);
                HidReportField battery = parsed.FindBattery();
                check("descriptor parses battery field", battery != null);
                check("battery field bit offset is 8", battery != null && battery.BitOffset == 8);
                check("battery field is a feature report", battery != null && battery.Type == HidReportType.Feature);
                check("descriptor uses report ids", parsed.UsesReportIds);

                byte[] featureBuffer = { 0x03, 0x05, 0x57 };
                check("feature battery extraction = 87",
                    HidReportDescriptor.ExtractBits(featureBuffer, 1, 8, 8, 100) == 87);
                byte[] unnumbered = { 0x05, 0x57 };
                check("unnumbered battery extraction = 87",
                    HidReportDescriptor.ExtractBits(unnumbered, 0, 8, 8, 100) == 87);

                byte[] razer = RazerProtocol.BuildRequest(91, 0x1F);
                check("razer request length 91", razer.Length == 91);
                check("razer request header", razer[6] == 0x02 && razer[7] == 0x07 && razer[8] == 0x80);
                check("razer checksum 0x85", razer[89] == 0x85);
                byte[] razerReply = new byte[91];
                razerReply[1] = 0x02;
                razerReply[10] = 128;
                check("razer battery = 50%", RazerProtocol.ParseBattery(razerReply) == 50);
                check("razer rejects empty reply", RazerProtocol.ParseBattery(new byte[3]) == null);
                byte[] razerHeadsetRequest = RazerHeadsetProtocol.BuildRequest(64);
                check("razer headset request header", razerHeadsetRequest.Length == 64 &&
                    razerHeadsetRequest[0] == 0x02 && razerHeadsetRequest[1] == 0x80 &&
                    razerHeadsetRequest[10] == 0x21);
                byte[] razerHeadsetReply = new byte[64];
                razerHeadsetReply[0] = 0x02;
                razerHeadsetReply[15] = 77;
                check("razer headset battery = 77%", RazerHeadsetProtocol.ParseBattery(razerHeadsetReply) == 77);
                razerHeadsetReply[15] = 0;
                check("razer headset rejects zero", RazerHeadsetProtocol.ParseBattery(razerHeadsetReply) == null);

                byte[] nordic = Nordic52Protocol.BuildRequest();
                check("nordic52 packet", nordic.Length == 17 && nordic[0] == 0x08 && nordic[1] == 0x04 && nordic[16] == 0x49);
                byte[] nordicReply = new byte[17];
                nordicReply[0] = 0x08;
                nordicReply[1] = 0x04;
                nordicReply[6] = 35;
                bool wired;
                int? nordicPercent = Nordic52Protocol.ParseBattery(nordicReply, out wired);
                check("nordic52 battery = 35%", nordicPercent == 35 && !wired);

                byte[] nordic54 = Nordic54Protocol.BuildRequest(1);
                check("nordic54 packet", nordic54.Length == 64 && nordic54[2] == 0x72 && nordic54[6] == 0x07);
                byte[] nordic54Reply = new byte[64];
                nordic54Reply[1] = 0x72;
                nordic54Reply[5] = 0x07;
                nordic54Reply[7] = 100;
                bool answered;
                check("nordic54 battery = 100%", Nordic54Protocol.ParseBattery(nordic54Reply, out answered) == 100 && answered);

                byte[] mchoseRequest = MchoseProtocol.BuildBatteryRequest(21);
                check("mchose request header", mchoseRequest.Length == 21 &&
                    mchoseRequest[0] == 0x11 && mchoseRequest[1] == 0xF4 && mchoseRequest[2] == 0x55);
                check("mchose caps 0x12 output to 0x11 size", MchoseProtocol.BuildBatteryRequest(64).Length == 21);
                byte[] mchosePayload = new byte[20];
                mchosePayload[0] = 0xE2;
                mchosePayload[1] = 0x01;
                mchosePayload[2] = 0x01;
                mchosePayload[3] = 0x00;
                mchosePayload[4] = 76;
                mchosePayload[6] = 0x02;
                byte[] mchoseName = Encoding.ASCII.GetBytes("K7 Ultra");
                Buffer.BlockCopy(mchoseName, 0, mchosePayload, 9, mchoseName.Length);
                byte[] mchosePacket = new byte[21];
                mchosePacket[0] = 0x13;
                for (int i = 0; i < 20; i++) mchosePacket[i + 1] = (byte)(mchosePayload[i] ^ 0xFF);
                ProbeResult mchose = MchoseProtocol.ParseStatus(mchosePacket, null);
                check("mchose battery = 76%", mchose != null && mchose.Percent == 76 && !mchose.Charging);
                check("mchose device name", mchose != null && mchose.ModelName == "K7 Ultra");

                var chargingLow = new DeviceReading { Name = "charging low", Percent = 5, Charging = true };
                var dischargingMid = new DeviceReading { Name = "discharging mid", Percent = 40 };
                var dischargingHigh = new DeviceReading { Name = "discharging high", Percent = 80 };
                var priorityList = new List<DeviceReading> { chargingLow, dischargingHigh, dischargingMid };
                check("priority skips charging low battery", DevicePriority.Primary(priorityList).Name == "discharging mid");
                check("priority sort puts charging last", DevicePriority.Sort(priorityList)[2].Name == "charging low");
                var allCharging = new List<DeviceReading>
                {
                    new DeviceReading { Name = "a", Percent = 30, Charging = true },
                    new DeviceReading { Name = "b", Percent = 5, Charging = true }
                };
                check("priority falls back when all charging", DevicePriority.Primary(allCharging).Name == "b");

                byte[] lamzu = new byte[9];
                lamzu[1] = 0xA1;
                lamzu[6] = 0x83;
                lamzu[7] = 1;
                lamzu[8] = 87;
                bool lamzuCharging;
                check("lamzu battery = 87% charging", LamzuProtocol.ParseBattery(lamzu, out lamzuCharging) == 87 && lamzuCharging);

                byte[] asus = new byte[12];
                asus[1] = 0x12;
                asus[2] = 0x07;
                asus[5] = 76;
                asus[10] = 1;
                bool asusCharging;
                bool asusFull;
                check("asus battery = 76% charging", AsusProtocol.ParseBattery(asus, out asusCharging, out asusFull) == 76 && asusCharging);

                byte[] hyperx = { 0x21, 0xBB, 0x0B, 55 };
                check("hyperx alpha battery = 55%", HyperXProtocol.ParseAlpha(hyperx) == 55);

                int flightPercent;
                bool flightCharging;
                byte[] flight = { 0x21, 0xFF, 0x05, 0x0E, 0x94 };
                bool flightOk = HyperXProtocol.ParseFlight(flight, out flightPercent, out flightCharging);
                check("hyperx flight battery = 25%", flightOk && flightPercent == 25 && !flightCharging);
                byte[] flightChargingPacket = { 0x21, 0xFF, 0x05, 0x10, 0x14 };
                bool chargingOk = HyperXProtocol.ParseFlight(flightChargingPacket, out flightPercent, out flightCharging);
                check("hyperx flight charging", chargingOk && flightCharging);

                int flight20Percent;
                bool flight20Charging;
                bool flight20Full;
                byte[] flight20 = { 0x21, 0xFF, 0x05, 0x0F, 0x50 };
                bool flight20Ok = HyperXProtocol.ParseFlight20(flight20, out flight20Percent, out flight20Charging, out flight20Full);
                check("hyperx flight 20-byte gauge = 85%", flight20Ok && flight20Percent == 85);
                byte[] flight20Mid = { 0x21, 0xFF, 0x05, 0x0E, 0xCD };
                HyperXProtocol.ParseFlight20(flight20Mid, out flight20Percent, out flight20Charging, out flight20Full);
                check("hyperx flight 20-byte mid = 50%", flight20Percent == 50);
                byte[] flight20ChargingPacket = { 0x21, 0xFF, 0x05, 0x10, 0x05 };
                bool flight20ChargeOk = HyperXProtocol.ParseFlight20(flight20ChargingPacket, out flight20Percent, out flight20Charging, out flight20Full);
                check("hyperx flight 20-byte charging", flight20ChargeOk && flight20Charging && flight20Percent == -1);

                check("known model name", L10n.DisplayName(0x373B, 0x1031, "USB Receiver") == "ATK F1 Ultimate");
                check("headset kind by name", L10n.KindFromName("HyperX Cloud III Wireless") == DeviceKind.Headset);
                check("generic name detection", L10n.IsGenericName("2.4G Wireless Receiver"));
                check("dock kind by name", L10n.KindFromName("HM HUB") == DeviceKind.Dock);
                check("keyboard kind by model", L10n.KindFromName("NINX_NX60ANSI") == DeviceKind.Keyboard);
                check("microphone kind by name", L10n.KindFromName("Razer Seiren V3 Mini") == DeviceKind.Microphone);
                check("known dock model", KnownDevices.Find(0xC019, 0xB300) != null &&
                    KnownDevices.Find(0xC019, 0xB300).Kind == DeviceKind.Dock);
                List<string> profileErrors;
                List<DeviceProfile> embeddedProfiles = ProfileStore.Load(out profileErrors);
                DeviceProfile profile = embeddedProfiles.FirstOrDefault(p => p.id == "hjsdzhi-s1-v2");
                check("name override path", !string.IsNullOrEmpty(NameOverrides.StoragePath));
                check("embedded profile preset exists", profile != null);
                if (profile != null)
                {
                    check("profile preset parses", profile.id == "hjsdzhi-s1-v2" && profile.matches.Length == 2);
                    DeviceProfile roundtrip = ProfileJson.Parse(ProfileJson.Pretty(profile));
                    check("profile roundtrip", roundtrip.id == profile.id && roundtrip.steps.Length == 2);
                }
                check("embedded hi75 profile", embeddedProfiles.Any(p => p.id == "leobog-hi75cpro"));
                check("embedded pelta profile", embeddedProfiles.Any(p => p.id == "asus-rog-pelta-2-4g"));
                DeviceProfile flydigi = embeddedProfiles.FirstOrDefault(p => p.id == "flydigi-vader-5-pro");
                check("embedded flydigi profile", flydigi != null);
                if (flydigi != null)
                {
                    check("flydigi battery nibble", flydigi.result.percent.offset == 12 &&
                        flydigi.result.percent.mask == 15 && flydigi.result.percent.scale == 10);
                }
                try
                {
                    ProfileJson.Parse("{\"schemaVersion\":1,\"id\":\"bad\",\"name\":\"bad\",\"unknown\":1}");
                    check("profile rejects unknown fields", false);
                }
                catch (FormatException)
                {
                    check("profile rejects unknown fields", true);
                }
                check("empty descriptor is safe", HidReportDescriptor.Parse(new byte[0]).Fields.Count == 0);
                check("garbage descriptor is safe", HidReportDescriptor.Parse(new byte[] { 0xFF, 0xFF, 0xFF, 0x00 }).Fields.Count == 0);
            }
            catch (Exception ex)
            {
                failed++;
                lines.Add("EXCEPTION  " + ex);
            }

            lines.Add("");
            lines.Add("RESULT  passed=" + passed + " failed=" + failed);
            try
            {
                string dir = Log.LogDirectory;
                if (string.IsNullOrEmpty(dir))
                {
                    string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    dir = string.IsNullOrEmpty(local)
                        ? Path.Combine(Path.GetTempPath(), "GearPower", "logs")
                        : Path.Combine(local, "GearPower", "logs");
                }
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, "selftest.log");
                File.WriteAllLines(path, lines, Encoding.UTF8);
                lines.Add("LOG  " + path);
            }
            catch
            {
            }
            return failed == 0;
        }
    }
}
