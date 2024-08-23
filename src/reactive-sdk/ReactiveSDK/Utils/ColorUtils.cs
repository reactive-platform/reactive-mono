using UnityEngine;

namespace Reactive.Components;

public static class ColorUtils {
    public static Color ColorWithAlpha(this Color color, float alpha) {
        color.a = alpha;
        return color;
    }
}