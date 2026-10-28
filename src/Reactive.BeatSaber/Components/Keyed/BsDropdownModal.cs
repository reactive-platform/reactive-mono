using System;
using System.Collections.Generic;
using System.Linq;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

internal class BsDropdownModal : ModalBase, IPoolableComponent {
    #region Public API

    public IReadOnlyList<BsDropdownItem> Items {
        get => _items.Value;
        set {
            _items.Value = value;
            _table.Items = Enumerable.Range(0, value.Count).ToArray();

            RefreshListSize();
        }
    }

    /// <summary>
    /// The index of the selected item, or -1 if there's none. Doesn't invoke <see cref="OnItemSelected"/>.
    /// </summary>
    public int SelectedIndex {
        get => _table.SelectedItems.Count > 0 ? _table.SelectedItems.First() : -1;
        set {
            _settingSelection = true;
            _table.SelectedItems = value >= 0 ? [value] : [];
            _settingSelection = false;
            _pendingScrollIndex = value;
            ScrollToPending();
        }
    }

    public BsDropdownColors Colors {
        get => _colors.Value;
        set => _colors.Value = value;
    }

    public int MaxVisibleRows {
        get;
        set {
            field = value;
            RefreshListSize();
        }
    } = DefaultMaxVisibleRows;

    /// <summary>
    /// Called with the index of an item the user has clicked.
    /// </summary>
    public Action<int>? OnItemSelected { get; set; }

    #endregion

    #region Impl

    private const float RowHeight = 7f;
    private const int DefaultMaxVisibleRows = 5;

    private Table<int, IReactiveComponent> _table = null!;
    private State<IReadOnlyList<BsDropdownItem>> _items = null!;
    private State<BsDropdownColors> _colors = null!;
    private ScrollContext _scrollContext = null!;
    private bool _settingSelection;
    private int _pendingScrollIndex = -1;

    protected override IReactiveComponent ConstructContent() {
        _items = Remember<IReadOnlyList<BsDropdownItem>>(Array.Empty<BsDropdownItem>());
        _colors = Remember(BeatSaberStyle.BsDropdownColors);

        var scrollContext = new ScrollContext();
        _scrollContext = scrollContext;
        
        scrollContext.AddCallback(x => {
            if (x.UpdateType.IsMeasurements && ScrollToPending()) {
                _pendingScrollIndex = -1;
            }
        }, lazy: true);

        return new Layout {
            Name = "Content",

            FlexController = {
                ConstrainHorizontal = false,
                ConstrainVertical = false,
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
                        new Table<int, IReactiveComponent> {
                            FlexItem = {
                                Size = new() { x = 36.pt, y = (RowHeight * DefaultMaxVisibleRows).pt },
                                Margin = new() { top = 1.pt, bottom = 1.pt }
                            },

                            ScrollContext = scrollContext,
                            Items = Array.Empty<int>(),

                            OnSelectedItemsChanged = x => {
                                if (!_settingSelection && x.Count > 0) {
                                    OnItemSelected?.Invoke(x.First());
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
        };
    }

    protected override void OnInitialize() {
        ApplyDefaults();
    }

    private void ApplyDefaults() {
        PlacementData = new() {
            Placement = RelativePlacement.Center,
            Clip = true
        };
    }
    
    private bool ScrollToPending() {
        var lineSize = _scrollContext.LineSize;

        if (_pendingScrollIndex < 0 || lineSize <= 0f) {
            return false;
        }

        var pos = _pendingScrollIndex * lineSize - (_scrollContext.ViewSize - lineSize) / 2f;
        _scrollContext.ScrollTo(pos, true);

        return true;
    }

    private void RefreshListSize() {
        // The list fits its items and scrolls once there are more than fit
        var rows = Mathf.Clamp(Items.Count, 1, Mathf.Max(1, MaxVisibleRows));
        _table.FlexItem.Size = new() { x = 36.pt, y = (RowHeight * rows).pt };
    }

    [StateGen]
    private IReactiveComponent CreateCell(CellContext<int> context, IState<BsDropdownColors> colors) {
        var graphic = Remember(GraphicState.None);
        // Unused cells can hold indices beyond the items, so they resolve to an empty item
        var item = RememberDerived(
            x => x.context.Item < x._items.Value.Count ? x._items.Value[x.context.Item] : default,
            (context, _items)
        );

        var fgColor = graphic.MapColorSet(colors, x => x.ItemContentColors);
        var bgColor = graphic.MapColorSet(colors, x => x.ItemBackgroundColors);

        context.AddCallback(x => graphic.IsActive = x.Selected);

        return new Background {
            FlexController = {
                Gap = 1.pt,
                JustifyContent = Justify.Center
            },

            FlexItem = {
                Size = new() { x = 36.pt, y = RowHeight.pt }
            },

            Do = x => x.WithPointerEvents(
                onEnter: _ => graphic.IsHovered = true,
                onLeave: _ => graphic.IsHovered = false,
                onDown: _ => {
                    GameResources.ButtonClickSignal.Raise();
                    context.Selected = true;
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

    #region Pooling

    void IPoolableComponent.OnSpawn() { }

    void IPoolableComponent.OnDespawn() {
        ResetModal();
        ApplyDefaults();

        OnItemSelected = null;
        SelectedIndex = -1;

        _scrollContext.ScrollTo(0f, true);
        Items = Array.Empty<BsDropdownItem>();
        MaxVisibleRows = DefaultMaxVisibleRows;
        Colors = BeatSaberStyle.BsDropdownColors;
    }

    #endregion
}
