using System;
using JetBrains.Annotations;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class BsInputFieldContext : StateBase<BsInputFieldContext>, IState<BsInputFieldContext> {
    public string? Text {
        get;
        set {
            if (Validate != null && !Validate(value)) {
                return;
            }

            field = value;
            NotifyValueInvalidated();
        }
    }
    
    public Func<string?, bool>? Validate { get; set; }

    public bool Focused {
        get;
        set {
            field = value;
            NotifyValueInvalidated();
        }
    }

    BsInputFieldContext IState<BsInputFieldContext>.Value => this;
}