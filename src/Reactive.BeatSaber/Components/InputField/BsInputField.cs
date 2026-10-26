using System;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

public record struct BsInputFieldColors(
    ColorSet ContentColors,
    ColorSet PlaceholderColors,
    ColorSet IconColors,
    ColorSet UnderlineColors
);

[PublicAPI]
public partial class BsInputField : ReactiveComponent, IGraphic, IInteractableComponent {
    #region Public API

    [RawState, Required]
    public BsInputFieldContext Context {
        get;
        set {
            _contextSubscription?.RemoveCallback();

            field = value;
            _contextSubscription = field.AddCallback(HandleContextUpdated);
            
            HandleContextUpdated(field);
        }
    }

    public string Placeholder {
        get => _placeholder.Value;
        set => _placeholder.Value = value;
    }
    
    public Sprite? Icon {
        get => _icon.Value;
        set => _icon.Value = value;
    }

    public bool Interactable {
        get => _graphicState.IsInteractable;
        set {
            _graphicState.IsInteractable = value;
            _button.Interactable = value;
        }
    }

    public BsInputFieldColors Colors {
        get => _colors.Value;
        set => _colors.Value = value;
    }

    private StateSubscription? _contextSubscription;

    private void HandleContextUpdated(BsInputFieldContext context) {
        _focused.Value = context.Focused;
        _text.Value = context.Text;
        _graphicState.IsActive = context.Focused;
    }

    #endregion

    #region Construct

    private State<bool> _focused = null!;
    private State<string?> _text = null!;
    private State<string> _placeholder = null!;
    private State<Sprite?> _icon = null!;
    private State<GraphicState> _graphicState = null!;
    private State<BsInputFieldColors> _colors = null!;
    private BsAeroButton _button = null!;
    private Label _label = null!;
    private Image _caret = null!;

    protected override GameObject Construct() {
        _focused = Remember(false);
        _text = Remember<string?>(null);
        _placeholder = Remember("Search");
        _icon = Remember<Sprite?>(GameResources.SearchIcon);
        _graphicState = Remember(GraphicState.None);
        _colors = Remember(BeatSaberStyle.BsInputFieldColors);

        var closeButtonEnabled = RememberDerived(
            x => x._graphicState.Value.IsInteractable && !x._focused.Value && x._text.Value != null,
            (_focused, _text, _graphicState)
        );
        
        var contentColor = _graphicState.MapColorSet(_colors, x => x.ContentColors);
        var placeholderColor = _graphicState.MapColorSet(_colors, x => x.PlaceholderColors);

        var go = new BsAeroButton {
            FlexController = {
                Padding = 0.pt
            },

            Skew = 0f,

            OnClick = () => {
                _focused.Value = true;
                Context.Focused = true;
            },
            
            Do = x => x.WithPointerEvents(
                onEnter: _ => _graphicState.IsHovered = true,
                onLeave: _ => _graphicState.IsHovered = false
            ),

            ConstructContent = (_, _) => new Layout {
                Name = "Content",

                FlexController = {
                    JustifyContent = Justify.FlexStart,
                    AlignItems = Align.Stretch,
                    Padding = new() { left = 1.pt, top = 1.5f.pt, bottom = 1.5f.pt },
                    Gap = 1.pt
                },

                FlexItem = {
                    Flex = 1
                },

                Children = {
                    new Image {
                        Name = "Underline",

                        FlexItem = {
                            Position = 0.pt,
                            PositionType = PositionType.Absolute
                        },

                        sEnabled = _focused,
                        sColor = _graphicState.MapColorSet(_colors, x => x.UnderlineColors).In(),

                        Sprite = BeatSaberResources.Sprites.backgroundUnderline,
                        PixelsPerUnit = 12f
                    },

                    new Image {
                        Name = "Icon",
                        PreserveAspect = true,
                        sColor = _graphicState.MapColorSet(_colors, x => x.IconColors).In(),
                        sSprite = _icon.Where(x => x != null),
                        sEnabled = _icon.Map(x => x != null)
                    }.AsFlexItem(),

                    new Layout {
                        Name = "Rail",

                        FlexItem = {
                            Flex = 1
                        },

                        FlexController = {
                            Direction = Direction.RightToLeft,
                            AlignItems = Align.Stretch
                        },

                        Children = {
                            new Label {
                                Name = "Text",

                                FlexItem = {
                                    Flex = 1,
                                    Margin = new() { left = 0.5f.pt }
                                },

                                Alignment = TextAlignmentOptions.CaplineLeft,
                                FontStyle = FontStyles.Italic,

                                sText = RememberDerived(
                                    x => x._text.Value ?? x._placeholder.Value,
                                    (_text, _placeholder)
                                ),
                                
                                sColor = RememberDerived(
                                    x => x._text.Value == null ? x.placeholderColor.Value : x.contentColor.Value,
                                    (_text, placeholderColor, contentColor)
                                ),
                            }.Bind(ref _label)
                        }
                    }.AsRectMask(),

                    CreateCloseButton(
                        closeButtonEnabled,
                        // The local text follows the context, so a rejected clear leaves the field as it is
                        () => Context.Text = null
                    )
                }
            }
        }.Bind(ref _button).Use();

        // Attached to the label directly, as it's a layout leaf and won't move the caret
        new Image {
            Name = "Caret",

            ContentTransform = {
                anchorMin = new(0f, 0.1f),
                anchorMax = new(0f, 0.9f),
                pivot = new(0f, 0.5f),
                sizeDelta = new(1f, 0f)
            },

            Enabled = false,
            Sprite = GameResources.CaretIcon,
            PreserveAspect = true,
            sColor = contentColor.In()
        }.Bind(ref _caret).Use(_label.ContentTransform);

        return go;
    }

    private static IReactiveComponent CreateCloseButton(
        IState<bool> enabled,
        Action onClick
    ) {
        var hovered = Remember(false);

        return new Background {
            Name = "ClearButton",

            FlexItem = {
                Size = new() { x = 8.pt }
            },

            FlexController = {
                JustifyContent = Justify.Center,
                AlignItems = Align.Center
            },

            sEnabled = enabled.In(),
            Sprite = ReactiveResources.TransparentPixel,

            Children = {
                new Background {
                    FlexItem = {
                        Size = 4.pt
                    },

                    FlexController = {
                        Padding = 0.75f.pt
                    },

                    Sprite = BeatSaberResources.Sprites.background,
                    PixelsPerUnit = 12f,

                    sColor = hovered.Map(x => x ? Color.black with { a = 0.5f } : Color.clear),

                    Children = {
                        new Image {
                            Sprite = GameResources.CloseIcon,
                            PreserveAspect = true
                        }.AsFlexItem()
                    }
                }
            },
        }.WithPointerEvents(
            onEnter: _ => hovered.Value = true,
            onLeave: _ => hovered.Value = false,
            onDown: _ => {
                GameResources.ButtonClickSignal.Raise();
                onClick();
            }
        );
    }

    #endregion

    #region Caret

    private const float CaretBlinkInterval = 0.5f;

    private string? _caretText;
    private float _caretBlinkStartTime;
    private bool _wasFocused;

    protected override void OnUpdate() {
        var focused = _focused.Value;

        if (focused != _wasFocused) {
            _wasFocused = focused;
            _caretText = null;
        }

        if (!focused) {
            if (_caret.Enabled) {
                _caret.Enabled = false;
            }

            return;
        }

        // The label shows the placeholder when there's no text, so the caret stays at the start then
        var text = _text.Value == null ? string.Empty : _label.Text;
        if (text != _caretText) {
            _caretText = text;
            _caretBlinkStartTime = Time.unscaledTime;

            PlaceCaret(text);
        }

        var visible = (Time.unscaledTime - _caretBlinkStartTime) % (CaretBlinkInterval * 2f) < CaretBlinkInterval;
        _caret.Enabled = visible;
    }

    private void PlaceCaret(string text) {
        var width = 0f;

        if (text.Length > 0) {
            var measured = _label.Measure(float.MaxValue, MeasureMode.AtMost, float.MaxValue, MeasureMode.AtMost);
            width = Mathf.Min(measured.x, _label.ContentTransform.rect.width);
        }

        _caret.ContentTransform.anchoredPosition = new(width, 0f);
    }

    #endregion
}