using System;
using System.Collections.Generic;
using HMUI;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public partial class KeyboardButton : ReactiveComponent, ILayoutDriver {
    public delegate ILayoutItem ConstructContentDelegate(IState<GraphicState> graphic, IState<float> skew);

    #region Public API

    public float Skew {
        get => _skew.Value;
        set => _skew.Value = value;
    }

    public bool Interactable {
        get => _graphicState.IsInteractable;
        set => _graphicState.IsInteractable = value;
    }

    [Required]
    public ConstructContentDelegate ConstructContent {
        init {
            var layoutItem = value(_graphicState, _skew);
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

    private ILayoutDriver _driver = null!;

    protected override GameObject Construct() {
        _skew = Remember(0f);
        _graphicState = Remember(GraphicState.None);

        var bgColor = _graphicState.MapColorSet(BeatSaberStyle.BsKeyboard.KeyBackgroundColors);
        var borderColor = _graphicState.MapColorSet(BeatSaberStyle.BsKeyboard.KeyBorderColors);
        
        return new Background {
            FlexController = {
                JustifyContent = Justify.SpaceAround,
                Padding = new() { left = 1.pt, right = 1.pt }
            },

            FlexItem = {
                Size = new() { y = 7.pt },
                MinSize = new() { x = 7.pt }
            },

            sColor = bgColor.Map(x => x.Color),
            sGradientColor0 = bgColor.Map(x => x.GradientColor0),
            sGradientColor1 = bgColor.Map(x => x.GradientColor1),

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