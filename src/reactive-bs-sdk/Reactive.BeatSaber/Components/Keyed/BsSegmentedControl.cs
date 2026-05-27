using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using BsImage = Reactive.BeatSaber.Components.Image;

namespace Reactive.BeatSaber.Components;

public record struct BsSegmentedControlItem(string? Text, Sprite? Icon);

[PublicAPI]
public partial class BsSegmentedControl<T> : ReactiveComponent, ISkewedComponent {
    #region PublicAPI

    [Required]
    public IReadOnlyDictionary<T, BsSegmentedControlItem> Items {
        get => _items!;
        set {
            _items = value;
            _repeater.Items = value.Keys.ToArray();

            DoInitialUpdate();
        }
    }

    [Required]
    public T Key {
        get => _key!;
        set {
            if (_initialized) {
                SetKey(value, true);
            } else {
                DoInitialUpdate();
            }
        }
    }

    public float Skew {
        get => _skew.Value;
        set => _skew.Value = value;
    }

    public Action<T>? OnKeyChanged { get; set; }

    private bool _initialized;
    private IReadOnlyDictionary<T, BsSegmentedControlItem>? _items;
    private T? _key;

    private void DoInitialUpdate() {
        if (!_initialized && _key != null && _items != null) {
            SetKey(_key, true);
            _initialized = true;
        }
    }

    private void SetKey(T value, bool updateListView) {
        _key = value;

        if (updateListView) {
            _repeater.SelectedItems = [value];
        }

        OnKeyChanged?.Invoke(value);
    }

    #endregion

    #region Construct

    private Repeater<T, Background> _repeater = null!;
    private State<float> _skew = null!;

    protected override GameObject Construct() {
        _skew = Remember(BeatSaberStyle.Skew);

        return new Repeater<T, Background> {
            FlexController = {
                FlexDirection = FlexDirection.Row,
                FlexWrap = Wrap.NoWrap,
                Gap = 1.pt
            },

            FlexItem = {
                Size = new() { y = 8.pt },
                FlexBasis = YogaValue.Auto
            },

            Items = Array.Empty<T>(),
            SelectionMode = SelectionMode.Single,

            OnSelectedItemsChanged = x => SetKey(x.First(), false),

            ConstructCell = ctx => {
                var item = ctx.Map(x => _items?[x.Item]);
                var hovered = Remember(false);

                var bgColor = RememberDerived(
                    x => {
                        var state = GraphicState.None
                            .AddIf(GraphicState.Hovered, x.hovered)
                            .AddIf(GraphicState.Active, x.ctx.Selected);

                        return BeatSaberStyle.CellColors.GetColor(state);
                    },
                    (ctx, hovered)
                );

                var fgColor = RememberDerived(
                    x => {
                        var state = GraphicState.None
                            .AddIf(GraphicState.Hovered, x.hovered)
                            .AddIf(GraphicState.Active, x.ctx.Selected);

                        return BeatSaberStyle.CellTextColors.GetColor(state);
                    },
                    (ctx, hovered)
                );

                return new Background {
                    FlexController = {
                        JustifyContent = Justify.Center,
                        Padding = new() { left = 2.pt, right = 2.pt },
                        Gap = 0.5f.pt
                    },

                    FlexItem = {
                        FlexBasis = YogaValue.FitContent,
                        Flex = 1f
                    },

                    sColor = bgColor,
                    sSkew = _skew,

                    PixelsPerUnit = 10f,

                    sSprite = ctx.Map(x => {
                        var i = x.Value.Index;
                        var s = x.Value.TotalCells;

                        if (i == 0 && s == 1) {
                            return BeatSaberResources.Sprites.background;
                        }
                        if (i == 0) {
                            return BeatSaberResources.Sprites.backgroundLeft;
                        }
                        if (i == s - 1) {
                            return BeatSaberResources.Sprites.backgroundRight;
                        }

                        return BeatSaberResources.Sprites.rectangle;
                    }),

                    Do = x => x.WithPointerEvents(
                        onEnter: _ => hovered.Value = true,
                        onLeave: _ => hovered.Value = false,
                        onDown: _ => ctx.Selected = true
                    ),

                    Children = {
                        new Label {
                            Alignment = TextAlignmentOptions.Capline,
                            sFontStyle = _skew.Map(x => x > 0 ? FontStyles.Italic : FontStyles.Normal),
                            
                            sColor = fgColor,

                            sText = item.Map(x => x?.Text).Where(x => x != null),
                            sEnabled = item.Map(x => x?.Text != null)
                        }.AsFlexItem(),

                        new BsImage {
                            FlexItem = {
                                Margin = new() { top = 2.pt, bottom = 2.pt },
                                AspectRatio = 1f
                            },

                            PreserveAspect = true,
                            sColor = fgColor,
                            sSkew = _skew,

                            sSprite = item.Map(x => x?.Icon).Where(x => x != null),
                            sEnabled = item.Map(x => x?.Icon != null)
                        }.AsFlexItem()
                    }
                };
            }
        }.Bind(ref _repeater).Use();
    }

    #endregion
}