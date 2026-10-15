using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class ColorPickerContext : StateBase<ColorPickerContext>, IState<ColorPickerContext> {
    public Color Color {
        get;
        set {
            field = value;
            NotifyValueChanged();
        }
    }

    public Color SelectedColor {
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
    
    ColorPickerContext IState<ColorPickerContext>.Value => this;
}