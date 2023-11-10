namespace input;

public interface IGamepad : IDisposable {
    public GamepadVibration Vibration { get; set; }
    public void PullState(); //GamepadState
}