using Windows.Gaming.Input;
using input;

namespace winui.input;

public class WindowsGamepadSource : IGamepadSource {
    public HashSet<IGamepad> Gamepads { get; } = new();
    private readonly Dictionary<Gamepad, WindowsGamepad> Lookup = new();

    public WindowsGamepadSource() {
        Gamepad.GamepadAdded += OnGamepadAdded;
        Gamepad.GamepadRemoved += OnGamepadRemoved;

        foreach (var gamepad in Gamepad.Gamepads) {
            OnGamepadAdded(null, gamepad);
        }
    }

    public void Dispose() {
        Gamepad.GamepadAdded -= OnGamepadAdded;
        Gamepad.GamepadRemoved -= OnGamepadRemoved;
    }

    private void OnGamepadRemoved(object? sender, Gamepad gamepad) {
        if (!Lookup.TryGetValue(gamepad, out var windowsGamepad)) {
            return;
        }

        Gamepads.Remove(windowsGamepad);
        Lookup.Remove(gamepad);
    }

    private void OnGamepadAdded(object? sender, Gamepad gamepad) {
        if (Lookup.ContainsKey(gamepad)) {
            return;
        }

        var windowsGamepad = new WindowsGamepad(gamepad);
        Gamepads.Add(windowsGamepad);
        Lookup.Add(gamepad, windowsGamepad);
    }
}