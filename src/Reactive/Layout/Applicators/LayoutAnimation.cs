using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// Defines how a value of <see cref="LayoutDimensions"/> is animated.
/// </summary>
[PublicAPI]
public readonly record struct LayoutAnimation(AnimationTiming Timing, AnimationCurve? Curve = null) {
    public static implicit operator LayoutAnimation(AnimationTiming timing) {
        return new(timing);
    }

    public static implicit operator LayoutAnimation(AnimationDuration duration) {
        return new(duration);
    }
}
