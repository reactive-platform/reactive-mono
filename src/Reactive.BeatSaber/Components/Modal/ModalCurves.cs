using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public static class ModalCurves {
    public static readonly AnimationCurve JumpCurveX = new JumpCurveX();
    public static readonly AnimationCurve JumpCurveY = new JumpCurveY();
}

[PublicAPI]
public static class ModalAnimations {
    /// <summary>
    /// Stretches the modal out of a line, slightly overshooting horizontally.
    /// </summary>
    public static readonly Animation<IReactiveComponent> JumpOpen = new() {
        Track = new JumpTrack(false), 
        Duration = 0.15f
    };

    /// <summary>
    /// <see cref="JumpOpen"/> played backwards.
    /// </summary>
    public static readonly Animation<IReactiveComponent> JumpClose = new() {
        Track = new JumpTrack(true), 
        Duration = 0.12f
    };
}

internal class JumpTrack(bool reversed) : IAnimationTrack<IReactiveComponent> {
    public void Apply(IReactiveComponent target, float progress) {
        var t = reversed ? 1f - progress : progress;

        target.ContentTransform.localScale = new Vector3(
            ModalCurves.JumpCurveX.Evaluate(t),
            ModalCurves.JumpCurveY.Evaluate(t),
            1f
        );
    }
}

internal class JumpCurveX : AnimationCurve {
    public override float Evaluate(float t) {
        return t <= 0.3f ?
            Mathf.Lerp(0.85f, 1.065f, t / 0.3f) :
            Mathf.Lerp(1.065f, 1f, (t - 0.3f) / 0.7f);
    }
}

internal class JumpCurveY : AnimationCurve {
    public override float Evaluate(float t) {
        return t <= 0.47f ?
            Mathf.Lerp(0f, 0.95f, t / 0.47f) :
            Mathf.Lerp(0.95f, 1f, (t - 0.47f) / 0.53f);
    }
}