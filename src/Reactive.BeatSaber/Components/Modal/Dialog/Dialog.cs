using System.Collections.Generic;
using JetBrains.Annotations;
using Reactive.Yoga;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class Dialog : ModalBase, ILayoutDriver {
    #region Public API

    public string Title {
        get => _header.Text;
        set => _header.Text = value;
    }

    public BsTextButton CancelButton => _cancelButton;
    public BsPrimaryTextButton OkButton => _okButton;

    #endregion
    
    #region Layout Driver

    public ICollection<ILayoutItem> Children => _layout.Children;

    public ILayoutController? LayoutController {
        get => _layout.LayoutController;
        set => _layout.LayoutController = value;
    }

    #endregion

    #region Construct

    private DialogHeader _header = null!;
    private BsTextButton _cancelButton = null!;
    private BsPrimaryTextButton _okButton = null!;
    private Layout _layout = null!;

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

                new Layout {
                    Name = "Content",

                    FlexItem = {
                        Flex = 1f
                    }
                }.AsFlexGroup().Bind(ref _layout),

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