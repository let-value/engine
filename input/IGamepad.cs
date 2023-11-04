namespace input;

public interface IGamepad {
    public GamepadVibration Vibration { get; set; }
    public GamepadState GetState();
}