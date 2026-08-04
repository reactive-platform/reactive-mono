using System;

namespace Reactive.Components {
    [Flags]
    public enum GraphicState {
        None = 0,
        Hovered = 1,
        Active = 2,
        NonInteractable = 4,
    }
}