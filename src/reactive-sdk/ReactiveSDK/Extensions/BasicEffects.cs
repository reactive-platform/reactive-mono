using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public static class BasicEffects {
    public static T WithScaleAnimation<T>(
        this T comp,
        Vector3 baseScale,
        Vector3 hoverScale,
        Optional<AnimationDuration> duration = default
    ) where T : ButtonBase {
        var value = ValueUtils.RememberAnimatedVector(
            comp,
            baseScale,
            duration.GetValueOrDefault("200ms")
        );
        comp.WithListener(
            static x => x.IsHovered,
            x => value.Value = x ? hoverScale : baseScale
        );
        return comp;
    }

    public static T WithScaleAnimation<T>(
        this T comp,
        float baseScale,
        float hoverScale,
        Optional<AnimationDuration> duration = default
    ) where T : ButtonBase {
        return WithScaleAnimation(
            comp,
            Vector3.one * baseScale,
            Vector3.one * hoverScale,
            duration
        );
    }
}