using JetBrains.Annotations;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class InputFieldContext : StateBase<InputFieldContext>, IState<InputFieldContext> {
    public string? Text {
        get;
        set {
            field = value;
            NotifyValueChanged(this);
        }
    }

    public bool Focused {
        get;
        set {
            field = value;
            NotifyValueChanged(this);
        }
    }

    InputFieldContext IState<InputFieldContext>.Value => this;
}