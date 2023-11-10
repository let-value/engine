using System.Reactive.Subjects;
using input;
using Linearstar.Windows.RawInput;
using Reactive.Bindings;
using Vortice.DirectInput;

namespace winui.input;

public class WindowsKeyboard : IKeyboard {
    private readonly RawInputKeyboard Device;

    // TODO implement IKeyboard

    public WindowsKeyboard(WindowsKeyboardSource source, RawInputKeyboard device) {
        source.OnInput += OnInput;
        Device = device;
    }

    public void Dispose() {
        // TODO release managed resources here
    }

    private void OnInput(RawInputKeyboardData data) {
        if (data.Device?.VendorId != Device.VendorId && data.Device?.ProductId != Device.VendorId) {
            return;
        }

        //todo handle input event
    }

    public void PullState() {
        throw new NotImplementedException();
    }
}