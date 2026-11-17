using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// A value moving from one point to another.
/// </summary>
/// <typeparam name="T">A type of the value.</typeparam>
[PublicAPI]
public struct AnimatedValue<T> {
    public T From { get; private set; }
    public T To { get; private set; }
    public T Value { get; private set; }

    /// <summary>
    /// Whether the value is still on its way. False until <see cref="Start"/> is called.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// The progress with the curve applied, so it may go beyond 0..1.
    /// </summary>
    public float Progress { get; private set; }

    /// <summary>
    /// How the value moves towards the target. A zero duration by default, which ends the animation on its first step.
    /// </summary>
    public AnimationTiming Timing { get; set; }

    /// <summary>
    /// A curve applied to the progress. Linear if not set.
    /// </summary>
    public AnimationCurve? Curve { get; set; }

    /// <summary>
    /// Turns the progress into the value. Has to be set before the first step.
    /// </summary>
    public IValueInterpolator<T> Interpolator { get; set; }

    private float _linearProgress;
    private float _elapsedTime;

    /// <summary>
    /// Puts the value at the given point, ending the animation.
    /// </summary>
    public void Set(T value) {
        From = value;
        To = value;
        Value = value;

        Progress = 1f;
        _linearProgress = 1f;
        _elapsedTime = 0f;
        IsRunning = false;
    }

    /// <summary>
    /// Starts moving from one point to another, replacing the animation in progress.
    /// </summary>
    public void Start(T from, T to) {
        From = from;
        To = to;
        Value = from;

        Progress = 0f;
        _linearProgress = 0f;
        _elapsedTime = 0f;
        IsRunning = true;
    }

    /// <summary>
    /// Starts moving to the target from where the value is, unless it's heading for it or rests there already.
    /// </summary>
    /// <returns>True if a new animation was started, otherwise False.</returns>
    public bool Retarget(T to) {
        if (Interpolator.Equals(To, to)) {
            return false;
        }

        Start(Value, to);
        return true;
    }

    /// <summary>
    /// Moves the value by a frame. Does nothing when the animation isn't running.
    /// </summary>
    public void Advance(float deltaTime) {
        if (!IsRunning) {
            return;
        }

        _elapsedTime += deltaTime;
        _linearProgress = Timing.Advance(_linearProgress, _elapsedTime, deltaTime);

        Progress = Curve?.Evaluate(_linearProgress) ?? _linearProgress;
        Value = Interpolator.Lerp(From, To, Progress);

        // The clock decides when it's over, the curve may take the value past the target on the way
        IsRunning = _linearProgress < 1f;
    }
}
