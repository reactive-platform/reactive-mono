using System.Collections.Generic;

namespace Reactive.Components {
    public interface IChildrenProvider {
        ICollection<ILayoutItem> Children { get; }
    }
}