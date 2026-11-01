using Reactive.Yoga;

namespace Reactive.BeatSaber.Components;

internal class BsNumpadModalView : ModalBase, IPoolableComponent {
    #region Public API

    /// <summary>
    /// A context to edit. The numpad picks up its text when assigned.
    /// </summary>
    public BsInputFieldContext? Context {
        get;
        set {
            field = value;
            _numpad.Text = value?.Text;
        }
    }

    public bool ClickOffCloses { get; set; } = true;

    public bool ShowBackground {
        get => _background.Enabled;
        set => _background.Enabled = value;
    }

    public BsNumpadColors NumpadColors {
        get => _numpad.Colors;
        set => _numpad.Colors = value;
    }

    #endregion

    #region Impl

    private BsNumpad _numpad = null!;
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
                        Padding = 2.pt
                    },

                    Children = {
                        new BsNumpad {
                            FlexItem = {
                                Size = YogaValue.FitContent
                            },

                            OnOkClicked = HandleClose,
                            OnTextChanged = HandleNumpadTextChanged
                        }.Bind(ref _numpad)
                    }
                }.AsBlurBackground().Bind(ref _background)
            }
        };
    }

    protected override void OnInitialize() {
        OnClickOutside = HandleClickOutside;
    }

    private void HandleNumpadTextChanged(string? text) {
        if (Context == null) {
            return;
        }

        Context.Text = text;

        // The context might have rejected the text, so the numpad is brought back in sync
        if (Context.Text != text) {
            _numpad.Text = Context.Text;
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
        ResetModal();
        OnClickOutside = HandleClickOutside;

        Context = null;
        ClickOffCloses = true;
        ShowBackground = true;
        NumpadColors = BeatSaberStyle.BsNumpadColors;
    }

    #endregion
}
