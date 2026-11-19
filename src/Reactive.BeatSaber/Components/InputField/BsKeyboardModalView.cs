using HMUI;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reactive.BeatSaber.Components;

internal class BsKeyboardModalView : ModalBase, IPoolableComponent {
    #region Public API

    /// <summary>
    /// A context to edit. The keyboard picks up its text when assigned.
    /// </summary>
    public BsInputFieldContext? Context {
        get;
        set {
            field = value;
            _keyboard.Text = value?.Text;
        }
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

    public bool ShowNumbersRow {
        get => _keyboard.ShowNumbersRow;
        set => _keyboard.ShowNumbersRow = value;
    }

    public bool ShowDetachButton {
        get => _showDetachButton.Value;
        set => _showDetachButton.Value = value;
    }

    public bool Detached {
        get => _detached.Value;
        set {
            if (value) {
                Detach();
            } else {
                Attach();
            }
        }
    }

    #endregion

    #region Detaching

    private readonly AnimatedLayoutApplicator _handleApplicator = LayoutApplicator.Animated(12.fact);
    private readonly AnimatedLayoutApplicator _keyboardApplicator = LayoutApplicator.Animated(12.fact);
    private readonly AnimatedLayoutApplicator _backgroundApplicator = LayoutApplicator.Animated(12.fact);

    private AnimatedState<Vector2> _position = null!;
    private bool _returnOnPlace;
    private bool _completeOnPlace;
    private bool _placed;

    protected override void PlaceContent(RectTransform content) {
        if (_completeOnPlace) {
            _completeOnPlace = false;

            _handleApplicator.Complete();
            _keyboardApplicator.Complete();
            _backgroundApplicator.Complete();
        }

        if (_detached.Value && _placed) {
            return;
        }

        _placed = true;

        var animated = _returnOnPlace || !_position.IsFinished;
        _returnOnPlace = false;

        var pivot = content.pivot;
        base.PlaceContent(content);

        if (!animated) {
            _position.SetValueImmediate(content.localPosition);
            return;
        }

        var size = Vector2.Scale(content.rect.size, content.localScale);

        _position.SetValueImmediate(_position.Value + Vector2.Scale(content.pivot - pivot, size));
        _position.TargetValue = content.localPosition;
    }

    private void Detach() {
        if (_detached.Value) {
            return;
        }

        BlocksOutside = false;
        _detached.Value = true;
        StopReturning();
    }

    private void Attach() {
        if (!_detached.Value) {
            return;
        }

        BlocksOutside = true;
        _returnOnPlace = true;
        _detached.Value = false;
        HandleRelease();
    }

    private void StopReturning() {
        _returnOnPlace = false;
        _position.SetValueImmediate(_position.Value);
    }

    #endregion

    #region Moving

    private PointerEventData? _press;
    private bool _grabbed;
    private Vector2 _grabOffset;
    private CurvedCanvasSettings? _curvedCanvas;

    private void HandlePress(PointerEventsHandler handler) {
        _press = handler.EventData;
        _grabbed = false;
        _gripGraphic.IsActive = true;
        _curvedCanvas = Content.GetComponentInParent<CurvedCanvasSettings>();
    }

    private void HandleRelease() {
        _press = null;
        _gripGraphic.IsActive = false;
    }

    private void RefreshMovement() {
        if (_press == null || !TryGetPointer(_press, out var point)) {
            return;
        }

        if (!_grabbed) {
            _grabbed = true;
            _grabOffset = _position.Value - point;
            return;
        }

        _position.SetValueImmediate(point + _grabOffset);
    }

    private bool TryGetPointer(PointerEventData data, out Vector2 point) {
        if (data.position == Vector2.zero || !data.pointerCurrentRaycast.isValid ||
            _contentTransform.parent is not RectTransform parent || Canvas is not { } canvas) {
            point = default;
            return false;
        }

        point = data.TranslateToLocalPoint(parent, canvas, _curvedCanvas);
        return true;
    }

    protected override void OnUpdate() {
        if (_detached.Value) {
            RefreshMovement();
        }
    }

    #endregion

    #region Impl

    private BsKeyboard _keyboard = null!;
    private Background _background = null!;
    private RectTransform _contentTransform = null!;

    private State<bool> _showDetachButton = null!;
    private State<bool> _detached = null!;
    private State<GraphicState> _gripGraphic = null!;
    private State<GraphicState> _detachGraphic = null!;

    [StateGen]
    protected override IReactiveComponent ConstructContent() {
        _showDetachButton = Remember(false);
        _detached = Remember(false);
        _gripGraphic = Remember(GraphicState.None);
        _detachGraphic = Remember(GraphicState.None);
        _position = RememberAnimated(Vector2.zero, 12.fact);

        var detachColors = new ColorSet {
            Color = Color.white.ColorWithAlpha(0.5f),
            HoveredColor = Color.white
        };

        var gripColors = new ColorSet {
            Color = Color.white.ColorWithAlpha(0.25f),
            HoveredColor = Color.white.ColorWithAlpha(0.6f),
            ActiveColor = Color.white.ColorWithAlpha(0.9f)
        };

        return new Layout {
            Name = "Content",

            FlexController = {
                ConstrainHorizontal = false,
                ConstrainVertical = false
            },

            ContentTransform = {
                slocalPosition = _position.Map(x => (Vector3)x)
            },

            Children = {
                new Background {
                    FlexController = {
                        FlexDirection = FlexDirection.Column,
                        AlignItems = Align.Center,
                        Padding = new() { left = 2.pt, right = 2.pt, bottom = 2.pt }
                    },

                    LayoutApplicator = _backgroundApplicator,

                    Children = {
                        new Background {
                            Name = "Handle",

                            FlexController = {
                                JustifyContent = Justify.Center,
                                AlignItems = Align.Center
                            },

                            FlexItem = {
                                AlignSelf = Align.Stretch,
                                sSize = _detached.Map(static x => new YogaVector { y = (x ? 6f : 2f).pt })
                            },

                            LayoutApplicator = _handleApplicator,

                            Sprite = ReactiveResources.TransparentPixel,
                            sRaycastTarget = _detached,

                            Do = x => x.WithPointerEvents(
                                onEnter: _ => _gripGraphic.IsHovered = true,
                                onLeave: _ => _gripGraphic.IsHovered = false,
                                onDown: HandlePress,
                                onUp: _ => HandleRelease()
                            ),

                            Children = {
                                new Background {
                                    FlexItem = {
                                        sSize = _gripGraphic.Map(static x => new YogaVector((x.IsHovered || x.IsActive ? 24f : 16f).pt, 1.pt))
                                    },

                                    LayoutApplicator = LayoutApplicator.Animated(12.fact),
                                    sEnabled = _detached,

                                    Sprite = BeatSaberResources.Sprites.background,
                                    PixelsPerUnit = 40f,
                                    sColor = _gripGraphic.MapColorSet(gripColors)
                                }
                            }
                        },

                        new Layout {
                            FlexController = {
                                AlignItems = Align.FlexStart
                            },

                            LayoutApplicator = _keyboardApplicator,

                            Children = {
                                new BsKeyboard {
                                    FlexItem = {
                                        Size = YogaValue.FitContent
                                    },

                                    OnOkClicked = HandleClose,
                                    OnTextChanged = HandleKeyboardTextChanged
                                }.Bind(ref _keyboard),

                                new Background {
                                    Name = "DetachButton",

                                    FlexController = {
                                        AlignItems = Align.Center,
                                        Gap = 0.8f.pt
                                    },

                                    FlexItem = {
                                        PositionType = PositionType.Absolute,
                                        Position = new() { left = 0.pt, bottom = 0.pt },
                                        Size = new() { y = 7.pt }
                                    },

                                    sEnabled = _showDetachButton,

                                    Sprite = ReactiveResources.TransparentPixel,
                                    RaycastTarget = true,

                                    Do = x => x.WithPointerEvents(
                                        onEnter: _ => _detachGraphic.IsHovered = true,
                                        onLeave: _ => _detachGraphic.IsHovered = false,
                                        onDown: _ => {
                                            GameResources.ButtonClickSignal.Raise();
                                            Detached = !Detached;
                                        }
                                    ),

                                    Children = {
                                        new Image {
                                            FlexItem = {
                                                Size = 2.2f.pt
                                            },

                                            ContentTransform = {
                                                slocalEulerAngles = _detached.Map(static x => new Vector3(0f, 0f, x ? 135f : -45f))
                                            },

                                            Sprite = GameResources.ArrowUpIcon,
                                            PreserveAspect = true,
                                            sColor = _detachGraphic.MapColorSet(detachColors)
                                        },

                                        new Label {
                                            FontSize = 2.6f,
                                            Alignment = TextAlignmentOptions.MidlineLeft,
                                            sText = _detached.Map(static x => x ? "Attach" : "Detach"),
                                            sColor = _detachGraphic.MapColorSet(detachColors)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }.AsBlurBackground().Bind(ref _background)
            }
        }.Bind(ref _contentTransform);
    }

    protected override void OnInitialize() {
        OnClickOutside = HandleClickOutside;

        BeatSaberUtils.AddCanvas(_contentTransform.gameObject);
        BeatSaberUtils.Make3DRaycastable(_contentTransform.gameObject);
    }

    private void HandleKeyboardTextChanged(string? text) {
        if (Context == null) {
            return;
        }

        Context.Text = text;

        // The context might have rejected the text, so the keyboard is brought back in sync
        if (Context.Text != text) {
            _keyboard.Text = Context.Text;
        }
    }

    private void HandleClickOutside() {
        if (ClickOffCloses) {
            HandleClose();
        }
    }

    private void HandleClose() {
        Context?.Focused = false;
    }

    #endregion

    #region Pooling

    void IPoolableComponent.OnSpawn() { }

    void IPoolableComponent.OnDespawn() {
        Attach();
        StopReturning();

        _completeOnPlace = true;
        _placed = false;

        ResetModal();
        OnClickOutside = HandleClickOutside;

        Context = null;
        ClickOffCloses = true;
        ShowBackground = true;
        KeyboardColors = BeatSaberStyle.BsKeyboardColors;
        ShowNumbersRow = false;
        ShowDetachButton = false;

        _gripGraphic.IsHovered = false;
        _detachGraphic.IsHovered = false;
    }

    #endregion
}