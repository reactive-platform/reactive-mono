using System;
using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// Defines how many times an animation is played.
/// </summary>
[PublicAPI]
public readonly record struct AnimationIteration {
    private AnimationIteration(float count) {
        // Stored without the first iteration, so the default value means once
        _extra = count - 1f;
    }

    private readonly float _extra;

    /// <summary>
    /// The number of iterations. Can be fractional, then the last iteration stops partway through.
    /// Positive infinity when <see cref="IsInfinite"/>.
    /// </summary>
    public float Count => _extra + 1f;

    /// <summary>
    /// Whether the animation repeats until it gets stopped.
    /// </summary>
    public bool IsInfinite => float.IsPositiveInfinity(_extra);

    public static readonly AnimationIteration Once = default;
    public static readonly AnimationIteration Infinite = new(float.PositiveInfinity);
    
    public static AnimationIteration Times(float count) {
        if (float.IsNaN(count) || count < 0f) {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Iterations can't be negative or NaN");
        }

        return new(count);
    }

    public static implicit operator AnimationIteration(float count) {
        return Times(count);
    }
}
