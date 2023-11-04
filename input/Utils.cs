using System.Management;
using Vortice.DirectInput;

namespace input;

public class Utils {
    public static DeviceInstance[] GetXInputDevices(DeviceInstance[] devices) {
        var query = "SELECT * FROM Win32_PNPEntity WHERE DeviceID LIKE '%IG_%'";
        using var searcher = new ManagementObjectSearcher(@"\\.\root\cimv2", query);
        var pnpDevices = searcher
            .Get()
            .Cast<ManagementObject>()
            .Select(pnpDevice => pnpDevice["DeviceID"]?.ToString() ?? string.Empty)
            .ToArray();

        return devices.Where(device => IsXInputDevice(device, pnpDevices)).ToArray();
    }

    private static bool IsXInputDevice(DeviceInstance device, string[] pnpDevices) {
        var guidBytes = device.ProductGuid.ToByteArray();
        var vid = BitConverter.ToUInt16(guidBytes, 0);
        var pid = BitConverter.ToUInt16(guidBytes, 2);

        var vidString = $"VID_{vid:X4}";
        var pidString = $"PID_{pid:X4}";

        return pnpDevices
            .Any(deviceId => deviceId.Contains(vidString) && deviceId.Contains(pidString) && deviceId.Contains("IG_"));
    }
}