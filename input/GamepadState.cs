using System.Numerics;
using System.Runtime.InteropServices;

namespace input;

[StructLayout(LayoutKind.Sequential)]
public struct GamepadState {
    public ulong Timestamp { get; set; }
    public GamepadButtons Buttons { get; init; }
    public float LeftTrigger { get; init; }
    public float RightTrigger { get; init; }
    public Vector2 LeftThumbstick { get; init; }
    public Vector2 RightThumbstick { get; init; }
}