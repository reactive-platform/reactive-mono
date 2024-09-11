using System;

namespace Reactive.Components {
    [Flags]
    public enum GraphicState {
        None = 0,
        NonInteractable = 1,
        Hovered = 2,
        Active = 4,
        Pressed = 8
    }
}