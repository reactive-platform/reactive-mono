using System;
using JetBrains.Annotations;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class InputFieldContext : IState<InputFieldContext> {
    public string? Text {
        get;
        set {
            field = value;
            NotifyValueChanged();
        }
    }

    public bool Focused {
        get;
        set {
            field = value;
            NotifyValueChanged();
        }
    }

    #region State Impl

    InputFieldContext IState<InputFieldContext>.Value => this;
    
    public event Action<InputFieldContext>? ValueChangedEvent;
    public event Action? StateUpdatedEvent;

    private void NotifyValueChanged() {
        ValueChangedEvent?.Invoke(this);
        StateUpdatedEvent?.Invoke();
    }

    #endregion
}