using input;
using Reactive.Bindings;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace winui.input;

public class WindowsGamepad(Windows.Gaming.Input.Gamepad gamepad) : IGamepad {
    public ReactiveProperty<GamepadState> State { get; set; } = new(
        initialValue: new(),
        mode: ReactivePropertyMode.DistinctUntilChanged,
        raiseEventScheduler: ImmediateScheduler.Instance
    );

    public Subject<GamepadEvent> Events { get; set; } = new();

    private IDisposable Subscription => State.Buffer(2, 1)
        .Where(buffer => buffer.Count == 2)
        .SelectMany(buffer => CompareStates(buffer[0], buffer[1]))
        .ObserveOn(ImmediateScheduler.Instance)
        .Subscribe(Events);

    private IEnumerable<GamepadEvent> CompareStates(GamepadState previous, GamepadState current) {
        var changedButtons = previous.Buttons ^ current.Buttons;
        foreach (GamepadButtons button in Enum.GetValues(typeof(GamepadButtons))) {
            if (changedButtons.HasFlag(button)) {
                yield return new GamepadButtonEvent {
                    EventType = current.Buttons.HasFlag(button)
                        ? GamepadButtonEventType.Down
                        : GamepadButtonEventType.Up,
                    Button = button
                };
            }
        }

        if (previous.LeftTrigger != current.LeftTrigger) {
            yield return new GamepadTriggerEvent {
                Axis = GamepadAxis.LeftTrigger,
                Value = current.LeftTrigger
            };
        }

        if (previous.RightTrigger != current.RightTrigger) {
            yield return new GamepadTriggerEvent {
                Axis = GamepadAxis.RightTrigger,
                Value = current.RightTrigger
            };
        }

        if (previous.LeftThumbstick != current.LeftThumbstick) {
            yield return new GamepadThumbstickEvent {
                Axis = GamepadAxis.LeftThumbstick,
                Value = current.LeftThumbstick
            };
        }

        if (previous.RightThumbstick != current.RightThumbstick) {
            yield return new GamepadThumbstickEvent {
                Axis = GamepadAxis.RightThumbstick,
                Value = current.RightThumbstick
            };
        }
    }

    public GamepadVibration Vibration {
        get => ToVibration(gamepad.Vibration);
        set => gamepad.Vibration = ToVibration(value);
    }

    public void PullState() {
        var reading = gamepad.GetCurrentReading();

        State.Value = new() {
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

    public void Dispose() {
        Subscription.Dispose();
        State.Dispose();
        Events.Dispose();
    }
}