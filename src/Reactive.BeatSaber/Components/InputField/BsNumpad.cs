using System;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

public record struct BsNumpadColors(
    BsKeyboardButtonColors KeyColors
);

/// <summary>
/// A keyboard for number-only inputs.
/// </summary>
[PublicAPI]
public class BsNumpad : ReactiveComponent {
    #region Public API

    public string? Text {
        get => _text.Value;
        set {
            _text.Value = value;
            OnTextChanged?.Invoke(value);
        }
    }

    public BsNumpadColors Colors {
        get => _colors.Value;
        set => _colors.Value = value;
    }

    public Action<string?>? OnTextChanged { get; set; }
    public Action? OnOkClicked { get; set; }

    #endregion

    #region Construct

    private static readonly char[][] _digitRows = [
        ['7', '8', '9'],
        ['4', '5', '6'],
        ['1', '2', '3']
    ];

    private State<string?> _text = null!;
    private State<BsNumpadColors> _colors = null!;

    protected override GameObject Construct() {
        _text = Remember<string?>(null);
        _colors = Remember(BeatSaberStyle.BsNumpadColors);

        var keyColors = _colors.Map(x => x.KeyColors);

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
                CreateRow(_digitRows[0], onClick, keyColors),
                CreateRow(_digitRows[1], onClick, keyColors),
                CreateRow(_digitRows[2], onClick, keyColors),

                new Layout {
                    FlexController = {
                        Gap = 1.pt
                    },

                    Children = {
                        new BsKeyboardButton {
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
                            }
                        },

                        CreateRow(['0'], onClick, keyColors),

                        new BsKeyboardButton {
                            Name = "Ok",

                            FlexItem = {
                                Size = 7.pt
                            },

                            OnClick = () => {
                                OnOkClicked?.Invoke();
                            },

                            sColors = keyColors,

                            ConstructContent = (color, _) => new Label {
                                Text = "OK",
                                Alignment = TextAlignmentOptions.Capline,
                                FontStyle = FontStyles.Italic,

                                sColor = color.In(),
                            }
                        }
                    }
                }
            }
        }.Use();
    }

    [StateGen]
    private static IReactiveComponent CreateRow(char[] row, Action<char> onClick, IState<BsKeyboardButtonColors> colors) {
        return new Repeater<char, BsKeyboardButton> {
            FlexController = {
                FlexDirection = FlexDirection.Row,
                JustifyContent = Justify.Center,
                Gap = 1.pt
            },

            Items = row,

            ConstructCell = ctx => new BsKeyboardButton {
                OnClick = () => {
                    onClick.Invoke(ctx.Item);
                },

                sColors = colors.In(),

                ConstructContent = (color, _) => new Label {
                    Alignment = TextAlignmentOptions.Capline,
                    FontStyle = FontStyles.Italic,

                    sText = ctx.Map(x => x.Item.ToString()),
                    sColor = color.In(),
                }
            }
        };
    }

    #endregion
}
