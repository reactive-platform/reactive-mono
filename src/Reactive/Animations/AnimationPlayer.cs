using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

/// <summary>
/// Plays <see cref="Animation{T}"/> on a component, one at a time.
/// </summary>
/// <typeparam name="T">A type of the animated component.</typeparam>
[PublicAPI]
public sealed class AnimationPlayer<T> : IReactiveModule {
    public AnimationPlayer(T target) {
        Target = target;
    }

    #region Public API

    public T Target { get; }

    /// <summary>
    /// The animation being played.
    /// </summary>
    public Animation<T>? Animation { get; private set; }

    /// <summary>
    /// Whether the animation advances. False when nothing is set or the animation is paused.
    /// </summary>
    public bool IsPlaying { get; private set; }

    /// <summary>
    /// The time since the animation started, in seconds.
    /// </summary>
    public float Time => _elapsedTime;

    /// <summary>
    /// The zero-based iteration being played.
    /// </summary>
    public int Iteration { get; private set; }

    /// <summary>
    /// Plays the animation from the start. Cancels the current one without completing it.
    /// </summary>
    /// <param name="animation">The animation to play.</param>
    /// <param name="onFinished">Called once the animation is finished. Not called if the animation gets cancelled.</param>
    public void Play(in Animation<T> animation, Action? onFinished = null) {
        Animation = animation;
        IsPlaying = true;
        _onFinished = onFinished;
        _elapsedTime = 0f;
        Iteration = 0;

        // Applying right away so the target doesn't keep the previous values for a frame
        if (Evaluate()) {
            End();
        }
    }

    /// <summary>
    /// Pauses the current animation, leaving it on the target so it can be resumed or seeked.
    /// </summary>
    public void Pause() {
        IsPlaying = false;
    }

    /// <summary>
    /// Resumes the current animation from where it was paused. Does nothing when no animation is set.
    /// </summary>
    public void Resume() {
        IsPlaying = Animation != null;
    }

    /// <summary>
    /// Jumps the animation to the given time in seconds.
    /// </summary>
    public void Seek(float time) {
        if (Animation is not { } animation) {
            return;
        }

        var length = animation.Duration.Seconds * animation.Iterations.Count;
        _elapsedTime = Mathf.Clamp(time, 0f, length);

        Evaluate();
    }

    /// <summary>
    /// Jumps to the end and invokes the finish callback. An infinite animation completes its current iteration.
    /// </summary>
    public void Complete() {
        if (Animation is not { } animation) {
            return;
        }

        if (animation.Iterations.IsInfinite && animation.Duration.Seconds > 0f) {
            Apply(animation, Iteration, 1f);
            End();
            return;
        }

        _elapsedTime = float.PositiveInfinity;
        Evaluate();
        End();
    }

    /// <summary>
    /// Cancels the current animation, leaving the target as it is. The finish callback never runs.
    /// </summary>
    public void Cancel() {
        Animation = null;
        IsPlaying = false;
        _onFinished = null;
    }

    #endregion

    #region Impl

    private Action? _onFinished;
    private float _elapsedTime;

    /// <summary>
    /// Applies the animation at the elapsed time.
    /// </summary>
    /// <returns>Whether the animation has reached its end.</returns>
    private bool Evaluate() {
        var animation = Animation!.Value;
        var duration = animation.Duration.Seconds;
        var iterations = animation.Iterations;

        // Infinite iterations never finish unless there's nothing to play
        var finished = duration <= 0f || _elapsedTime >= duration * iterations.Count;

        if (!finished) {
            var elapsedIterations = _elapsedTime / duration;
            var iteration = Mathf.FloorToInt(elapsedIterations);

            Apply(animation, iteration, elapsedIterations - iteration);
            return false;
        }

        // The end of the last iteration, which is only partially played when the count is fractional
        if (iterations.Count == 0f) {
            Apply(animation, 0, 0f);
        } else if (iterations.IsInfinite) {
            Apply(animation, 0, 1f);
        } else {
            var last = Mathf.CeilToInt(iterations.Count) - 1;
            Apply(animation, last, iterations.Count - last);
        }

        return true;
    }

    private void Apply(in Animation<T> animation, int iteration, float progress) {
        Iteration = iteration;

        var reversed = animation.Direction switch {
            AnimationDirection.Reverse => true,
            AnimationDirection.Alternate => iteration % 2 == 1,
            AnimationDirection.AlternateReverse => iteration % 2 == 0,
            _ => false
        };

        // The curve goes over the directed progress, so a reversed ease-in plays like an ease-out, as on the web
        if (reversed) {
            progress = 1f - progress;
        }

        animation.Track.Apply(Target, animation.Curve?.Evaluate(progress) ?? progress);
    }

    private void End() {
        var onFinished = _onFinished;
        Cancel();
        onFinished?.Invoke();
    }

    void IReactiveModule.OnUpdate() {
        if (Animation == null || !IsPlaying) {
            return;
        }

        _elapsedTime += UnityEngine.Time.deltaTime;

        if (Evaluate()) {
            End();
        }
    }

    void IReactiveModule.OnBind() { }
    void IReactiveModule.OnUnbind() { }

    #endregion
}
