using input;

namespace winui.input;

internal class WindowsGamepad(Windows.Gaming.Input.Gamepad gamepad) : IGamepad {
    public GamepadVibration Vibration {
        get => ToVibration(gamepad.Vibration);
        set => gamepad.Vibration = ToVibration(value);
    }

    public GamepadState GetState() {
        var reading = gamepad.GetCurrentReading();

        return new() {
            Timestamp = reading.Timestamp,
            Buttons = (GamepadButtons)reading.Buttons,
            LeftTrigger = (float)reading.LeftTrigger,
            RightTrigger = (float)reading.RightTrigger,
            LeftThumbstick = new((float)reading.LeftThumbstickX, (float)reading.LeftThumbstickY),
            RightThumbstick = new((float)reading.RightThumbstickX, (float)reading.RightThumbstickY)
        };
    }

    public GamepadVibration ToVibration(in Windows.Gaming.Input.GamepadVibration vibration) => new() {
        LeftMotor = (float)vibration.LeftMotor,
        LeftTrigger = (float)vibration.LeftTrigger,
        RightMotor = (float)vibration.RightMotor,
        RightTrigger = (float)vibration.RightTrigger
    };

    private Windows.Gaming.Input.GamepadVibration ToVibration(in GamepadVibration vibration) =>
        new() {
            LeftMotor = vibration.LeftMotor,
            LeftTrigger = vibration.LeftTrigger,
            RightMotor = vibration.RightMotor,
            RightTrigger = vibration.RightTrigger
        };
}