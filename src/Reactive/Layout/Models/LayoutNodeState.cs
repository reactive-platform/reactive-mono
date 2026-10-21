using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A state holding info related to layout recalculation and invalidation.
/// Used for efficient layout calculation and is not readable or writable from outside.
/// </summary>
[PublicAPI]
public struct LayoutNodeState {
    // The pass this node is queued for
    internal int QueuedFor;

    // The pass in which this node passed its change to its driver
    internal int PropagatedFor;

    // The pass in which this node was collected as a root to recalculate
    internal int RootFor;

    // The pass in which this node's own controller recalculated its children
    internal int RecalculatedFor;

    // The dimensions applied last time, to skip applying the same ones again
    internal LayoutDimensions LastApplied;
    internal bool HasApplied;
}
