using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive {
    /// <summary>
    /// Represents a standalone layout controller.
    /// </summary>
    [PublicAPI]
    public interface ILayoutController : IContextMember, ILayoutMeasurable {
        int ChildCount { get; }

        event Action? LayoutControllerUpdatedEvent;

        /// <summary>
        /// Adds a child to the controller.
        /// </summary>
        /// <param name="child">A component to add.</param>
        /// <param name="index">An index to insert at.</param>
        void InsertChild(ILayoutItem child, int index);

        /// <summary>
        /// Removes a child from the controller.
        /// </summary>
        /// <param name="child">A component to remove.</param>
        void RemoveChild(ILayoutItem child);

        /// <summary>
        /// Removes all children from the controller.
        /// </summary>
        void RemoveAllChildren();

        /// <summary>
        /// Checks if the controller has the specified child added to its hierarchy.
        /// </summary>
        /// <param name="child">A component to search for.</param>
        /// <returns>True if the component is in hierarchy, otherwise False.</returns>
        bool HasChild(ILayoutItem child);

        /// <summary>
        /// Notifies the controller that the specified child has updated values.
        /// </summary>
        /// <param name="item">A child to update.</param>
        void UpdateChild(ILayoutItem item);

        /// <summary>
        /// Applies the own modifier of the item this controller belongs to. Participants get their
        /// modifier applied by the parent controller, so this is meaningful for roots only.
        /// </summary>
        /// <param name="item">An item the controller belongs to.</param>
        void UpdateSelf(ILayoutItem item);

        /// <summary>
        /// Causes layout recalculation. This method doesn't apply any physical dimensions.
        /// </summary>
        /// <param name="constraints">Constraints of the parent object.</param>
        void Recalculate(Vector2 constraints);

        /// <summary>
        /// Returns layout dimensions of the controller itself.
        /// Applicable only if the controller is not a layout participant.
        /// </summary>
        /// <returns></returns>
        LayoutDimensions GetSelfDimensions();

        /// <summary>
        /// Returns layout dimensions of the specified child. 
        /// </summary>
        /// <param name="child">A child whose dimensions should be returned.</param>
        LayoutDimensions GetChildDimensions(ILayoutItem child);
    }
}