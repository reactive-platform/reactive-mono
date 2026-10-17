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

    extension<T>(IAnimationTrack<T>) where T : IReactiveComponent {
        public static IAnimationTrack<T> operator +(IAnimationTrack<T> left, IAnimationTrack<T> right) {
            return new CombinedTrack<T>(left, right);
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

internal sealed class CombinedTrack<T>(IAnimationTrack<T> left, IAnimationTrack<T> right) : IAnimationTrack<T> where T : IReactiveComponent {
    public void Apply(T target, float progress) {
        left.Apply(target, progress);
        right.Apply(target, progress);
    }
}
