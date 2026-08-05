using System;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public partial class InputField : ReactiveComponent, IGraphic {
    #region Public API

    [RawState, Required]
    public InputFieldContext Context {
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

    private StateSubscription? _contextSubscription;

    private void HandleContextUpdated(InputFieldContext context) {
        _focused.Value = context.Focused;
        _text.Value = context.Text;
    }

    #endregion

    #region Construct

    private State<bool> _focused = null!;
    private State<string?> _text = null!;
    private State<string> _placeholder = null!;

    protected override GameObject Construct() {
        _focused = Remember(false);
        _text = Remember<string?>(null);
        _placeholder = Remember("Search");

        var closeButtonEnabled = RememberDerived(
            x => !x._focused.Value && _text.Value != null,
            (_focused, _text)
        );

        return new BsAeroButton {
            FlexController = {
                Padding = 0.pt
            },

            Skew = 0f,

            OnClick = () => {
                _focused.Value = true;
                Context.Focused = true;
            },

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

                        Sprite = BeatSaberResources.Sprites.backgroundUnderline,
                        Color = BeatSaberStyle.BsInputField.PlaceholderColors.Color,
                        PixelsPerUnit = 12f
                    },

                    new Image {
                        Name = "Icon",
                        PreserveAspect = true,
                        Color = BeatSaberStyle.BsInputField.ContentColors.HoveredColor.GetValueOrDefault(),
                        Sprite = GameResources.SearchIcon,
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

                                Color = BeatSaberStyle.BsAeroButtonColors.ContentColors.Color,
                                Alignment = TextAlignmentOptions.CaplineLeft,
                                FontStyle = FontStyles.Italic,

                                sText = RememberDerived(
                                    x => x._text.Value ?? x._placeholder.Value,
                                    (_text, _placeholder)
                                ),

                                // When there's text, placeholder becomes a solid label, so the color set changes
                                sColor = _text.Map(
                                    x => x == null ?
                                        BeatSaberStyle.BsInputField.PlaceholderColors.Color :
                                        BeatSaberStyle.BsInputField.ContentColors.Color
                                ),
                            }
                        }
                    }.AsRectMask(),

                    CreateCloseButton(
                        closeButtonEnabled,
                        () => {
                            _text.Value = null;
                            Context.Text = null;
                        }
                    )
                }
            }
        }.Use();
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
}