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
    /// Plays the animation from the start. Stops the current one without finishing it.
    /// </summary>
    /// <param name="animation">The animation to play.</param>
    /// <param name="onFinished">Called once the animation is finished. Not called if the animation gets stopped.</param>
    public void Play(in Animation<T> animation, Action? onFinished = null) {
        Animation = animation;
        _onFinished = onFinished;
        _elapsedTime = 0f;

        // Applying right away so the target doesn't keep the previous values for a frame
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
        var progress = animation.Duration > 0f ? Mathf.Clamp01(_elapsedTime / animation.Duration) : 1f;

        animation.Track.Apply(Target, animation.Curve?.Evaluate(progress) ?? progress);

        if (progress < 1f) {
            return;
        }

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
