using JetBrains.Annotations;
using Reactive.Yoga;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public abstract class DialogBase : ModalBase {
    #region Public API

    public string Title {
        get => _header.Text;
        set => _header.Text = value;
    }

    public BsTextButton CancelButton => _cancelButton;
    public BsPrimaryTextButton OkButton => _okButton;

    #endregion

    #region Construct

    private DialogHeader _header = null!;
    private BsTextButton _cancelButton = null!;
    private BsPrimaryTextButton _okButton = null!;

    /// <summary>
    /// Constructs the content placed between the header and the buttons.
    /// </summary>
    protected abstract IReactiveComponent ConstructDialogContent();

    protected sealed override IReactiveComponent ConstructContent() {
        return new Background {
            FlexController = {
                FlexDirection = FlexDirection.Column,
                ConstrainHorizontal = false,
                ConstrainVertical = false
            },

            Children = {
                new DialogHeader {
                    FlexItem = {
                        FlexBasis = 6.pt
                    }
                }.Bind(ref _header),

                ConstructDialogContent().With(x => x.FlexItem.Flex = 1f),

                new Layout {
                    FlexItem = {
                        FlexBasis = 8f
                    },

                    FlexController = {
                        Padding = 1.pt,
                        Gap = 1.pt
                    },

                    Children = {
                        new BsTextButton {
                            Name = "CancelButton",

                            FlexItem = {
                                Flex = 1f,
                                Size = YogaVector.Undefined
                            },

                            Text = "Cancel",
                            Skew = 0f
                        }.Bind(ref _cancelButton),

                        new BsPrimaryTextButton {
                            Name = "OkButton",

                            FlexItem = {
                                Flex = 1f,
                                Size = YogaVector.Undefined
                            },

                            Text = "OK",
                            Skew = 0f
                        }.Bind(ref _okButton)
                    }
                }
            }
        }.AsBlurBackground();
    }

    #endregion
}
