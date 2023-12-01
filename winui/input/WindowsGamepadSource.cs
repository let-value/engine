using input;
using System.Collections.Concurrent;
using Windows.Gaming.Input;

namespace winui.input;

public class WindowsGamepadSource : IGamepadSource {
    public HashSet<IGamepad> Gamepads { get; } = new();

    private readonly ConcurrentDictionary<Gamepad, WindowsGamepad> Lookup = new();

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
        var ok = Lookup.TryRemove(gamepad, out var windowsGamepad);

        if (!ok || windowsGamepad == null) {
            return;
        }

        Gamepads.Remove(windowsGamepad);
        windowsGamepad.Dispose();
    }

    private void OnGamepadAdded(object? sender, Gamepad gamepad) {
        if (Lookup.ContainsKey(gamepad)) {
            return;
        }

        var windowsGamepad = new WindowsGamepad(gamepad);
        Gamepads.Add(windowsGamepad);
        Lookup.TryAdd(gamepad, windowsGamepad);
    }

    public void PullState() {
        foreach (var gamepad in Gamepads) {
            gamepad.PullState();
        }
    }
}