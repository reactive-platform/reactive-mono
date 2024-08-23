using UnityEngine;

namespace Reactive.Components;

internal static class MathUtils {
    public static float RoundStepped(float value, float step, float startValue = 0) {
        if (step is 0) return value;
        var relativeValue = value - startValue;
        return startValue + Mathf.Round(relativeValue / step) * step;
    }
}