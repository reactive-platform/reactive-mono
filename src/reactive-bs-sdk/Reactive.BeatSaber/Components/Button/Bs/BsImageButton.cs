using System;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

/// <summary>
/// Beat Saber styled button with an image.
/// </summary>
[PublicAPI]
public class BsImageButton : ReactiveComponent, ISkewedComponent, IInteractableComponent, IComponentHolder<Image> {
    #region Adapter

    public Sprite? Sprite {
        get => _image.Sprite;
        set => _image.Sprite = value;
    }

    public Material? Material {
        get => _image.Material;
        set => _image.Material = value;
    }

    public bool PreserveAspect {
        get => _image.PreserveAspect;
        set => _image.PreserveAspect = value;
    }

    public UnityEngine.UI.Image.Type ImageType {
        get => _image.ImageType;
        set => _image.ImageType = value;
    }

    public UnityEngine.UI.Image.FillMethod FillMethod {
        get => _image.FillMethod;
        set => _image.FillMethod = value;
    }

    public float FillAmount {
        get => _image.FillAmount;
        set => _image.FillAmount = value;
    }

    public float PixelsPerUnit {
        get => _image.PixelsPerUnit;
        set => _image.PixelsPerUnit = value;
    }

    public bool ShowUnderline {
        get => _button.ShowUnderline;
        set => _button.ShowUnderline = value;
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

    Image IComponentHolder<Image>.Component => _image;

    private Image _image = null!;
    private BsButton _button = null!;

    protected override GameObject Construct() {
        return new BsButton {
            ConstructContent = (graphic, skew) => new Image {
                    sSkew = skew.In(),
                    sColor = graphic.MapColorSet(BeatSaberStyle.BsButton.ContentColors).In(),
                    PreserveAspect = true
                }
                .AsFlexItem()
                .Bind(ref _image)
        }.Bind(ref _button).Use();
    }

    #endregion
}