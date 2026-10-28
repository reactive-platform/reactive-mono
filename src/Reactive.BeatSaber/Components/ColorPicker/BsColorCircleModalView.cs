using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

internal class BsColorCircleModalView : ModalBase, IPoolableComponent {
    #region Public API

    /// <summary>
    /// A context to edit. The circle picks up its selected color when assigned.
    /// </summary>
    public BsColorPickerContext? Context {
        get;
        set {
            field = value;

            // Synced without echoing it back
            if (value != null) {
                _circle.SetColor(value.SelectedColor, false);
            }
        }
    }

    public bool ClickOffCloses { get; set; } = true;

    public string? Title {
        get => _header.Enabled ? _header.Text : null;
        set {
            _header.Enabled = value != null;
            _header.Text = value ?? string.Empty;
        }
    }

    public bool ShowBackground {
        get => _background.Enabled;
        set => _background.Enabled = value;
    }

    public YogaVector CircleSize {
        get => _circle.FlexItem.Size;
        set => _circle.FlexItem.Size = value;
    }

    #endregion

    #region Impl

    internal static readonly YogaVector DefaultCircleSize = 40.pt;

    private BsColorCircle _circle = null!;
    private DialogHeader _header = null!;
    private Background _background = null!;

    protected override IReactiveComponent ConstructContent() {
        return new Layout {
            Name = "Content",

            FlexController = {
                ConstrainHorizontal = false,
                ConstrainVertical = false
            },

            Children = {
                new Background {
                    FlexController = {
                        FlexDirection = FlexDirection.Column
                    },

                    Children = {
                        new DialogHeader {
                            Enabled = false,

                            FlexItem = {
                                FlexBasis = 6.pt
                            }
                        }.Bind(ref _header),

                        new Layout {
                            FlexController = {
                                Padding = 2.pt
                            },

                            Children = {
                                new BsColorCircle {
                                    FlexItem = {
                                        Size = DefaultCircleSize
                                    },

                                    OnColorUpdated = HandleColorUpdated,
                                    OnColorSaved = HandleColorSaved
                                }.Bind(ref _circle)
                            }
                        }
                    }
                }.AsBlurBackground().AsFlexItem().Bind(ref _background)
            }
        };
    }

    protected override void OnInitialize() {
        OnClickOutside = HandleClickOutside;
    }

    private void HandleColorUpdated(Color color) {
        Context?.Color = color;
    }

    private void HandleColorSaved(Color color) {
        Context?.SelectedColor = color;
    }

    private void HandleClickOutside() {
        if (ClickOffCloses) {
            Context?.Focused = false;
        }
    }

    #endregion

    #region Pooling

    void IPoolableComponent.OnSpawn() { }

    void IPoolableComponent.OnDespawn() {
        ResetModal();
        OnClickOutside = HandleClickOutside;

        Context = null;
        ClickOffCloses = true;
        Title = null;
        ShowBackground = true;
        CircleSize = DefaultCircleSize;
    }

    #endregion
}
