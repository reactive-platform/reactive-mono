using UnityEngine;

namespace Reactive.Components;

public static class ReactiveResources {
    static ReactiveResources() {
        var texture = new Texture2D(1, 1);
        TransparentPixel = ReactiveUtils.CreateSprite(texture)!;
    }

    public static readonly Sprite TransparentPixel;
}