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
    /// Plays the animation from the start. Stops the current one without finishing it.
    /// </summary>
    /// <param name="animation">The animation to play.</param>
    /// <param name="onFinished">Called once the animation is finished. Not called if the animation gets stopped.</param>
    public void Play(in Animation<T> animation, Action? onFinished = null) {
        Animation = animation;
        _onFinished = onFinished;
        _elapsedTime = 0f;
        Iteration = 0;

        // Applying right away so the target doesn't keep the previous values for a frame
        Evaluate();
    }

    /// <summary>
    /// Jumps the current animation to its end, applying the final values and invoking the finish callback.
    /// An infinite animation has no end, so it finishes the iteration being played instead.
    /// </summary>
    public void Finish() {
        if (Animation is not { } animation) {
            return;
        }

        if (float.IsPositiveInfinity(animation.Iterations) && animation.Duration > 0f) {
            Apply(animation, Iteration, 1f);
            Complete();
            return;
        }

        _elapsedTime = float.PositiveInfinity;
        Evaluate();
    }

    /// <summary>
    /// Stops the current animation, leaving the target as it is.
    /// </summary>
    public void Stop() {
        Animation = null;
        _onFinished = null;
    }

    #endregion

    #region Impl

    private Action? _onFinished;
    private float _elapsedTime;

    private void Evaluate() {
        var animation = Animation!.Value;
        var iterations = Mathf.Max(animation.Iterations, 0f);

        // Infinite iterations never finish unless there's nothing to play
        var finished = animation.Duration <= 0f || _elapsedTime >= animation.Duration * iterations;

        if (!finished) {
            var elapsedIterations = _elapsedTime / animation.Duration;
            var iteration = Mathf.FloorToInt(elapsedIterations);

            Apply(animation, iteration, elapsedIterations - iteration);
            return;
        }

        // The end of the last iteration, which is only partially played when the count is fractional
        if (iterations == 0f) {
            Apply(animation, 0, 0f);
        } else if (float.IsPositiveInfinity(iterations)) {
            Apply(animation, 0, 1f);
        } else {
            var last = Mathf.CeilToInt(iterations) - 1;
            Apply(animation, last, iterations - last);
        }

        Complete();
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

    private void Complete() {
        var onFinished = _onFinished;
        Stop();
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
