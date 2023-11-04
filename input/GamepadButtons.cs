using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace input;

[Flags]
public enum GamepadButtons : uint {
    None = 0,
    Menu = 1,
    View = 2,
    A = 4,
    B = 8,
    X = 16,
    Y = 32,
    DPadUp = 64,
    DPadDown = 128,
    DPadLeft = 256,
    DPadRight = 512,
    LeftShoulder = 1024,
    RightShoulder = 2048,
    LeftThumbstick = 4096,
    RightThumbstick = 8192,
    Paddle1 = 16384,
    Paddle2 = 32768,
    Paddle3 = 65536,
    Paddle4 = 131072
}