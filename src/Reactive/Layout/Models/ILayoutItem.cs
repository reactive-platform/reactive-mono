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
    public interface ILayoutItem : ILayoutNode, ILayoutMeasurable {
        /// <summary>
        /// A parent this layout item is attached to.
        /// </summary>
        ILayoutDriver? LayoutDriver { get; }
        
        /// <summary>
        /// A context used to stitch hierarchies together. 
        /// </summary>
        object? LayoutContext { get; }

        /// <inheritdoc cref="ILayoutModifier"/>
        ILayoutModifier? LayoutModifier { get; set; }
        
        /// <summary>
        /// Whether this node takes part in its parent's layout and occupies space there.
        /// Depends on the node alone, not on its parents. 
        /// </summary>
        /// <remarks>
        /// A node can be out of the layout implicitly (disabled) or explicitly (detached on purpose), the parent treats
        /// both the same way. A node out of the layout still lays out its own hierarchy as a separate tree while active.
        /// A node inside a hidden hierarchy is still within the layout.
        /// See <see cref="ILayoutNode.IsActiveInHierarchy"/> if you need the actual state.
        /// </remarks>
        bool WithinLayout { get; }

        /// <summary>
        /// Whether this node has content that defines its size, like text or an image. False means this node
        /// measures as an empty one, so controllers may skip calling <see cref="ILayoutMeasurable.Measure"/>.
        /// </summary>
        bool HasIntrinsicContent { get; }

        /// <summary>
        /// Invoked when some values affecting layout have changed.
        /// <see cref="LayoutModifier"/> and <see cref="WithinLayout"/> are some of those.
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