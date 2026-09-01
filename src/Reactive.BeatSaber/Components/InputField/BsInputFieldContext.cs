using JetBrains.Annotations;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class BsInputFieldContext : StateBase<BsInputFieldContext>, IState<BsInputFieldContext> {
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

    BsInputFieldContext IState<BsInputFieldContext>.Value => this;
}