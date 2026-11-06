using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

/// <summary>
/// Defines how a value moves towards its target. Can be either a fixed duration or a delta factor.
/// </summary>
[PublicAPI]
public readonly struct AnimationTiming {
    private AnimationTiming(float value, TimingUnit unit) {
        Value = value;
        Unit = unit;
    }
    
    public float Value { get; }
    public TimingUnit Unit { get; }

    public static AnimationTiming Fact(float factor) {
        return new(factor, TimingUnit.TimeDeltaFactor);
    }

    public static AnimationTiming Time(AnimationDuration duration) {
        return new(duration.Seconds, TimingUnit.Time);
    }

    /// <summary>
    /// Advances linear progress from 0 to 1 by a frame.
    /// </summary>
    public float Advance(float progress, float elapsedTime, float deltaTime) {
        if (Unit is TimingUnit.Time) {
            return Value > 0f ? Mathf.Clamp01(elapsedTime / Value) : 1f;
        }

        // Exponential, so the same factor moves the same way at any frame rate
        progress = Mathf.Lerp(progress, 1f, 1f - Mathf.Exp(-Value * deltaTime));

        // Only approaches 1, so it's snapped at the end
        return Mathf.Approximately(progress, 1f) ? 1f : progress;
    }

    public static implicit operator AnimationTiming(AnimationDuration duration) {
        return new(duration.Seconds, TimingUnit.Time);
    }
}
