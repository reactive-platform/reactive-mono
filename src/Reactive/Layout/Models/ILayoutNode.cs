using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// An object the layout scheduler can queue, track and invoke. Both items and drivers are nodes,
/// which allows a driver to be a root of a hierarchy without being laid out itself.
/// </summary>
[PublicAPI]
public interface ILayoutNode {
    /// <summary>
    /// A state containing data required for efficient layout recalculation.
    /// Does not affect the layout structure. Should not be written manually either.
    /// </summary>
    ref LayoutNodeState LayoutState { get; }

    /// <summary>
    /// Starts an immediate layout recalculation going down from this node. Does nothing for nodes
    /// without a layout of their own. Must be called on a root only, a node controlled by its parent throws.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">Thrown when the node is controlled by its parent.</exception>
    void RecalculateLayout();
}
