using System;
using System.Globalization;
using HMUI;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

public record struct BsSliderColors(
    ColorSet BackgroundColors,
    ColorSet HandleColors,
    ColorSet TextColors,
    CompositeColorSet LeftButtonColors,
    CompositeColorSet RightButtonColors
);

[PublicAPI]
public class BsSlider : ReactiveComponent, IInteractableComponent {
    #region Public API

    public bool Interactable {
        get => _graphicState.IsInteractable;
        set => _graphicState.IsInteractable = value;
    }

    public BsSliderColors Colors {
        get => _colors.Value;
        set => _colors.Value = value;
    }

    public Func<float, string>? ValueFormatter {
        get => _valueFormatter.Value;
        set => _valueFormatter.Value = value;
    }

    public bool ShowValueText {
        get => _text.Enabled;
        set => _text.Enabled = value;
    }

    public bool ShowButtons {
        get => _buttonsEnabled.Value;
        set => _buttonsEnabled.Value = value;
    }

    public float MinValue {
        get => _minValue.Value;
        set {
            _minValue.Value = value;
            UpdateValue(_value);
        }
    }

    public float MaxValue {
        get => _maxValue.Value;
        set {
            _maxValue.Value = value;
            UpdateValue(_value);
        }
    }

    public float ValueStep {
        get;
        set {
            field = value;
            UpdateValue(_value);
        }
    }

    public float Value {
        get => _value.Value;
        set => UpdateValue(value);
    }

    private void UpdateValue(float value) {
        var clamped = Mathf.Clamp(value, MinValue, MaxValue);
        var rounded = MathUtils.RoundStepped(clamped, ValueStep, MinValue);

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (_value.Value == rounded) {
            return;
        }

        _value.Value = rounded;
        PlaceHandleWithText();
    }

    #endregion

    #region Math

    protected float MaxHandlePosition => _slidingAreaTransform.rect.width - _handleTransform.rect.width;

    private void PlaceHandleWithText() {
        PlaceHandle();
        PlaceText();
    }

    private void PlaceText() {
        if (!_text.Enabled) {
            return;
        }

        var handlePos = _handleTransform.localPosition.x;
        var halfPassed = handlePos > MaxHandlePosition / 2f;

        var measuredSize = _text.Measure(int.MaxValue, MeasureMode.AtMost, int.MaxValue, MeasureMode.AtMost);
        var textSize = measuredSize.x / 2f + 1f;
        var textPos = halfPassed ? handlePos - textSize : handlePos + textSize + _handleTransform.rect.width;

        var text = _text.ContentTransform;
        text.localPosition = new(textPos, 0f, 0f);
        text.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, measuredSize.x);
        text.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, measuredSize.y);
    }

    private void PlaceHandle() {
        var x = MathUtils.Map(_value.Value, MinValue, MaxValue, 0, MaxHandlePosition);

        _handleTransform.localPosition = new(x, 0f);
    }

    #endregion

    #region DimensionsListener

    // Yeah, there's no cheap way to track that yet :(
    class RectDimensionsListener : MonoBehaviour {
        public BsSlider Slider = null!;

        private void OnRectTransformDimensionsChange() {
            Slider.PlaceHandleWithText();
        }
    }

    protected override void OnInitialize() {
        _slidingAreaTransform.gameObject.AddComponent<RectDimensionsListener>().Slider = this;
    }

    #endregion

    #region Construct

    private CurvedCanvasSettings? _curvedCanvasSettings;
    private RectTransform _slidingAreaTransform = null!;
    private RectTransform _handleTransform = null!;
    private RectTransform _textTransform = null!;
    private Label _text = null!;

    private State<BsSliderColors> _colors = null!;
    private State<GraphicState> _graphicState = null!;
    private State<Func<float, string>?> _valueFormatter = null!;
    private State<float> _value = null!;
    private State<float> _maxValue = null!;
    private State<float> _minValue = null!;
    private State<bool> _buttonsEnabled = null!;

    protected override GameObject Construct() {
        _colors = Remember(BeatSaberStyle.BsSliderColors);
        _graphicState = Remember(GraphicState.None);
        _valueFormatter = Remember<Func<float, string>?>(null);
        _value = Remember(0f);
        _minValue = Remember(0f);
        _maxValue = Remember(0f);
        _buttonsEnabled = Remember(true);

        return new Layout {
            FlexItem = {
                Size = new() { x = 40.pt, y = 6.pt }
            },

            FlexController = {
                Gap = 0.5f
            },

            Children = {
                CreateButton(
                    _colors,
                    RememberDerived(
                        static x => x._value.Value > x._minValue.Value,
                        (_value, _minValue)
                    ),
                    _buttonsEnabled,
                    leftButton: true,
                    name: "DecButton",
                    callback: () => Value -= ValueStep
                ),

                new Background {
                    FlexItem = {
                        FlexGrow = 1f
                    },

                    FlexController = {
                        Padding = 1.3f.pt
                    },

                    Do = x => x.WithPointerEvents(
                        onDown: _ => _curvedCanvasSettings = Content.GetComponentInParent<CurvedCanvasSettings>(),
                        onUp: _ => _curvedCanvasSettings = null,
                        onUpdate: evt => {
                            _graphicState.IsHovered = evt.IsHovered;
                            _graphicState.IsActive = evt.IsPressed;

                            if (!evt.IsPressed || evt.EventData?.position == Vector2.zero) {
                                return;
                            }

                            var point = evt.EventData!.TranslateToLocalPoint(_slidingAreaTransform, Canvas!, _curvedCanvasSettings);
                            var maxHandlePosition = MaxHandlePosition;

                            var clamped = Mathf.Clamp(point.x, 0f, maxHandlePosition);
                            var mapped = MathUtils.Map(clamped, 0f, maxHandlePosition, MinValue, MaxValue);

                            UpdateValue(mapped);
                        }
                    ),

                    sSprite = _buttonsEnabled.Map(x => x ?
                        BeatSaberResources.Sprites.rectangle :
                        BeatSaberResources.Sprites.background
                    ),

                    sColor = _graphicState.MapColorSet(_colors, static x => x.BackgroundColors).In(),

                    PixelsPerUnit = 12f,
                    Material = GameResources.UINoGlowMaterial,

                    Children = {
                        // Sliding area
                        new Layout {
                            FlexItem = {
                                FlexGrow = 1f
                            },

                            ContentTransform = {
                                pivot = new(0f, 0.5f)
                            },

                            Name = "SlidingArea",

                            Children = {
                                // Text
                                new Label {
                                    ContentTransform = {
                                        anchorMin = new(0.5f, 0f),
                                        anchorMax = new(0.5f, 1f),
                                        sizeDelta = Vector2.zero
                                    },

                                    Name = "ValueText",
                                    Alignment = TextAlignmentOptions.Capline,

                                    sColor = _graphicState.MapColorSet(_colors, static x => x.TextColors).In(),

                                    sText = RememberDerived(
                                        x => {
                                            if (x._valueFormatter.Value is { } formatter) {
                                                return formatter(_value);
                                            }

                                            return _value.Value.ToString(CultureInfo.CurrentCulture);
                                        },
                                        (_value, _valueFormatter)
                                    )
                                }.Bind(ref _textTransform).Bind(ref _text),

                                // Handle
                                new Image {
                                    ContentTransform = {
                                        anchorMin = new(0.5f, 0f),
                                        anchorMax = new(0.5f, 1f),
                                        sizeDelta = new(1f, 0f),
                                        pivot = new(0f, 0.5f)
                                    },

                                    Name = "Handle",

                                    sColor = _graphicState.MapColorSet(_colors, static x => x.HandleColors).In(),

                                    Sprite = BeatSaberResources.Sprites.background,
                                    PixelsPerUnit = 30f
                                }.Bind(ref _handleTransform)
                            }
                        }.Bind(ref _slidingAreaTransform)
                    }
                },

                CreateButton(
                    _colors,
                    RememberDerived(
                        static x => x._value.Value < x._maxValue.Value,
                        (_value, _maxValue)
                    ),
                    _buttonsEnabled,
                    leftButton: false,
                    name: "IncButton",
                    callback: () => Value += ValueStep
                ),
            }
        }.Use();
    }

    private static Background CreateButton(
        IState<BsSliderColors> colors,
        IState<bool> interactable,
        IState<bool> enabled,
        bool leftButton,
        string name,
        Action callback
    ) {
        var graphic = Remember(GraphicState.None);

        interactable.AddCallback(x => graphic.IsInteractable = x);

        return new Background {
            FlexItem = {
                FlexBasis = 6.pt
            },

            FlexController = {
                Padding = 1.5f.pt
            },

            Name = name,
            sEnabled = enabled.In(),

            Sprite = leftButton ?
                BeatSaberResources.Sprites.backgroundLeft :
                BeatSaberResources.Sprites.backgroundRight,

            PixelsPerUnit = 12f,
            GradientDirection = ImageView.GradientDirection.Horizontal,
            Material = GameResources.UINoGlowMaterial,
            UseGradient = true,

            sColors = graphic.MapColorSet(colors, leftButton ?
                static x => x.LeftButtonColors :
                static x => x.RightButtonColors
            ).In(),

            Do = x => x.WithPointerEvents(
                onEnter: _ => graphic.IsHovered = true,
                onLeave: _ => graphic.IsHovered = false,
                onDown: _ => {
                    if (graphic.IsInteractable) {
                        callback();
                        GameResources.ButtonClickSignal.Raise();
                    }
                }),

            Children = {
                // Icon
                new Image {
                    FlexItem = {
                        FlexGrow = 1f
                    },

                    ContentTransform = {
                        localEulerAngles = new(0f, 0f, leftButton ? 270f : 90f)
                    },

                    Sprite = GameResources.ArrowIcon,
                    PreserveAspect = true,
                    Color = Color.white.ColorWithAlpha(0.8f),
                }
            }
        };
    }

    #endregion
}