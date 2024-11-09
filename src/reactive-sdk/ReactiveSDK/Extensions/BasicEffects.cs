using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public static class BasicEffects {
    public static T WithScaleAnimation<T>(
        this T comp,
        Vector3 baseScale,
        Vector3 hoverScale,
        Optional<AnimationDuration> duration = default,
        AnimationCurve? curve = null
    ) where T : ButtonBase {
        var value = ValueUtils.RememberAnimatedVector(
            comp,
            baseScale,
            duration.GetValueOrDefault(10.fact()),
            curve
        );
        comp.WithListener(
            static x => x.IsHovered,
            x => value.Value = x ? hoverScale : baseScale
        );
        comp.WithEffect(
            value,
            static (x, y) => x.ContentTransform.localScale = y
        );
        return comp;
    }

    public static T WithScaleAnimation<T>(
        this T comp,
        float baseScale,
        float hoverScale,
        Optional<AnimationDuration> duration = default,
        AnimationCurve? curve = null
    ) where T : ButtonBase {
        return WithScaleAnimation(
            comp,
            Vector3.one * baseScale,
            Vector3.one * hoverScale,
            duration,
            curve
        );
    }
}