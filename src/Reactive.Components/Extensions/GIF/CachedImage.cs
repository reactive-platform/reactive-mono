using B83.Image.GIF;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

// TODO: make this a closed hierarchy after C# 15
[PublicAPI]
public class CachedImage {
    #region Constructors

    internal CachedImage(GIFImage gifImage) {
        Gif = gifImage;
    }

    internal CachedImage(Sprite sprite) {
        Sprite = sprite;
    }

    #endregion

    #region Public API

    /// <summary>
    /// The image itself. Null if the image is animated, use <see cref="GifPlayer"/> to show it.
    /// </summary>
    public Sprite? Sprite { get; }
    internal GIFImage? Gif { get; }

    public bool IsAnimated => Gif != null;

    #endregion
}
