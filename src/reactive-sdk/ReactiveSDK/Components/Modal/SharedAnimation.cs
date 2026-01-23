using System;

namespace Reactive.Components;

// TODO: remove after modals rework
public class SharedAnimation(Action<SharedAnimation> start) {
    public event Action? AnimationFinishedEvent;

    public void Play() {
        start(this);
    }

    public void NotifyFinished() {
        AnimationFinishedEvent?.Invoke();
    }
}