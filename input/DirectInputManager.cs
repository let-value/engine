using System.Diagnostics;
using System.Text.Json;
using Vortice.DirectInput;

namespace input;

internal record struct DirectInputDevice(DeviceInstance Description, IDirectInputDevice8 Device) : IDisposable {
    public readonly void Dispose() {
        Device.Dispose();
    }
}

public class DirectInputManager {
    private IntPtr WindowHandle = IntPtr.Zero;
    private readonly IDirectInput8 DirectInput = DInput.DirectInput8Create();

    private readonly Dictionary<Guid, DirectInputDevice> Keyboards = new();
    private readonly Dictionary<Guid, DirectInputDevice> Gamepads = new();

    public void Initialize(IntPtr handle) {
        WindowHandle = handle;

        UpdateKeyboards();
        UpdateGamepads();
    }

    public void GetUpdates() {
        GetKeyboardUpdates();
        GetGamepadsUpdates();
    }

    private void UpdateKeyboards() {
        var keyboards = DirectInput
            .GetDevices(DeviceClass.Keyboard, DeviceEnumerationFlags.AttachedOnly)
            .Where(instance => instance.Type == DeviceType.Keyboard)
            .ToDictionary(instance => instance.InstanceGuid);

        foreach (var id in Keyboards.Keys.Where(key => !keyboards.ContainsKey(key))) {
            Keyboards[id].Dispose();
            Keyboards.Remove(id);
        }

        foreach (var id in keyboards.Keys.Where(key => !Keyboards.ContainsKey(key))) {
            var device = DirectInput.CreateDevice(id);
            device.SetCooperativeLevel(WindowHandle, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);
            device.Properties.BufferSize = 16;

            if (!DirectInput.IsDeviceAttached(id)) {
                device.Dispose();
                continue;
            }

            var result = device.SetDataFormat<RawKeyboardState>();

            if (!result.Success) {
                device.Dispose();
                continue;
            }

            Keyboards.Add(id, new(keyboards[id], device));
        }
    }

    private void UpdateGamepads() {
        var gamepads = DirectInput
            .GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly)
            .Where(deviceInstance =>
                deviceInstance.Type == DeviceType.Gamepad || deviceInstance.Type == DeviceType.Joystick)
            .ToDictionary(deviceInstance => deviceInstance.InstanceGuid);

        var xInput = Utils.GetXInputDevices(gamepads.Values.ToArray());

        foreach (var id in Gamepads.Keys.Where(key => !gamepads.ContainsKey(key))) {
            Gamepads[id].Dispose();
            Gamepads.Remove(id);
        }

        foreach (var id in gamepads.Keys.Where(key => !Gamepads.ContainsKey(key))) {
            var device = DirectInput.CreateDevice(id);
            device.SetCooperativeLevel(WindowHandle, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);
            device.Properties.BufferSize = 16;

            if (!DirectInput.IsDeviceAttached(id)) {
                device.Dispose();
                continue;
            }

            var result = device.SetDataFormat<RawJoystickState>();

            if (!result.Success) {
                device.Dispose();
                continue;
            }

            Gamepads.Add(id, new(gamepads[id], device));
        }
    }

    private void GetKeyboardUpdates() {
        foreach (var keyboard in Keyboards.Values.Select(x => x.Device)) {
            var result = keyboard.Poll();

            if (result.Failure) {
                result = keyboard.Acquire();

                if (result.Failure)
                    continue;
            }

            try {
                var bufferedData = keyboard.GetBufferedKeyboardData();

                if (bufferedData.Length > 0) {
                    Debug.WriteLine(string.Join('\n', bufferedData.Select(x => JsonSerializer.Serialize(x))));
                }
            }
            catch {
                //
            }
        }
    }

    private void GetGamepadsUpdates() {
        foreach (var (id, instance) in Gamepads) {
            var (_, gamepad) = instance;
            var result = gamepad.Poll();

            if (result.Failure) {
                result = gamepad.Acquire();

                if (result.Failure)
                    continue;
            }

            try {
                var bufferedData = gamepad.GetBufferedJoystickData();

                if (bufferedData.Length > 0) {
                    Debug.WriteLine(string.Join('\n', bufferedData.Select(x => JsonSerializer.Serialize(x))));
                }
            }
            catch {
                //
            }
        }
    }
}