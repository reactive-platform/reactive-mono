using JetBrains.Annotations;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class BsInputFieldContext : StateBase<BsInputFieldContext>, IState<BsInputFieldContext> {
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

    BsInputFieldContext IState<BsInputFieldContext>.Value => this;
}