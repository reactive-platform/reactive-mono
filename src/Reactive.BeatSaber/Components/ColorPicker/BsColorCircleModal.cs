using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

/// <summary>
/// A modal with a <see cref="BsColorCircle"/> driven by a <see cref="BsColorPickerContext"/>,
/// the same way <see cref="BsKeyboardModal"/> serves <see cref="BsInputField"/>.
/// Opens while the context is focused, writes the live color to <see cref="BsColorPickerContext.Color"/>
/// and the saved one to <see cref="BsColorPickerContext.SelectedColor"/>.
/// </summary>
[PublicAPI]
public partial class BsColorCircleModal : ReactiveComponent {
    #region Public API

    [RawState, Required]
    public BsColorPickerContext Context {
        get => _context!;
        set {
            _contextSubscription?.RemoveCallback();

            _context = value;
            _contextSubscription = _context.AddCallback(HandleContextUpdated);

            HandleContextUpdated(_context);
        }
    }

    public RectTransform? PlacementAnchor {
        get => _modal.PlacementAnchor;
        set => _modal.PlacementAnchor = value;
    }

    public PlacementData PlacementData {
        get => _modal.PlacementData;
        set => _modal.PlacementData = value;
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

    private Modal _modal = null!;
    private BsColorCircle _circle = null!;
    private DialogHeader _header = null!;
    private Background _background = null!;
    private BsColorPickerContext? _context;
    private StateSubscription? _contextSubscription;

    protected override GameObject Construct() {
        return new Modal {
            PlacementData = new() {
                Placement = RelativePlacement.BottomCenter,
                Offset = new(0f, 1f)
            },

            OnClickOutside = HandleClickOutside,

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
                                        Size = new() { x = 60.pt, y = 40.pt }
                                    },

                                    OnColorUpdated = HandleColorUpdated,
                                    OnColorSaved = HandleColorSaved
                                }.Bind(ref _circle)
                            }
                        }
                    }
                }.AsBlurBackground().AsFlexItem().Bind(ref _background)
            }
        }.Bind(ref _modal).Use();
    }

    private void HandleContextUpdated(BsColorPickerContext context) {
        // The color could be changed outside, so syncing it on open without echoing it back
        if (context.Focused && !_modal.IsPushed) {
            _circle.SetColor(context.SelectedColor, false);
        }

        _modal.IsPushed = context.Focused;
    }

    private void HandleColorUpdated(Color color) {
        _context?.Color = color;
    }

    private void HandleColorSaved(Color color) {
        _context?.SelectedColor = color;
    }

    private void HandleClickOutside() {
        if (ClickOffCloses) {
            HandleClose();
        }
    }

    private void HandleClose() {
        _context?.Focused = false;
    }

    #endregion
}
