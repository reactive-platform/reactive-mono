using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public static class ModalCurves {
    public static readonly AnimationCurve JumpCurveX = new JumpCurveX();
    public static readonly AnimationCurve JumpCurveY = new JumpCurveY();
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