using Reactive.Yoga;

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

    #endregion

    #region Impl

    private BsKeyboard _keyboard = null!;
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
        };
    }

    protected override void OnInitialize() {
        OnClickOutside = HandleClickOutside;
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
        ResetModal();
        OnClickOutside = HandleClickOutside;

        Context = null;
        ClickOffCloses = true;
        ShowBackground = true;
        KeyboardColors = BeatSaberStyle.BsKeyboardColors;
    }

    #endregion
}
