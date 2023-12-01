using input;
using Linearstar.Windows.RawInput;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace winui.input;

public delegate void KeyboardInput(RawInputKeyboardData data);

public partial class WindowsKeyboardSource : IKeyboardSource {
    public event KeyboardInput? OnInput;
    public HashSet<IKeyboard> Keyboards { get; } = new();
    public ConcurrentDictionary<RawInputKeyboard, IKeyboard> Lookup { get; } = new();

    public WindowsKeyboardSource() {
        var devices = RawInputDevice.GetDevices();
        var keyboards = devices.OfType<RawInputKeyboard>();

        foreach (var keyboard in keyboards) {
            var windowsKeyboard = new WindowsKeyboard(this, keyboard);
            Keyboards.Add(windowsKeyboard);
            Lookup.TryAdd(keyboard, windowsKeyboard);
        }
    }

    private void UpdateDevices() {
        var devices = RawInputDevice.GetDevices();
        var keyboards = devices.OfType<RawInputKeyboard>().ToArray();

        foreach (var keyboard in keyboards) {
            if (Lookup.ContainsKey(keyboard)) {
                continue;
            }

            var windowsKeyboard = new WindowsKeyboard(this, keyboard);
            Keyboards.Add(windowsKeyboard);
            Lookup.TryAdd(keyboard, windowsKeyboard);
        }

        foreach (var keyboard in Lookup.Keys) {
            if (keyboards.Contains(keyboard)) {
                continue;
            }

            var ok = Lookup.TryRemove(keyboard, out var windowsKeyboard);
            if (!ok || windowsKeyboard is null) {
                continue;
            }

            Keyboards.Remove(windowsKeyboard);
            windowsKeyboard.Dispose();
        }
    }

    public void Dispose() {
        RawInputDevice.UnregisterDevice(HidUsageAndPage.Keyboard);
    }

    public void Initialize(IntPtr handle) {
        RawInputDevice.RegisterDevice(
            HidUsageAndPage.Keyboard,
            RawInputDeviceFlags.DevNotify | RawInputDeviceFlags.NoLegacy,
            handle
        );

        var subClassDelegate = new Subclassproc(WindowSubClass);
        var ok = SetWindowSubclass(handle, subClassDelegate, 0, 0);
        if (!ok) {
            throw new("Failed to subclass window");
        }
    }

    private const uint WmInputDeviceChange = 0x00FE;
    private const uint WmInput = 0x00FF;

    private int WindowSubClass(
        IntPtr hwnd,
        uint msg,
        IntPtr wparam,
        IntPtr lparam,
        IntPtr uidsubclass,
        uint dwrefdata
    ) {
        switch (msg) {
            case WmInputDeviceChange:
                UpdateDevices();

                break;
            case WmInput:
                var data = RawInputData.FromHandle(lparam);

                if (data is RawInputKeyboardData keyboard) {
                    OnInput?.Invoke(keyboard);
                }

                break;
        }

        return DefSubclassProc(hwnd, msg, wparam, lparam);
    }

    public delegate int Subclassproc(
        IntPtr hWnd,
        uint uMsg,
        IntPtr wParam,
        IntPtr lParam,
        IntPtr uIdSubclass,
        uint dwRefData
    );

    [LibraryImport("Comctl32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowSubclass(
        IntPtr hWnd,
        Subclassproc pfnSubclass,
        uint uIdSubclass,
        uint dwRefData
    );

    [LibraryImport("Comctl32.dll", SetLastError = true)]
    public static partial int DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
}