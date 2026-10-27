using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using GearPower;

internal static class HistoryIntegrationTests
{
    private static int count;
    private static string historyFile;
    private static void Check(string name, bool ok)
    { if (!ok) throw new Exception(name); Console.WriteLine("PASS " + name); count++; }
    private static DeviceReading Reading(string id, string hardware, int percent)
    { return new DeviceReading { Id=id, HardwareId=hardware, Name="PRO X Wireless", OriginalName="PRO X Wireless",
        Vid=0x046D, Pid=0xC547, Kind=DeviceKind.Headset, Vendor="Logitech", Transport="2.4G",
        Protocol="Logitech HID++ 2.0", Percent=percent, Updated=DateTime.Now }; }
    private static void Clear()
    {
        ((IDictionary)typeof(DeviceHistory).GetField("Entries", BindingFlags.NonPublic|BindingFlags.Static).GetValue(null)).Clear();
        ((IDictionary)typeof(NameOverrides).GetField("Names", BindingFlags.NonPublic|BindingFlags.Static).GetValue(null)).Clear();
        File.WriteAllText(historyFile, "# empty test fixture\n");
    }
    private static int Main(string[] args)
    {
        try
        {
            string folder=Path.GetFullPath(args[0]); Directory.CreateDirectory(folder);
            historyFile=Path.Combine(folder,"device-history.tsv");
            typeof(DeviceHistory).GetField("FilePath",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,historyFile);
            typeof(NameOverrides).GetField("FilePath",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,Path.Combine(folder,"names.tsv"));
            typeof(NameOverrides).GetField("loaded",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,true);
            Clear();
            var old=Reading("old-port","unit-1",94); old.Kind=DeviceKind.Keyboard;
            DeviceHistory.Merge(new[]{old}); NameOverrides.Set(old.Id,"My PRO X");
            var current=Reading("new-port","unit-1",79);
            var merged=DeviceHistory.Merge(new[]{current});
            Check("USB port change leaves one card",merged.Count==1 && merged[0].Id=="new-port");
            Check("fresh level and corrected kind retained",merged[0].Percent==79 && merged[0].Kind==DeviceKind.Headset);
            Check("custom name migrated",NameOverrides.Get("new-port")=="My PRO X");
            Check("original history backed up",File.Exists(historyFile+".before-identity-migration.bak"));
            Check("hardware id persisted",File.ReadAllText(historyFile).Contains("unit-1"));
            ((IDictionary)typeof(DeviceHistory).GetField("Entries",BindingFlags.NonPublic|BindingFlags.Static).GetValue(null)).Clear();
            typeof(DeviceHistory).GetMethod("Load",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,null);
            merged=DeviceHistory.Merge(new[]{Reading("third-port","unit-1",78)});
            Check("restart and next port change still one card",merged.Count==1 && merged[0].Id=="third-port");
            merged=DeviceHistory.Merge(new DeviceReading[0]);
            Check("disconnect retains a single correct history",merged.Count==1 && merged[0].Disconnected && merged[0].Percent==78);
            Clear();
            DeviceHistory.Merge(new[]{Reading("legacy",null,94)});
            current=Reading("current","unit-1",79);
            merged=DeviceHistory.Merge(new[]{current});
            Check("legacy same name not silently merged",merged.Count==2);
            Check("explicit legacy merge succeeds",DeviceHistory.MergeConfirmed("legacy",current));
            merged=DeviceHistory.Merge(new[]{current});
            Check("legacy duplicate gone after confirmation",merged.Count==1 && merged[0].Percent==79);
            Check("repeat merge is harmless",!DeviceHistory.MergeConfirmed("legacy",current));
            Clear();
            DeviceHistory.Merge(new[]{Reading("other-unit","unit-2",94)});
            merged=DeviceHistory.Merge(new[]{current});
            Check("same model different hardware kept separate",merged.Count==2);
            Check("different hardware merge rejected",!DeviceHistory.MergeConfirmed("other-unit",current));
            Clear();
            merged=DeviceHistory.Merge(new[]{Reading("first","unit-1",79),Reading("second","unit-2",81)});
            Check("two connected same-model devices retained",merged.Count==2 && merged.All(r=>!r.Disconnected));
            Console.WriteLine("RESULT passed="+count+" failed=0"); return 0;
        }
        catch(Exception ex) { Console.WriteLine("FAIL "+ex); return 1; }
    }
}
