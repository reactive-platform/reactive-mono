using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// Defines how a track is played.
/// </summary>
/// <param name="Track">The track to play.</param>
/// <param name="Duration">The duration of a single iteration.</param>
/// <param name="Curve">A curve applied to the progress before passing it to the track. Linear if not set.</param>
/// <param name="Iterations">How many times the animation is played. Once if not set.</param>
/// <param name="Direction">Which way each iteration is played.</param>
/// <typeparam name="T">A type of the animated target.</typeparam>
[PublicAPI]
public readonly record struct Animation<T>(
    IAnimationTrack<T> Track,
    AnimationDuration Duration,
    AnimationCurve? Curve = null,
    AnimationIteration Iterations = default,
    AnimationDirection Direction = AnimationDirection.Normal
);