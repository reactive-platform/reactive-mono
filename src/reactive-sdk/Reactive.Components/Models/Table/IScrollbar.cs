using System;

namespace Reactive.Components;

public interface IScrollbar {
    float PageHeight { set; }
    float Progress { set; }

    bool CanScrollUp { set; }
    bool CanScrollDown { set; }

    event Action? ScrollBackwardButtonPressedEvent;
    event Action? ScrollForwardButtonPressedEvent;

    void SetActive(bool active);
}