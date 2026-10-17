using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A stateless track that encapsulates which values are applied to a component. Tracks are meant to be shared,
/// so a single instance can be used by any number of components.
/// </summary>
/// <typeparam name="T">A type of the component the track can be applied to.</typeparam>
[PublicAPI]
public interface IAnimationTrack<in T> {
    /// <summary>
    /// Applies the track to the target.
    /// </summary>
    /// <param name="target">The component being animated.</param>
    /// <param name="progress">An animation progress with the curve already applied, so it may go beyond 0..1.</param>
    void Apply(T target, float progress);
}
