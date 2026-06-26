using System;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class Keyboard : ReactiveComponent {
    #region Public API

    public string? Text {
        get => _text.Value;
        set => _text.Value = value;
    }

    public Action<string?>? OnTextChanged { get; set; }

    #endregion

    #region Construct

    private static readonly char[][] _alphabetRows = [
        ['q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p'],
        ['a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l'],
        ['z', 'x', 'c', 'v', 'b', 'n', 'm']
    ];

    private State<string?> _text = null!;

    protected override GameObject Construct() {
        _text = Remember<string?>(null);
        var uppercase = Remember(false);

        var onClick = (char x) => {
            _text.Value += x;
            OnTextChanged?.Invoke(_text.Value);
        };

        return new Layout {
            FlexController = {
                FlexDirection = FlexDirection.Column,
                JustifyContent = Justify.Center,
                AlignItems = Align.Center,
                Gap = 1.pt
            },

            Children = {
                CreateRow(_alphabetRows[0], onClick, uppercase),
                CreateRow(_alphabetRows[1], onClick, uppercase),

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

                            ConstructContent = (state, _) => new Image {
                                FlexItem = {
                                    AspectRatio = 1
                                },

                                Skew = BeatSaberStyle.Skew,

                                sSprite = uppercase.Map(x => x ? GameResources.ArrowUpIcon : GameResources.ArrowOutlineIcon),
                                sColor = state.MapColorSet(BeatSaberStyle.BsKeyboard.KeyContentColors).In(),
                            }
                        },

                        CreateRow(_alphabetRows[2], onClick, uppercase),

                        new KeyboardButton {
                            Name = "Backspace",

                            FlexItem = {
                                Size = 7.pt
                            },

                            OnClick = () => {
                                _text.Value = _text.Value?[..^1];
                            },

                            ConstructContent = (state, _) => new Label {
                                Text = "DEL",
                                Alignment = TextAlignmentOptions.Capline,
                                FontStyle = FontStyles.Italic,

                                sColor = state.MapColorSet(BeatSaberStyle.BsKeyboard.KeyContentColors).In(),
                            }.AsFlexItem()
                        }
                    }
                }.AsFlexItem(),
            }
        }.Use();
    }

    [StateGen]
    private static IReactiveComponent CreateRow(char[] row, Action<char> onClick, IState<bool> uppercase) {
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

                ConstructContent = (state, _) => new Label {
                    Alignment = TextAlignmentOptions.Capline,
                    FontStyle = FontStyles.Italic,

                    sText = RememberDerived(
                        x => {
                            var c = x.uppercase.Value ? char.ToUpper(x.ctx.Item) : x.ctx.Item;
                            return c.ToString();
                        },
                        (uppercase, ctx)
                    ),

                    sColor = state.MapColorSet(BeatSaberStyle.BsKeyboard.KeyContentColors).In(),
                }
            }
        }.AsFlexItem();
    }

    #endregion
}