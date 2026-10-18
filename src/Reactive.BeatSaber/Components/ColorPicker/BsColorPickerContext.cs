using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class BsColorPickerContext : StateBase<BsColorPickerContext>, IState<BsColorPickerContext> {
    public Color Color {
        get;
        set {
            field = value;
            NotifyValueInvalidated();
        }
    }

    public Color SelectedColor {
        get;
        set {
            field = value;
            NotifyValueInvalidated();
        }
    }
    
    public bool Focused {
        get;
        set {
            field = value;
            NotifyValueInvalidated();
        }
    }
    
    BsColorPickerContext IState<BsColorPickerContext>.Value => this;
}