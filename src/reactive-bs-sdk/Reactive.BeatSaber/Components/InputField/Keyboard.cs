using System;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

public record struct BsKeyboardColors(
    BsKeyboardButtonColors KeyColors
);

[PublicAPI]
public class Keyboard : ReactiveComponent {
    #region Public API

    public string? Text {
        get => _text.Value;
        set {
            _text.Value = value;
            OnTextChanged?.Invoke(value);
        }
    }

    public BsKeyboardColors Colors {
        get => _colors.Value;
        set => _colors.Value = value;
    }

    public Action<string?>? OnTextChanged { get; set; }
    public Action? OnOkClicked { get; set; }

    #endregion

    #region Construct

    private static readonly char[][] _alphabetRows = [
        ['q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p'],
        ['a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l'],
        ['z', 'x', 'c', 'v', 'b', 'n', 'm']
    ];

    private State<string?> _text = null!;
    private State<BsKeyboardColors> _colors = null!;

    protected override GameObject Construct() {
        _text = Remember<string?>(null);
        _colors = Remember(BeatSaberStyle.BsKeyboardColors);

        var keyColors = _colors.Map(x => x.KeyColors);
        var uppercase = Remember(false);

        var onClick = (char x) => {
            Text += x;
        };

        return new Layout {
            FlexController = {
                FlexDirection = FlexDirection.Column,
                JustifyContent = Justify.Center,
                AlignItems = Align.Center,
                Gap = 1.pt
            },

            Children = {
                CreateRow(_alphabetRows[0], onClick, uppercase, keyColors),
                CreateRow(_alphabetRows[1], onClick, uppercase, keyColors),

                new Layout {
                    FlexController = {
                        Gap = 1.pt
                    },

                    Children = {
                        new KeyboardButton {
                            Name = "Shift",

                            FlexItem = {
                                Size = new() { x = 14.pt, y = 7.pt }
                            },

                            FlexController = {
                                JustifyContent = Justify.FlexStart,
                                Padding = new() { left = 1.pt, top = 2.pt, bottom = 2.pt }
                            },

                            OnClick = () => {
                                uppercase.Value = !uppercase.Value;
                            },
                            
                            sColors = keyColors,

                            ConstructContent = (color, _) => new Image {
                                FlexItem = {
                                    AspectRatio = 1
                                },

                                Skew = BeatSaberStyle.Skew,

                                sSprite = uppercase.Map(x => x ? GameResources.ArrowUpIcon : GameResources.ArrowOutlineIcon),
                                sColor = color.In(),
                            }
                        },

                        CreateRow(_alphabetRows[2], onClick, uppercase, keyColors),

                        new KeyboardButton {
                            Name = "Backspace",

                            FlexItem = {
                                Size = 7.pt
                            },

                            OnClick = () => {
                                if (Text == null) {
                                    return;
                                }

                                Text = Text.Length == 1 ? null : Text[..^1];
                            },
                            
                            sColors = keyColors,
                            
                            ConstructContent = (color, _) => new Label {
                                Text = "DEL",
                                Alignment = TextAlignmentOptions.Capline,
                                FontStyle = FontStyles.Italic,

                                sColor = color.In(),
                            }.AsFlexItem()
                        }
                    }
                }.AsFlexItem(),

                new Layout {
                    FlexController = {
                        Gap = 1.pt
                    },

                    Children = {
                        new KeyboardButton {
                            FlexItem = {
                                Size = new() { x = 40.pt, y = 7.pt }
                            },

                            OnClick = () => {
                                Text += " ";
                            },

                            ConstructContent = (color, _) => new Label {
                                Text = "SPACE",
                                Alignment = TextAlignmentOptions.Capline,
                                FontStyle = FontStyles.Italic,

                                sColor = color.In(),
                            }.AsFlexItem()
                        },

                        new KeyboardButton {
                            FlexItem = {
                                Size = new() { x = 12.pt, y = 7.pt }
                            },

                            OnClick = () => {
                                OnOkClicked?.Invoke();
                            },

                            ConstructContent = (state, _) => new Label {
                                Text = "OK",
                                Alignment = TextAlignmentOptions.Capline,
                                FontStyle = FontStyles.Italic,

                                sColor = state.In(),
                            }.AsFlexItem()
                        }
                    }
                }.AsFlexItem()
            }
        }.Use();
    }

    [StateGen]
    private static IReactiveComponent CreateRow(char[] row, Action<char> onClick, IState<bool> uppercase, IState<BsKeyboardButtonColors> colors) {
        return new Repeater<char, KeyboardButton> {
            FlexController = {
                FlexDirection = FlexDirection.Row,
                JustifyContent = Justify.Center,
                Gap = 1.pt
            },

            Items = row,

            ConstructCell = ctx => new KeyboardButton {
                OnClick = () => {
                    var c = uppercase.Value ? char.ToUpper(ctx.Item) : ctx.Item;
                    onClick.Invoke(c);
                },

                sColors = colors.In(),
                
                ConstructContent = (color, _) => new Label {
                    Alignment = TextAlignmentOptions.Capline,
                    FontStyle = FontStyles.Italic,

                    sText = RememberDerived(
                        x => {
                            var c = x.uppercase.Value ? char.ToUpper(x.ctx.Item) : x.ctx.Item;
                            return c.ToString();
                        },
                        (uppercase, ctx)
                    ),

                    sColor = color.In(),
                }
            }
        }.AsFlexItem();
    }

    #endregion
}