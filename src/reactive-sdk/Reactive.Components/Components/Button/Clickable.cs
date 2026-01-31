using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

/// <summary>
/// A dummy button without graphic content. Will work only with a Graphic component inside it.
/// </summary>
[PublicAPI]
public class Clickable : ButtonBase, ILayoutDriver {
    #region Adapter

    public ICollection<ILayoutItem> Children => _layout.Children;

    public ILayoutController? LayoutController {
        get => _layout.LayoutController;
        set => _layout.LayoutController = value;
    }

    #endregion

    private Layout _layout = null!;

    protected override GameObject Construct() {
        return new Layout()
            .With(x => base.Construct(x.ContentTransform))
            .Bind(ref _layout)
            .Use();
    }
}