using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

[PublicAPI]
public static class AnimationTracks {
    public static IAnimationTrack<IReactiveComponent> Fade(float from, float to) {
        return new FadeTrack(from, to);
    }
    
    public static IAnimationTrack<IReactiveComponent> Scale(Vector3 from, Vector3 to) {
        return new ScaleTrack(from, to);
    }
    
    public static IAnimationTrack<IReactiveComponent> Scale(float from, float to) {
        return Scale(new Vector3(from, from, 1f), new Vector3(to, to, 1f));
    }

    /// <summary>
    /// Creates a track from a delegate.
    /// </summary>
    /// <param name="apply">Applies the track to the target with the given progress.</param>
    public static IAnimationTrack<T> Create<T>(Action<T, float> apply) {
        return new DelegateTrack<T>(apply);
    }

    extension<T>(IAnimationTrack<T>) {
        public static IAnimationTrack<T> operator +(IAnimationTrack<T> left, IAnimationTrack<T> right) {
            return new CombinedTrack<T>(left, right);
        }
    }

    extension<T>(IAnimationTrack<T> track) {
        /// <summary>
        /// Creates a track that applies this track to a value selected from another target.
        /// </summary>
        /// <param name="selector">Selects the value to apply the track to.</param>
        public IAnimationTrack<TTarget> For<TTarget>(Func<TTarget, T> selector) {
            return new SelectorTrack<TTarget, T>(track, selector);
        }
    }
}

internal sealed class FadeTrack(float from, float to) : IAnimationTrack<IReactiveComponent> {
    public void Apply(IReactiveComponent target, float progress) {
        var group = target.Content.GetOrAddComponent<CanvasGroup>();
        group.alpha = Mathf.LerpUnclamped(from, to, progress);
    }
}

internal sealed class ScaleTrack(Vector3 from, Vector3 to) : IAnimationTrack<IReactiveComponent> {
    public void Apply(IReactiveComponent target, float progress) {
        target.ContentTransform.localScale = Vector3.LerpUnclamped(from, to, progress);
    }
}

internal sealed class CombinedTrack<T>(IAnimationTrack<T> left, IAnimationTrack<T> right) : IAnimationTrack<T> {
    public void Apply(T target, float progress) {
        left.Apply(target, progress);
        right.Apply(target, progress);
    }
}

internal sealed class SelectorTrack<TTarget, T>(IAnimationTrack<T> track, Func<TTarget, T> selector) : IAnimationTrack<TTarget> {
    public void Apply(TTarget target, float progress) {
        track.Apply(selector(target), progress);
    }
}

internal sealed class DelegateTrack<T>(Action<T, float> apply) : IAnimationTrack<T> {
    public void Apply(T target, float progress) {
        apply(target, progress);
    }
}
