using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reactive.Components {
    [PublicAPI]
    public class Button : ButtonBase, IChildrenProvider {
        public new ICollection<ILayoutItem> Children => base.Children;
    }
}