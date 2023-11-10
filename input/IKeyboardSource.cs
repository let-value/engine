namespace input;

public interface IKeyboardSource : IDisposable {
    public HashSet<IKeyboard> Keyboards { get; }
}