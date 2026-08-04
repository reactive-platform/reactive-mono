using System;
using JetBrains.Annotations;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class BsToggle : ReactiveComponent, IInteractableComponent {
    #region Props

    public bool Toggled {
        get => _toggled.Value;
        set {
            if (value == _toggled.Value) {
                return;
            }

            _toggled.Value = value;
            OnToggleStateChanged?.Invoke(value);
        }
    }

    public bool Interactable {
        get => _interactable.Value;
        set => _interactable.Value = value;
    }

    public Action<bool>? OnToggleStateChanged { get; set; }

    #endregion

    #region Construct

    private float _knobMargin = 1f;
    private float _knobWidth = 7.55f;
    private float _knobHeight = 5f;
    private float _horizontalStretchAmount = 0.8f;
    private float _verticalStretchAmount = 0.8f;

    private AnimatedState<float> _progressValue = null!;
    private State<bool> _interactable = null!;
    private State<bool> _toggled = null!;

    protected override GameObject Construct() {
        _progressValue = RememberAnimated(0f, 10.fact);
        _interactable = Remember(true);
        _toggled = Remember(false);

        var hovered = Remember(false);

        var bgColor = hovered.Map(x => x ?
            BeatSaberStyle.BsToggle.BackgroundColors.HoveredColor :
            BeatSaberStyle.BsToggle.BackgroundColors.Color
        );

        var knobColor = _toggled.Map(x => x ?
            BeatSaberStyle.BsToggle.KnobColors.ActiveColor :
            BeatSaberStyle.BsToggle.KnobColors.Color
        );

        _toggled.AddCallback(x => _progressValue.TargetValue = x ? 1 : 0);

        return new Background {
            FlexItem = {
                Size = new() { x = 18.pt, y = 6.pt }
            },

            Sprite = BeatSaberResources.Sprites.background,
            Material = GameResources.UINoGlowMaterial,
            PixelsPerUnit = 12f,

            sColor = bgColor,

            Do = x => x.WithPointerEvents(
                onDown: _ => Toggled = !Toggled,
                onEnter: _ => hovered.Value = true,
                onLeave: _ => hovered.Value = false
            ),

            Children = {
                // Text area
                new Layout {
                    Children = {
                        new Label {
                            ContentTransform = {
                                anchorMin = new(0f, 0f),
                                anchorMax = new(0.5f, 1f),
                            },

                            Text = "I",
                            Alignment = TextAlignmentOptions.Capline,

                            sColor = _progressValue.Map(x => Color.Lerp(Color.clear, BeatSaberStyle.BsToggle.TextColors.ActiveColor, x))
                        },

                        new Label {
                            ContentTransform = {
                                anchorMin = new(0.5f, 0f),
                                anchorMax = new(1f, 1f),
                            },

                            Text = "O",
                            Alignment = TextAlignmentOptions.Capline,

                            sColor = _progressValue.Map(x => Color.Lerp(Color.clear, BeatSaberStyle.BsToggle.TextColors.Color, 1 - x))
                        }
                    }
                }.WithRectExpand(),

                // Knob slide area
                new Layout {
                    Children = {
                        // Knob
                        new Image {
                            ContentTransform = {
                                sanchorMin = _progressValue.Map(x => new Vector2(x, 0)),
                                sanchorMax = _progressValue.Map(x => new Vector2(x, 1f)),

                                ssizeDelta = _progressValue.Map(t => {
                                    var factor = 1f - Mathf.Abs(t - 0.5f) * 2f;

                                    var x = _knobWidth * (1f + _horizontalStretchAmount * factor);
                                    var y = _knobHeight * (_verticalStretchAmount * -factor) - _knobMargin;

                                    return new Vector2(x, y);
                                })
                            },

                            Sprite = BeatSaberResources.Sprites.background,
                            PixelsPerUnit = 12f,

                            sColor = knobColor
                        }
                    }
                }.WithRectExpand().WithSizeDelta(-_knobWidth - _knobMargin, 0f)
            }
        }.AsFlexGroup().Use();
    }

    #endregion
}