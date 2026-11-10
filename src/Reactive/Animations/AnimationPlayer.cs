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

    public bool IsPlaying => Animation != null;

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
        _onFinished = onFinished;
        _elapsedTime = 0f;
        Iteration = 0;

        // Applying right away so the target doesn't keep the previous values for a frame
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
    }

    /// <summary>
    /// Cancels the current animation, leaving the target as it is. The finish callback never runs.
    /// </summary>
    public void Cancel() {
        Animation = null;
        _onFinished = null;
    }

    #endregion

    #region Impl

    private Action? _onFinished;
    private float _elapsedTime;

    private void Evaluate() {
        var animation = Animation!.Value;
        var duration = animation.Duration.Seconds;
        var iterations = animation.Iterations;

        // Infinite iterations never finish unless there's nothing to play
        var finished = duration <= 0f || _elapsedTime >= duration * iterations.Count;

        if (!finished) {
            var elapsedIterations = _elapsedTime / duration;
            var iteration = Mathf.FloorToInt(elapsedIterations);

            Apply(animation, iteration, elapsedIterations - iteration);
            return;
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

        End();
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
        if (Animation == null) {
            return;
        }

        _elapsedTime += Time.deltaTime;
        Evaluate();
    }

    void IReactiveModule.OnBind() { }
    void IReactiveModule.OnUnbind() { }

    #endregion
}
