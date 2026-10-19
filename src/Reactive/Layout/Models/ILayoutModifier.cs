using System;
using JetBrains.Annotations;

namespace Reactive {
    /// <summary>
    /// An object containing layout-specific properties for the parent.
    /// </summary>
    [PublicAPI]
    public interface ILayoutModifier : ICopiable<ILayoutModifier> {
        /// <summary>
        /// Fired when the modifier got updated. Tracked by <see cref="ILayoutItem"/>,
        /// shouldn't be subscribed directly.
        /// </summary>
        event Action? ModifierUpdatedEvent;
    }
}