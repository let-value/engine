using Vortice.DirectInput;

namespace input;

public class DirectInputKeyboard(IDirectInputDevice8 device) : IKeyboard {
    private readonly IDirectInputDevice8 Device = device;
    private readonly KeyboardState State = device.GetCurrentKeyboardState();

    public KeyboardState GetState() {
        var result = device.Poll();

        if (result.Failure) {
            result = device.Acquire();
            if (result.Failure) {
                throw new Exception("Failed to acquire keyboard");
            }
        }

        var bufferedData = device.GetBufferedKeyboardData();

        foreach (var update in bufferedData) {
            State.Update(update);
        }

        return State;
    }

    private VirtualKey ConvertKeyToVirtualKey(Key key) {
        return key switch {
            Key.CapsLock => VirtualKey.CapitalLock,
            Key.D1 => VirtualKey.Number1,
            Key.D2 => VirtualKey.Number2,
            Key.D3 => VirtualKey.Number3,
            Key.D4 => VirtualKey.Number4,
            Key.D5 => VirtualKey.Number5,
            Key.D6 => VirtualKey.Number6,
            Key.D7 => VirtualKey.Number7,
            Key.D8 => VirtualKey.Number8,
            Key.D9 => VirtualKey.Number9,
            Key.D0 => VirtualKey.Number0,
            _ => (VirtualKey)Enum.Parse(typeof(VirtualKey), key.ToString())
        };
    }

    public static DirectInputKeyboard? Create(DirectInput directInput, DeviceInstance instance) {
        var device = directInput.NativeDirectInput.CreateDevice(instance.InstanceGuid);
        device.SetCooperativeLevel(IntPtr.Zero, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground);
        device.Properties.BufferSize = 16;

        if (!directInput.NativeDirectInput.IsDeviceAttached(instance.InstanceGuid)) {
            device.Dispose();
            return null;
        }

        var result = device.SetDataFormat<RawKeyboardState>();

        if (!result.Success) {
            device.Dispose();
            return null;
        }

        return new(device);
    }
}