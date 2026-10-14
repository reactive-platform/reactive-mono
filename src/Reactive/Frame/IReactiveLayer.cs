using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A layer of the work Reactive does each frame. Work is scheduled into it during the frame, and
/// <see cref="ReactiveFrame"/> runs it after the layers below it and before the layers above it.
/// </summary>
[PublicAPI]
public interface IReactiveLayer {
    /// <summary>
    /// Whether the layer has work scheduled.
    /// </summary>
    bool HasWork { get; }

    /// <summary>
    /// Does the scheduled work. It may schedule work into any layer; the frame runs a lower layer again before
    /// continuing up.
    /// </summary>
    void Run();
}
