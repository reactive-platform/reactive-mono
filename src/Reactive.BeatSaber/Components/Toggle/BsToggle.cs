using System;
using JetBrains.Annotations;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

public record struct BsToggleColors(
    ColorSet BackgroundColors,
    ColorSet KnobColors,
    ColorSet TextColors
);

[PublicAPI]
public class BsToggle : ReactiveComponent, IInteractableComponent {
    #region Props

    public bool Toggled {
        get => _graphic.IsActive;
        set {
            if (value == _graphic.IsActive) {
                return;
            }

            _graphic.IsActive = value;
            OnToggleStateChanged?.Invoke(value);
        }
    }

    public bool Interactable {
        get => _graphic.IsInteractable;
        set => _graphic.IsInteractable = value;
    }

    public BsToggleColors Colors {
        get => _colors.Value;
        set => _colors.Value = value;
    } 

    public Action<bool>? OnToggleStateChanged { get; set; }

    #endregion

    #region Construct

    private float _knobMargin = 1f;
    private float _knobWidth = 7.55f;
    private float _knobHeight = 5f;
    private float _horizontalStretchAmount = 0.8f;
    private float _verticalStretchAmount = 0.8f;

    private AnimatedState<float> _progress = null!;
    private State<GraphicState> _graphic = null!;
    private State<BsToggleColors> _colors = null!;

    protected override GameObject Construct() {
        _progress = RememberAnimated(0f, 10.fact);
        _graphic = Remember(GraphicState.None);
        _colors = Remember(BeatSaberStyle.BsToggleColors);

        var bgColor = RememberDerived(x => _colors.Value.BackgroundColors.GetColor(x._graphic), (_graphic, _colors));
        var knobColor = RememberDerived(x => _colors.Value.KnobColors.GetColor(x._graphic), (_graphic, _colors));

        _graphic.AddCallback(x => {
            _progress.TargetValue = x.IsActive ? 1 : 0;
        });

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
                onEnter: _ => _graphic.IsHovered = true,
                onLeave: _ => _graphic.IsHovered = false
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

                            sColor = RememberDerived(
                                x => Color.Lerp(Color.clear, x._colors.Value.TextColors.GetColor(_graphic), x._progress),
                                (_graphic, _colors, _progress)
                            )
                        },

                        new Label {
                            ContentTransform = {
                                anchorMin = new(0.5f, 0f),
                                anchorMax = new(1f, 1f),
                            },

                            Text = "O",
                            Alignment = TextAlignmentOptions.Capline,

                            sColor = RememberDerived(
                                x => Color.Lerp(Color.clear, x._colors.Value.TextColors.GetColor(_graphic), 1 - x._progress),
                                (_graphic, _colors, _progress)
                            )
                        }
                    }
                }.WithRectExpand(),

                // Knob slide area
                new Layout {
                    Children = {
                        // Knob
                        new Image {
                            ContentTransform = {
                                sanchorMin = _progress.Map(x => new Vector2(x, 0)),
                                sanchorMax = _progress.Map(x => new Vector2(x, 1f)),

                                ssizeDelta = _progress.Map(t => {
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