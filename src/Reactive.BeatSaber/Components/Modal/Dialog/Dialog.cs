using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class Dialog : DialogBase, ILayoutControllerHolder {
    #region Layout Driver

    public ICollection<ILayoutItem> Children => _layout.Children;

    public ILayoutController? LayoutController {
        get => _layout.LayoutController;
        set => _layout.LayoutController = value;
    }

    #endregion

    #region Construct

    private Layout _layout = null!;

    protected sealed override IReactiveComponent ConstructDialogContent() {
        return new Layout {
            Name = "Content"
        }.AsFlexGroup().Bind(ref _layout);
    }

    #endregion
}
