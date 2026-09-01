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

namespace Reactive.BeatSaber.Components;

public record struct BsDropdownItem(string? Text, Sprite? Icon);

public record struct BsDropdownColors(
    ColorSet ButtonBackgroundColors,
    ColorSet ItemContentColors,
    ColorSet ItemBackgroundColors
);

[PublicAPI]
public partial class BsDropdown<T> : ReactiveComponent, ISkewedComponent {
    #region Public API

    [Required]
    public IReadOnlyDictionary<T, BsDropdownItem> Items {
        get => _items!;
        set {
            _items = value;
            _table.Items = value.Keys.ToArray();

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
                _key = value;
                DoInitialUpdate();
            }
        }
    }

    public float Skew {
        get => _skew.Value;
        set => _skew.Value = value;
    }

    public BsDropdownColors Colors {
        get => _colors.Value;
        set => _colors.Value = value;
    }

    public Action<T>? OnKeyChanged { get; set; }

    private bool _initialized;
    private IReadOnlyDictionary<T, BsDropdownItem>? _items;
    private T? _key;

    private void DoInitialUpdate() {
        if (!_initialized && _key != null && _items != null) {
            SetKey(_key, true);
            _initialized = true;
        }
    }

    private void SetKey(T value, bool updateTable) {
        _key = value;
        _keyState.Value = value;

        if (updateTable) {
            _table.SelectedItems = [value];
        }

        OnKeyChanged?.Invoke(value);
    }

    #endregion

    #region Construct

    private Table<T, IReactiveComponent> _table = null!;
    private State<float> _skew = null!;
    private State<T?> _keyState = null!;
    private State<BsDropdownColors> _colors = null!;

    protected override GameObject Construct() {
        _keyState = Remember<T?>(default);
        _skew = Remember(BeatSaberStyle.Skew);
        _colors = Remember(BeatSaberStyle.BsDropdownColors);

        var modalOpened = Remember(false);
        var graphic = Remember(GraphicState.None);
        var anchor = Remember<RectTransform?>(null);

        var contentColor = graphic.MapColorSet(_colors, x => x.ItemContentColors);
        var bgColor = graphic.MapColorSet(_colors, x => x.ButtonBackgroundColors);

        var item = _keyState.Map(x => x != null ? _items?[x] : null);
        var scrollContext = new ScrollContext();

        return new Background {
            FlexController = {
                Padding = new() { left = 2.pt, right = 2.pt },
                Gap = 1.pt,
                JustifyContent = Justify.Center
            },

            FlexItem = {
                Size = new() { y = 7.pt, x = 36.pt }
            },

            Sprite = BeatSaberResources.Sprites.background,
            Material = GameResources.UINoGlowMaterial,
            PixelsPerUnit = 12f,

            sSkew = _skew,
            sColor = bgColor.In(),

            Do = x => {
                x.WithPointerEvents(
                    onEnter: _ => graphic.IsHovered = true,
                    onLeave: _ => graphic.IsHovered = false,
                    onDown: _ => {
                        modalOpened.Value = true;
                        GameResources.ButtonClickSignal.Raise();
                    }
                );

                anchor.Value = x.ContentTransform;
            },

            Children = {
                new Label {
                    Alignment = TextAlignmentOptions.Capline,
                    sFontStyle = _skew.Map(x => x > 0 ? FontStyles.Italic : FontStyles.Normal),

                    sEnabled = item.Map(x => x?.Text != null),
                    sText = item.Map(x => x?.Text),
                    sColor = contentColor.In(),
                }.AsFlexItem(),

                new Image {
                    FlexItem = {
                        Margin = new() { top = 2.pt, bottom = 2.pt },
                        AspectRatio = 1f
                    },

                    PreserveAspect = true,

                    sEnabled = item.Map(x => x?.Icon != null),
                    sSprite = item.Map(x => x?.Icon),
                    sColor = contentColor.In()
                },

                new Modal {
                    sIsPushed = modalOpened,
                    sPlacementAnchor = anchor,

                    OnClickOutside = () => {
                        modalOpened.Value = false;
                    },

                    PlacementData = new() {
                        Placement = RelativePlacement.Center,
                        Clip = true
                    },

                    FlexController = {
                        FlexDirection = FlexDirection.Row,
                        Gap = 0.5f.pt
                    },

                    Children = {
                        new Background {
                            FlexItem = {
                                Flex = 1
                            },

                            FlexController = {
                                Padding = new() { top = 1.pt, bottom = 1.pt }
                            },

                            Children = {
                                new Table<T, IReactiveComponent> {
                                    FlexItem = {
                                        Size = new() { x = 36.pt, y = (7 * 5).pt },
                                        Margin = new() { top = 1.pt, bottom = 1.pt }
                                    },

                                    ScrollContext = scrollContext,
                                    Items = Array.Empty<T>(),

                                    OnSelectedItemsChanged = x => {
                                        if (x.Count > 0) {
                                            SetKey(x.First(), false);
                                        }
                                    },

                                    ConstructCell = ctx => CreateCell(ctx, _colors)
                                }.Bind(ref _table),
                            }
                        }.AsBlurBackground(),

                        new Scrollbar {
                            ScrollContext = scrollContext,
                            HideIfNothingToScroll = true
                        }
                    }
                }
            }
        }.Use();
    }

    [StateGen]
    private IReactiveComponent CreateCell(CellContext<T> context, IState<BsDropdownColors> colors) {
        var graphic = Remember(GraphicState.None);
        var item = context.Map(x => Items[x.Item]);

        var fgColor = graphic.MapColorSet(colors, x => x.ItemContentColors);
        var bgColor = graphic.MapColorSet(colors, x => x.ItemBackgroundColors);

        context.AddCallback(x => graphic.IsActive = x.Selected);

        return new Background {
            FlexController = {
                Gap = 1.pt,
                JustifyContent = Justify.Center
            },

            FlexItem = {
                Size = new() { x = 36.pt, y = 7.pt }
            },

            Do = x => x.WithPointerEvents(
                onEnter: _ => graphic.IsHovered = true,
                onLeave: _ => graphic.IsHovered = false,
                onDown: _ => {
                    context.Selected = true;
                    GameResources.ButtonClickSignal.Raise();
                }
            ),

            Sprite = BeatSaberResources.Sprites.rectangle,
            sColor = bgColor.In(),

            Children = {
                new Label {
                    Alignment = TextAlignmentOptions.Capline,

                    sEnabled = item.Map(x => x.Text != null),
                    sText = item.Map(x => x.Text),
                    sColor = fgColor.In()
                }.AsFlexItem(),

                new Image {
                    FlexItem = {
                        Margin = new() { top = 2.pt, bottom = 2.pt },
                        AspectRatio = 1f
                    },

                    PreserveAspect = true,

                    sEnabled = item.Map(x => x.Icon != null),
                    sSprite = item.Map(x => x.Icon),
                    sColor = fgColor.In()
                },
            }
        };
    }

    #endregion
}