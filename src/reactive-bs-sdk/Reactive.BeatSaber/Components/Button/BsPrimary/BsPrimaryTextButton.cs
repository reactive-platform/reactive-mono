using System;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using TMPro;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

/// <summary>
/// Beat Saber styled button with a label.
/// </summary>
[PublicAPI]
public class BsPrimaryTextButton : ReactiveComponent, ISkewedComponent, IInteractableComponent, IComponentHolder<Label> {
    #region Adapter

    public string Text {
        get => _label.Text;
        set => _label.Text = value;
    }

    public bool RichText {
        get => _label.RichText;
        set => _label.RichText = value;
    }

    public float FontSize {
        get => _label.FontSize;
        set => _label.FontSize = value;
    }

    public float FontSizeMin {
        get => _label.FontSizeMin;
        set => _label.FontSizeMin = value;
    }

    public float FontSizeMax {
        get => _label.FontSizeMax;
        set => _label.FontSizeMax = value;
    }

    public bool EnableAutoSizing {
        get => _label.EnableAutoSizing;
        set => _label.EnableAutoSizing = value;
    }

    public FontStyles FontStyle {
        get => _label.FontStyle;
        set => _label.FontStyle = value;
    }

    public TMP_FontAsset Font {
        get => _label.Font;
        set => _label.Font = value;
    }

    public bool EnableWrapping {
        get => _label.EnableWrapping;
        set => _label.EnableWrapping = value;
    }

    public TextOverflowModes Overflow {
        get => _label.Overflow;
        set => _label.Overflow = value;
    }

    public TextAlignmentOptions Alignment {
        get => _label.Alignment;
        set => _label.Alignment = value;
    }
    
    public float Skew {
        get => _button.Skew;
        set => _button.Skew = value;
    }

    public bool Interactable {
        get => _button.Interactable;
        set => _button.Interactable = value;
    }

    public Action? OnClick {
        get => _button.OnClick;
        set => _button.OnClick = value;
    }

    #endregion

    #region Setup

    Label IComponentHolder<Label>.Component => _label;

    private Label _label = null!;
    private BsPrimaryButton _button = null!;

    protected override GameObject Construct() {
        return new BsPrimaryButton {
            ConstructContent = (graphic, skew) => new Label {
                Do = x => x
                    .AsFlexItem()
                    .Bind(ref _label),
                
                sFontStyle = skew.Map(x => FontStyle | (x > 0 ? FontStyles.Italic : FontStyles.Normal)),
                sColor = graphic.MapColorSet(BeatSaberStyle.BsPrimaryButton.ContentColors).In()
            }
        }.Bind(ref _button).Use();
    }

    #endregion
}