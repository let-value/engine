using System.Runtime.InteropServices;

namespace input;

[StructLayout(LayoutKind.Sequential)]
public record struct GamepadVibration {
    public float LeftMotor { get; init; }
    public float RightMotor { get; init; }
    public float LeftTrigger { get; init; }
    public float RightTrigger { get; init; }
}