using System.Numerics;

namespace input;

public abstract class GamepadEvent;

public enum GamepadButtonEventType {
    Down,
    Up,
}

public class GamepadButtonEvent : GamepadEvent {
    public GamepadButtonEventType EventType { get; init; }
    public GamepadButtons Button { get; init; }
}

public class GamepadTriggerEvent : GamepadEvent {
    public GamepadAxis Axis { get; init; }
    public float Value { get; init; }
}

public class GamepadThumbstickEvent : GamepadEvent {
    public GamepadAxis Axis { get; init; }
    public Vector2 Value { get; init; }
}