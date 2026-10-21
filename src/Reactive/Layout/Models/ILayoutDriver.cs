using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reactive {
    /// <summary>
    /// Represents an object that contains children. The driver itself does not control the children,
    /// but provides all necessary data to the layout controller if it's presented.
    /// </summary>
    [PublicAPI]
    public interface ILayoutDriver : ILayoutControllerHolder {
        /// <summary>
        /// An observable collection with children.
        /// </summary>
        ICollection<ILayoutItem> Children { get; }

        /// <summary>
        /// Starts an immediate layout recalculation going down from this node.
        /// Must be called on a root only, a driver controlled by its parent throws.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown when the driver is controlled by its parent.</exception>
        void RecalculateLayout();
    }
}