using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

public record struct BsDropdownItem(string? Text, Sprite? Icon);

public record struct BsDropdownModalParams {
    public BsDropdownModalParams() { }
    
    public int MaxVisibleRows { get; init; } = 5;
    public CompositionAnimParams CompositionAnimParams { get; init; } = new();

    public YogaValue Width { get; init; } = YogaValue.Undefined;
    public YogaValue MinWidth { get; init; } = 24.pt;
    public YogaValue MaxWidth { get; init; } = 46.pt;
}

public record struct BsDropdownColors(
    ColorSet ButtonBackgroundColors,
    ColorSet ArrowColors,
    ColorSet ItemContentColors,
    ColorSet ItemBackgroundColors
);

[PublicAPI]
public partial class BsDropdown<T> : ReactiveComponent, ISkewedComponent, IInteractableComponent {
    #region Public API

    [Required]
    public IReadOnlyDictionary<T, BsDropdownItem> Items {
        get => _items!;
        set {
            _items = value;
            _itemsState.Value = value;
            _itemsState.EnforceInvalidation();
            _keys = value.Keys.ToArray();

            SyncModal();
            DoInitialUpdate();
        }
    }

    [Required]
    public T Key {
        get => _key.Value!;
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

    public bool Interactable {
        get => _graphic.IsInteractable;
        set => _graphic.IsInteractable = value;
    }

    public BsDropdownColors Colors {
        get => _colors.Value;
        set {
            _colors.Value = value;
            SyncModal();
        }
    }

    public BsDropdownModalParams ModalParams {
        get;
        set {
            field = value;
            SyncModal();
        }
    } = new();

    public Action<T>? OnKeyChanged { get; set; }

    private bool _initialized;
    private IReadOnlyDictionary<T, BsDropdownItem>? _items;
    private Optional<T> _key;
    private T[] _keys = [];

    private void DoInitialUpdate() {
        if (!_initialized && _key.HasValue && _items != null) {
            var key = _key.Value!;
            _key = Optional<T>.None();

            SetKey(key, true);
            _initialized = true;
        }
    }

    private void SetKey(T value, bool updateModal) {
        if (_key.HasValue && EqualityComparer<T>.Default.Equals(_key.Value!, value)) {
            return;
        }

        _key = value;
        _keyState.Value = value;

        if (updateModal) {
            SyncModal();
        }

        OnKeyChanged?.Invoke(value);
    }

    #endregion

    #region Construct

    private State<float> _skew = null!;
    private State<T?> _keyState = null!;
    private State<IReadOnlyDictionary<T, BsDropdownItem>?> _itemsState = null!;
    private State<BsDropdownColors> _colors = null!;
    private State<bool> _modalOpened = null!;
    private State<GraphicState> _graphic = null!;
    private SharedModal<BsDropdownModal> _modal = null!;

    protected override GameObject Construct() {
        _keyState = Remember<T?>(default);
        _itemsState = Remember<IReadOnlyDictionary<T, BsDropdownItem>?>(null);
        _skew = Remember(BeatSaberStyle.Skew);
        _colors = Remember(BeatSaberStyle.BsDropdownColors);

        _modalOpened = Remember(false);
        _graphic = Remember(GraphicState.None);

        var contentColor = _graphic.MapColorSet(_colors, x => x.ItemContentColors);
        var bgColor = _graphic.MapColorSet(_colors, x => x.ButtonBackgroundColors);

        var item = RememberDerived(
            _keyState, _itemsState,
            (key, items) => key is { } k && items?.TryGetValue(k, out var i) == true ? i : (BsDropdownItem?)null
        );

        return new Background {
            FlexController = {
                Padding = new() { left = 5.pt, right = 5.pt },
                Gap = 0.5f.pt,
                JustifyContent = Justify.Center,
                AlignItems = Align.Center
            },

            FlexItem = {
                Size = new() { y = 6.pt, x = 36.pt }
            },

            Sprite = BeatSaberResources.Sprites.background,
            Material = GameResources.UINoGlowMaterial,
            PixelsPerUnit = 12f,

            sSkew = _skew,
            sColor = bgColor.In(),

            Do = x => {
                x.WithPointerEvents(
                    onEnter: _ => _graphic.IsHovered = true,
                    onLeave: _ => _graphic.IsHovered = false,
                    onDown: _ => {
                        if (!_graphic.IsInteractable) {
                            return;
                        }

                        _modalOpened.Value = true;
                        GameResources.ButtonClickSignal.Raise();
                    }
                );
            },

            Children = {
                new Label {
                    FlexItem = {
                        FlexShrink = 1f
                    },

                    EnableWrapping = false,
                    Overflow = TextOverflowModes.Ellipsis,
                    EnableAutoSizing = true,
                    FontSizeMin = 2f,
                    FontSizeMax = 4f,

                    Alignment = TextAlignmentOptions.Capline,
                    sFontStyle = _skew.Map(x => x > 0 ? FontStyles.Italic : FontStyles.Normal),

                    sEnabled = item.Map(x => x?.Text != null),
                    sText = item.Map(x => x?.Text),
                    sColor = contentColor.In(),
                },

                new Image {
                    FlexItem = {
                        Size = new() { y = 50.pct },
                        AspectRatio = 1f
                    },

                    PreserveAspect = true,
                    sSkew = _skew,

                    sEnabled = item.Map(x => x?.Icon != null),
                    sSprite = item.Map(x => x?.Icon),
                    sColor = contentColor.In()
                },

                new Image {
                    Name = "Arrow",

                    FlexItem = {
                        PositionType = PositionType.Absolute,
                        Size = new() { y = 45.pct },
                        AspectRatio = 1f,
                        Position = new() { right = 2f.pt }
                    },

                    Sprite = GameResources.ArrowIcon,
                    PreserveAspect = true,
                    sColor = _graphic.MapColorSet(_colors, x => x.ArrowColors).In()
                },

                new SharedModal<BsDropdownModal> {
                    sIsPushed = _modalOpened,
                    ApplyProps = ApplyModalProps
                }.Bind(ref _modal)
            }
        }.Use();
    }

    private void ApplyModalProps(BsDropdownModal modal) {
        modal.PlacementAnchor = ContentTransform;
        modal.OnClickOutside = () => _modalOpened.Value = false;

        modal.OnItemSelected = x => {
            SetKey(_keys[x], false);
            _modalOpened.Value = false;
        };

        UpdateModal(modal);
    }

    private void SyncModal() {
        // Only an open modal is ours to update, the rest is applied once it's taken from the pool
        if (_modal?.Modal is { } modal) {
            UpdateModal(modal);
        }
    }

    private float ResolveModalWidth() {
        var width = ResolveWidth(ModalParams.Width, ContentTransform.rect.width);
        var min = ResolveWidth(ModalParams.MinWidth, 0f);
        var max = ResolveWidth(ModalParams.MaxWidth, float.PositiveInfinity);
        
        return Mathf.Max(min, Mathf.Min(width, max));
    }

    private float ResolveWidth(YogaValue value, float fallback) {
        return value.unit switch {
            Unit.Percent => ContentTransform.rect.width * value.value / 100f,
            Unit.Point => value.value,
            _ => fallback
        };
    }

    private void UpdateModal(BsDropdownModal modal) {
        modal.CompositionAnimation = ModalParams.CompositionAnimParams;
        modal.Colors = _colors.Value;
        modal.MaxVisibleRows = ModalParams.MaxVisibleRows;
        modal.Width = ResolveModalWidth();
        modal.Items = _items == null ? [] : _keys.Select(x => _items[x]).ToArray();
        modal.SelectedIndex = _key.HasValue ? Array.IndexOf(_keys, _key.Value!) : -1;
    }

    #endregion
}