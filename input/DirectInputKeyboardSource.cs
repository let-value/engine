using Vortice.DirectInput;

namespace input;

public class DirectInputKeyboardSource : IKeyboardSource {
    public HashSet<IKeyboard> Keyboards { get; } = new();

    public DirectInputKeyboardSource(DirectInput directInput) {
        var keyboards = directInput.NativeDirectInput
            .GetDevices(DeviceClass.Keyboard, DeviceEnumerationFlags.AttachedOnly)
            .Where(instance => instance.Type == DeviceType.Keyboard);

        foreach (var instance in keyboards) {
            var keyboard = DirectInputKeyboard.Create(directInput, instance);
            if (keyboard != null) {
                Keyboards.Add(keyboard);
            }
        }
    }
}