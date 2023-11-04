namespace input;

public interface IGamepadSource : IDisposable {
    public HashSet<IGamepad> Gamepads { get; }
}