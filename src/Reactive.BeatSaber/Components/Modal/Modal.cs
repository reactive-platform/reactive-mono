using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reactive.Components;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

/// <summary>
/// A wrapper overlay that carries a bounded content view.
/// Provides support for placing and managing a view inside an overlay.
/// </summary>
[PublicAPI]
public class Modal : ModalBase, ILayoutDriver {
    #region Impl

    public ICollection<ILayoutItem> Children => _content.Children;

    public ILayoutController? LayoutController {
        get => _content.LayoutController;
        set => _content.LayoutController = value;
    }

    private Layout _content = null!;

    protected override IReactiveComponent ConstructContent() {
        return new Layout {
            Name = "Content",

            FlexController = {
                ConstrainHorizontal = false,
                ConstrainVertical = false
            },
        }.Bind(ref _content);
    }

    #endregion
}