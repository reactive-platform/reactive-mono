using System;
using System.Collections.Generic;
using HMUI;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

public record struct BsAeroButtonColors(
    CompositeColorSet BackgroundColors,
    ColorSet ContentColors
);

/// <summary>
/// A Beat Saber styled button without any content.
/// </summary>
[PublicAPI]
public partial class BsAeroButton : ReactiveComponent, ILayoutDriver, ISkewedComponent, IInteractableComponent {
    public delegate ILayoutItem ConstructContentDelegate(IState<Color> color, IState<float> skew);

    #region Public API

    public float Skew {
        get => _skew.Value;
        set => _skew.Value = value;
    }

    public bool Interactable {
        get => _graphicState.IsInteractable;
        set => _graphicState.IsInteractable = value;
    }

    public BsAeroButtonColors Colors {
        get => _colors.Value;
        set => _colors.Value = value;
    }

    [Required]
    public ConstructContentDelegate ConstructContent {
        init {
            var layoutItem = value(_contentColor, _skew);
            _background.Children.Add(layoutItem);
        }
    }

    public Action? OnClick { get; set; }

    #endregion

    #region Layout Driver

    public ICollection<ILayoutItem> Children { get; } = Array.Empty<ILayoutItem>();

    public ILayoutController? LayoutController {
        get => _background.LayoutController;
        set => _background.LayoutController = value;
    }

    #endregion

    #region Setup

    private State<float> _skew = null!;
    private State<GraphicState> _graphicState = null!;
    private State<BsAeroButtonColors> _colors = null!;
    private IState<Color> _contentColor = null!;

    private Background _background = null!;

    protected override GameObject Construct() {
        _skew = Remember(BeatSaberStyle.Skew);
        _graphicState = Remember(GraphicState.None);
        _colors = Remember(BeatSaberStyle.BsAeroButtonColors);

        var bgColor = _graphicState.MapColorSet(_colors, x => x.BackgroundColors);
        _contentColor = _graphicState.MapColorSet(_colors, x => x.ContentColors);

        return new Background {
            FlexController = {
                JustifyContent = Justify.SpaceAround,
                Padding = new() { left = 1.pt, right = 1.pt }
            },

            FlexItem = {
                Size = new() { y = 8.pt }
            },

            sColors = bgColor.In(),

            UseGradient = true,
            GradientDirection = ImageView.GradientDirection.Vertical,
            Sprite = BeatSaberResources.Sprites.background,
            PixelsPerUnit = 12f,

            sSkew = _skew,

            Do = x => x.WithPointerEvents(
                onEnter: _ => _graphicState.IsHovered = true,
                onLeave: _ => _graphicState.IsHovered = false,
                onDown: _ => {
                    if (_graphicState.IsInteractable) {
                        GameResources.ButtonClickSignal.Raise();
                        OnClick?.Invoke();
                    }
                })
        }.Bind(ref _background).Use();
    }

    #endregion
}