using System;
using System.Collections.Generic;
using HMUI;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using BsImage = Reactive.BeatSaber.Components.Image;

namespace Reactive.BeatSaber.Components;

public record struct BsListControlItem<T>(T Key, string? Text, Sprite? Icon);

[PublicAPI]
public partial class BsListControl<T> : ReactiveComponent, ISkewedComponent {
    #region Public API

    [Required]
    public IReadOnlyList<BsListControlItem<T>> Items {
        get => _items!;
        set {
            _items = value;

            if (_initialized) {
                if (FindKey(Key) is var idx && idx != _selectedIdx.Value) {
                    SetKey(idx);
                }
            } else {
                DoInitialUpdate();
            }
        }
    }

    [Required]
    public T Key {
        get => _key!;
        set {
            if (_initialized) {
                SetKey(value);
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
        get => _interactable.Value;
        set => _interactable.Value = value;
    }

    public Action<T>? OnKeyChanged { get; set; }

    private bool _initialized;
    private IReadOnlyList<BsListControlItem<T>>? _items;
    private T? _key;

    private void DoInitialUpdate() {
        if (!_initialized && _key != null && _items != null) {
            SetKey(_key);
            _initialized = true;
        }
    }

    private void SetKey(T key) {
        var index = -1;

        if (_items != null) {
            for (var i = 0; i < _items.Count; i++) {
                if (EqualityComparer<T>.Default.Equals(_items[i].Key, key)) {
                    index = i;
                }
            }
        }

        if (index is -1) {
            throw new KeyNotFoundException();
        }

        SetKey(index);
    }

    private void SetKey(int index) {
        _key = _items![index].Key;
        _selectedIdx.Value = index;
        OnKeyChanged?.Invoke(_key);
    }

    private int FindKey(T key) {
        var index = -1;

        if (_items == null) {
            return index;
        }

        for (var i = 0; i < _items.Count; i++) {
            if (EqualityComparer<T>.Default.Equals(_items[i].Key, key)) {
                index = i;
            }
        }

        return index;
    }

    #endregion

    #region Construct

    private State<float> _skew = null!;
    private State<int> _selectedIdx = null!;
    private State<bool> _interactable = null!;

    protected override GameObject Construct() {
        _skew = Remember(BeatSaberStyle.Skew);
        _selectedIdx = Remember(0);
        _interactable = Remember(true);

        var selected = _selectedIdx.Map(x => _items?[x]);

        var nextAvailable = RememberDerived(
            x => x._interactable && x._selectedIdx < _items?.Count - 1,
            (_selectedIdx, _interactable)
        );

        var prevAvailable = RememberDerived(
            x => x._interactable && x._selectedIdx > 0 && _items?.Count > 1,
            (_selectedIdx, _interactable)
        );

        return new Layout {
            FlexItem = {
                Size = new() { x = 40.pt, y = 6.pt }
            },

            FlexController = {
                JustifyContent = Justify.Center
            },

            Children = {
                new Label {
                    Alignment = TextAlignmentOptions.Capline,
                    sFontStyle = _skew.Map(x => x > 0 ? FontStyles.Italic : FontStyles.Normal),

                    sText = selected.Map(x => x?.Text).Where(x => x != null),
                    sEnabled = selected.Map(x => x?.Text != null)
                }.AsFlexItem(),

                new BsImage {
                    FlexItem = {
                        Margin = new() { top = 2.pt, bottom = 2.pt },
                        AspectRatio = 1f
                    },

                    PreserveAspect = true,
                    sSkew = _skew,

                    sSprite = selected.Map(x => x?.Icon).Where(x => x != null),
                    sEnabled = selected.Map(x => x?.Icon != null)
                },

                CreateButton(true, () => SetKey(_selectedIdx.Value - 1), prevAvailable, _skew),
                CreateButton(false, () => SetKey(_selectedIdx.Value + 1), nextAvailable, _skew)
            }
        }.Use();
    }

    [StateGen]
    private static IReactiveComponent CreateButton(bool left, Action callback, IState<bool> interactable, IState<float> skew) {
        var hovered = Remember(false);

        var highlighted = RememberDerived(
            x => x.hovered.Value && x.interactable.Value,
            (hovered, interactable)
        );

        var color0 = highlighted.Map(x => Color.white with { a = x ? 0.2f : 1f });
        var color1 = highlighted.Map(x => x ? Color.black with { a = 0.5f } : Color.white);

        return new Background {
            FlexController = {
                Padding = 1.5f,
                JustifyContent = left ? Justify.FlexStart : Justify.FlexEnd
            },

            FlexItem = {
                Size = new() { x = 50.pct, y = 100.pct },
                Position = left ?
                    new() { top = 0.pt, left = 0.pt } :
                    new() { top = 0.pt, right = 0.pt },
                PositionType = PositionType.Absolute
            },

            sColor = highlighted.Map(x => x ? Color.white : Color.black with { a = 0.5f }),

            sGradientColor0 = left ? color0 : color1,
            sGradientColor1 = left ? color1 : color0,

            UseGradient = true,
            GradientDirection = ImageView.GradientDirection.Horizontal,

            Sprite = left ? BeatSaberResources.Sprites.backgroundLeft : BeatSaberResources.Sprites.backgroundRight,
            Material = GameResources.UINoGlowMaterial,
            PixelsPerUnit = 12f,
            
            sSkew = skew.In(),

            Do = x => x.WithPointerEvents(
                onEnter: _ => hovered.Value = true,
                onLeave: _ => hovered.Value = false,
                onDown: _ => {
                    if (interactable.Value) {
                        callback();
                    }
                }
            ),

            Children = {
                new BsImage {
                    FlexItem = {
                        AspectRatio = 1f
                    },

                    Sprite = GameResources.ArrowIcon,
                    PreserveAspect = true,

                    sColor = interactable.Map(x => x ?
                        Color.white with { a = 0.8f } :
                        Color.white with { a = 0.25f } * 0.9f
                    ),

                    ContentTransform = {
                        localEulerAngles = new(0f, 0f, left ? 270f : 90f)
                    }
                }
            }
        };
    }

    #endregion
}