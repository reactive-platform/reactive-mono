using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

/// <summary>
/// A modal with a <see cref="BsKeyboard"/> bound to an input field context.
/// </summary>
[PublicAPI]
public partial class BsKeyboardModal : ReactiveComponent {
    #region Public API

    [RawState, Required]
    public BsInputFieldContext Context {
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

    public bool ShowBackground {
        get => _background.Enabled;
        set => _background.Enabled = value;
    }

    public BsKeyboardColors KeyboardColors {
        get => _keyboard.Colors;
        set => _keyboard.Colors = value;
    }

    #endregion

    #region Impl

    private Modal _modal = null!;
    private BsKeyboard _keyboard = null!;
    private Background _background = null!;
    private BsInputFieldContext? _context;
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
                        Padding = 2.pt
                    },

                    Children = {
                        new BsKeyboard {
                            FlexItem = {
                                Size = YogaValue.FitContent
                            },

                            OnOkClicked = HandleClose,
                            OnTextChanged = HandleKeyboardTextChanged
                        }.Bind(ref _keyboard)
                    }
                }.AsBlurBackground().AsFlexItem().Bind(ref _background)
            }
        }.Bind(ref _modal).Use();
    }

    private void HandleContextUpdated(BsInputFieldContext context) {
        // The text could be changed outside (e.g. cleared by the input field), so syncing it on open
        if (context.Focused && !_modal.IsPushed) {
            _keyboard.Text = context.Text;
        }

        _modal.IsPushed = context.Focused;
    }

    private void HandleKeyboardTextChanged(string? text) {
        _context?.Text = text;
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