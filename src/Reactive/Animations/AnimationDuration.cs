using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A fixed length of an animation.
/// </summary>
[PublicAPI]
public readonly record struct AnimationDuration(float Seconds) {
    public int Milliseconds { get; } = (int)(Seconds * 1000);

    public static AnimationDuration Sec(float seconds) {
        return new(seconds);
    }

    public static AnimationDuration Ms(int millis) {
        return new(millis / 1000f);
    }
}