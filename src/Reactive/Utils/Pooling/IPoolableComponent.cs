using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A component that can be stored in <see cref="GlobalReactivePool"/> and reinitialized without being reconstructed.
/// </summary>
[PublicAPI]
public interface IPoolableComponent : IReactiveComponent {
    /// <summary>
    /// Called when the component is taken from the pool, right before it is handed out.
    /// </summary>
    void OnSpawn();

    /// <summary>
    /// Called when the component is returned to the pool. Must bring the component back to its
    /// initial state, dropping everything the previous user has set (callbacks, contexts, anchors, etc.),
    /// as the next user expects a freshly constructed instance.
    /// </summary>
    void OnDespawn();
}
