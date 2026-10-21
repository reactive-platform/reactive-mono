using System;
using JetBrains.Annotations;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive {
    [PublicAPI]
    public record struct LayoutDimensions(
        float TopOffset,
        float LeftOffset,
        float Width,
        float Height
    );

    /// <summary>
    /// An object inside a layout tree.
    /// </summary>
    [PublicAPI]
    public interface ILayoutItem : ILayoutMeasurable {
        /// <summary>
        /// A parent this layout item is attached to.
        /// </summary>
        ILayoutDriver? LayoutDriver { get; }
        
        /// <summary>
        /// A context used to stitch hierarchies together. 
        /// </summary>
        object? LayoutContext { get; }

        /// <summary>
        /// A state containing data required for efficient layout recalculation.
        /// Does not affect the layout structure. Should not be written manually either.
        /// </summary>
        ref LayoutItemState LayoutState { get; }
        
        /// <inheritdoc cref="ILayoutModifier"/>
        ILayoutModifier? LayoutModifier { get; set; }
        
        /// <summary>
        /// Whether this node is enabled from the layout perspective. Items outside the layout
        /// are neither calculated by the parent nor recalculated on their own.
        /// </summary>
        bool WithinLayout { get; }

        /// <summary>
        /// Whether this node is intentionally detached from the layout calculation while being within the layout.
        /// Applies only to the parent hierarchy, so if the node itself is a driver
        /// it will proceed to work as a separate tree.
        /// </summary>
        bool IgnoreLayout { get; }

        /// <summary>
        /// Invoked when some values affecting layout have changed.
        /// <see cref="LayoutModifier"/>, <see cref="WithinLayout"/> and <see cref="IgnoreLayout"/> are some of those.
        /// Tracked by <see cref="ILayoutDriver"/>, shouldn't be subscribed directly.
        /// </summary>
        event Action<ILayoutItem>? LayoutDataUpdatedEvent;
        
        /// <summary>
        /// Invoked when the layout has been recalculated and applied.
        /// </summary>
        event Action<ILayoutItem>? LayoutAppliedEvent;

        /// <summary>
        /// Assigns a parent to this node. Only sets the properties, doesn't perform physical reparenting.
        /// </summary>
        /// <param name="driver">A driver to assign.</param>
        void AssignParent(ILayoutDriver? driver);

        /// <summary>
        /// Assigns a context to this node. This doesn't automatically mean that <see cref="LayoutContext"/> will
        /// be exposing this exact context as the own context has more priority than the assigned one.
        /// </summary>
        /// <param name="context"></param>
        void AssignContext(object? context);

        /// <summary>
        /// Physically attaches this item to the specified transform instance.
        /// Doesn't set any layout properties, operates purely on the physical layer.
        /// </summary>
        /// <param name="parent">A transform to attach to.</param>
        void AttachToParent(Transform? parent);

        /// <summary>
        /// Applies the calculated dimensions.
        /// </summary>
        void ApplyLayout(LayoutDimensions dimensions);

        /// <summary>
        /// Calculates and returns the item hash code.
        /// </summary>
        /// <returns>A hash code.</returns>
        int GetLayoutItemHashCode();

        /// <summary>
        /// Compares the current instance with another one.
        /// </summary>
        /// <param name="item">An instance to compare with.</param>
        /// <returns>True if items belong to the same host, otherwise False.</returns>
        bool EqualsToLayoutItem(ILayoutItem item);
    }
}