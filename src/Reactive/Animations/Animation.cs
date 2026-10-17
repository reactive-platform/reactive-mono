using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// Defines how a track is played.
/// </summary>
/// <param name="Track">The track to play.</param>
/// <param name="Duration">The duration of the animation in seconds.</param>
/// <param name="Curve">A curve applied to the progress before passing it to the track. Linear if not set.</param>
/// <typeparam name="T">A type of the animated component.</typeparam>
[PublicAPI]
public readonly record struct Animation<T>(
    IAnimationTrack<T> Track,
    float Duration,
    AnimationCurve? Curve = null
) where T : IReactiveComponent;