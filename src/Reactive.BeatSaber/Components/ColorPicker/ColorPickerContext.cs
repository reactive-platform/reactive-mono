using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class ColorPickerContext : StateBase<ColorPickerContext>, IState<ColorPickerContext> {
    public Color Color {
        get;
        set {
            field = value;
            NotifyValueChanged(this);
        }
    }

    public Color SelectedColor {
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
    
    ColorPickerContext IState<ColorPickerContext>.Value => this;
}