using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components.Basic;

/// <summary>
/// Layout wrapper for <see cref="Image"/>.
/// </summary>
[PublicAPI]
public class Background : ComponentLayout<Image> {
    #region Adapter

    public Sprite? Sprite {
        get => Component.Sprite;
        set => Component.Sprite = value;
    }

    public Color Color {
        get => Component.Color;
        set => Component.Color = value;
    }

    public Material? Material {
        get => Component.Material;
        set => Component.Material = value;
    }

    public bool PreserveAspect {
        get => Component.PreserveAspect;
        set => Component.PreserveAspect = value;
    }

    public UnityEngine.UI.Image.Type ImageType {
        get => Component.ImageType;
        set => Component.ImageType = value;
    }

    public UnityEngine.UI.Image.FillMethod FillMethod {
        get => Component.FillMethod;
        set => Component.FillMethod = value;
    }

    public float FillAmount {
        get => Component.FillAmount;
        set => Component.FillAmount = value;
    }

    public float PixelsPerUnit {
        get => Component.PixelsPerUnit;
        set => Component.PixelsPerUnit = value;
    }

    public bool RaycastTarget {
        get => Component.RaycastTarget;
        set => Component.RaycastTarget = value;
    }

    #endregion
    
    public Image WrappedImage => Component;
}