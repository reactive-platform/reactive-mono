using System;
using System.Collections.Generic;
using HMUI;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

public record struct BsKeyboardButtonColors(
    CompositeColorSet BackgroundColors,
    ColorSet BorderColors,
    ColorSet ContentColors
);

[PublicAPI]
public partial class BsKeyboardButton : ReactiveComponent, ILayoutDriver {
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

    public BsKeyboardButtonColors Colors {
        get => _colors.Value;
        set => _colors.Value = value;
    }

    [Required]
    public ConstructContentDelegate ConstructContent {
        init {
            var layoutItem = value(_contentColor, _skew);
            _driver.Children.Add(layoutItem);
        }
    }

    public Action? OnClick { get; set; }

    #endregion

    #region Layout Driver

    public ICollection<ILayoutItem> Children { get; } = Array.Empty<ILayoutItem>();

    public ILayoutController? LayoutController {
        get => _driver.LayoutController;
        set => _driver.LayoutController = value;
    }

    #endregion

    #region Setup

    private State<float> _skew = null!;
    private State<GraphicState> _graphicState = null!;
    private State<BsKeyboardButtonColors> _colors = null!;
    private IState<Color> _contentColor = null!;

    private ILayoutDriver _driver = null!;

    protected override GameObject Construct() {
        _skew = Remember(0f);
        _graphicState = Remember(GraphicState.None);
        _colors = Remember(BeatSaberStyle.BsKeyboardColors.KeyColors);
        
        var bgColor = _graphicState.MapColorSet(_colors, x => x.BackgroundColors);
        var borderColor = _graphicState.MapColorSet(_colors, x => x.BorderColors);
        _contentColor = _graphicState.MapColorSet(_colors, x => x.ContentColors);
        
        return new Background {
            FlexController = {
                JustifyContent = Justify.SpaceAround,
                Padding = new() { left = 1.pt, right = 1.pt }
            },

            FlexItem = {
                Size = new() { y = 7.pt },
                MinSize = new() { x = 7.pt }
            },

            sColors = bgColor.In(),

            UseGradient = true,
            GradientDirection = ImageView.GradientDirection.Vertical,
            Sprite = BeatSaberResources.Sprites.background,
            PixelsPerUnit = 14f,
            
            sSkew = _skew,
            
            Do = x => x.WithPointerEvents(
                onEnter: _ => _graphicState.IsHovered = true,
                onLeave: _ => _graphicState.IsHovered = false,
                onDown: _ => {
                    if (_graphicState.IsInteractable) {
                        GameResources.ButtonClickSignal.Raise();
                        OnClick?.Invoke();
                    }
                }),

            Children = {
                new Image {
                    Name = "Border",
                    
                    PixelsPerUnit = 14f,
                    Sprite = BeatSaberResources.Sprites.frame,
                    
                    sColor = borderColor.In()
                }.WithRectExpand(),
            }
        }.Bind(ref _driver).Use();
    }

    #endregion
}