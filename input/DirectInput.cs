using Vortice.DirectInput;

namespace input;

public record DirectInput {
    public readonly IDirectInput8 NativeDirectInput = DInput.DirectInput8Create();
}