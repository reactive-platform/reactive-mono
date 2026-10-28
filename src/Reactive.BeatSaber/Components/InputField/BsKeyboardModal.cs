using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
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

            // An open modal follows the replaced context
            _modal.Modal?.Context = value;

            HandleContextUpdated(_context);
        }
    }

    public RectTransform? PlacementAnchor {
        get;
        set {
            field = value;
            _modal.Modal?.PlacementAnchor = value;
        }
    }

    public PlacementData PlacementData {
        get;
        set {
            field = value;
            _modal.Modal?.PlacementData = value;
        }
    } = new() {
        Placement = RelativePlacement.BottomCenter,
        Offset = new(0f, 1f)
    };

    public CompositionAnimParams CompositionAnimation {
        get;
        set {
            field = value;
            _modal.Modal?.CompositionAnimation = value;
        }
    } = new() {
        DimCanvas = false
    };

    /// <inheritdoc cref="ModalBase.Animated"/>
    public bool Animated {
        get;
        set {
            field = value;
            _modal.Modal?.Animated = value;
        }
    } = true;

    public bool ClickOffCloses {
        get;
        set {
            field = value;
            _modal.Modal?.ClickOffCloses = value;
        }
    } = true;

    public bool ShowBackground {
        get;
        set {
            field = value;
            _modal.Modal?.ShowBackground = value;
        }
    } = true;

    public BsKeyboardColors KeyboardColors {
        get;
        set {
            field = value;
            _modal.Modal?.KeyboardColors = value;
        }
    } = BeatSaberStyle.BsKeyboardColors;

    #endregion

    #region Impl

    private SharedModal<BsKeyboardModalView> _modal = null!;
    private BsInputFieldContext? _context;
    private StateSubscription? _contextSubscription;

    protected override GameObject Construct() {
        return new SharedModal<BsKeyboardModalView> {
            ApplyProps = ApplyModalProps
        }.Bind(ref _modal).Use();
    }

    private void ApplyModalProps(BsKeyboardModalView modal) {
        modal.PlacementAnchor = PlacementAnchor;
        modal.PlacementData = PlacementData;
        modal.CompositionAnimation = CompositionAnimation;
        modal.Animated = Animated;
        modal.ClickOffCloses = ClickOffCloses;
        modal.ShowBackground = ShowBackground;
        modal.KeyboardColors = KeyboardColors;
        modal.Context = _context;
    }

    private void HandleContextUpdated(BsInputFieldContext context) {
        _modal.IsPushed = context.Focused;
    }

    #endregion
}
