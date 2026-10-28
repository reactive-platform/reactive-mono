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

    public string? Title {
        get;
        set {
            field = value;
            _modal.Modal?.Title = value;
        }
    }

    public bool ShowBackground {
        get;
        set {
            field = value;
            _modal.Modal?.ShowBackground = value;
        }
    } = true;

    public YogaVector CircleSize {
        get;
        set {
            field = value;
            _modal.Modal?.CircleSize = value;
        }
    } = BsColorCircleModalView.DefaultCircleSize;

    #endregion

    #region Impl

    private SharedModal<BsColorCircleModalView> _modal = null!;
    private BsColorPickerContext? _context;
    private StateSubscription? _contextSubscription;

    protected override GameObject Construct() {
        return new SharedModal<BsColorCircleModalView> {
            ApplyProps = ApplyModalProps
        }.Bind(ref _modal).Use();
    }

    private void ApplyModalProps(BsColorCircleModalView modal) {
        modal.PlacementAnchor = PlacementAnchor;
        modal.PlacementData = PlacementData;
        modal.CompositionAnimation = CompositionAnimation;
        modal.Animated = Animated;
        modal.ClickOffCloses = ClickOffCloses;
        modal.Title = Title;
        modal.ShowBackground = ShowBackground;
        modal.CircleSize = CircleSize;
        modal.Context = _context;
    }

    private void HandleContextUpdated(BsColorPickerContext context) {
        _modal.IsPushed = context.Focused;
    }

    #endregion
}
