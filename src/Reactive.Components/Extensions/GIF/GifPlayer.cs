using System;
using B83.Image.GIF;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[PublicAPI]
public sealed class GifPlayer : IReactiveModule {
    #region Public API

    /// <summary>
    /// The sprite to show. Animated frames are rendered into the same sprite,
    /// so it has to be reapplied only when <see cref="Image"/> changes.
    /// </summary>
    public Sprite? Sprite { get; private set; }

    /// <summary>
    /// The image to show.
    /// </summary>
    public CachedImage? Image {
        get => _image;
        set {
            if (!value?.IsAnimated ?? false) {
                Debug.LogWarning("Setting static images into GifPlayer is not allowed, null will be passed");
                value = null;
            }

            if (value == _image) {
                return;
            }

            SetImage(value);
        }
    }

    /// <summary>
    /// Whether the playback is running. Kept when the image changes.
    /// </summary>
    public bool IsPlaying { get; set; } = true;

    /// <summary>
    /// Whether the first frame is shown. Setting true rewinds to it without changing <see cref="IsPlaying"/>,
    /// setting false does nothing.
    /// </summary>
    public bool IsFirstFrame {
        get => _currentIndex == 0;
        set {
            if (value) {
                Rewind();
            }
        }
    }

    /// <summary>
    /// Pauses the playback and rewinds to the first frame.
    /// </summary>
    public void Stop() {
        IsPlaying = false;
        Rewind();
    }

    /// <summary>
    /// Releases the current image. Must be called when the owner is destroyed
    /// as modules aren't unbound on destroy.
    /// </summary>
    public void ClearResources() {
        Image = null;
    }

    #endregion

    #region Impl

    private CachedImage? _image;
    private GIFImage? _gifImage;
    private Texture2D? _texture;
    private Sprite? _gifSprite;
    private Color32[]? _colors;
    private int _currentIndex;
    private float _deltaAccumulated;

    private void SetImage(CachedImage? image) {
        ReleaseGif();

        _image = image;

        if (image?.Gif is { } gifImage) {
            var width = gifImage.screen.width;
            var height = gifImage.screen.height;

            _texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            _gifSprite = SpriteUtils.CreateSprite(_texture)!;

            _colors = new Color32[width * height];

            _gifImage = gifImage;
        }

        Sprite = _gifSprite ?? image?.Sprite;
    }

    private void Rewind() {
        if (_colors != null) {
            Array.Clear(_colors, 0, _colors.Length);
        }

        _currentIndex = 0;
        _deltaAccumulated = 0;

        // Showing the frame right away as it won't be drawn while paused
        if (_gifImage is { imageData.Count: > 0 }) {
            DrawFrame(_gifImage.imageData[0]);
        }
    }

    private void ReleaseGif() {
        if (_texture != null) {
            UnityEngine.Object.Destroy(_gifSprite);
            UnityEngine.Object.Destroy(_texture);
        }

        _gifImage = null;
        _texture = null;
        _gifSprite = null;
        _colors = null;
        _currentIndex = 0;
        _deltaAccumulated = 0;
    }

    private void DrawFrame(IGIFRenderingBlock frame) {
        var texture = _texture!;

        frame.Dispose(_colors!, texture.width, texture.height);
        try {
            frame.DrawTo(_colors!, texture.width, texture.height);
        } catch (Exception) {
            // Showing what got drawn
        }

        texture.SetPixels32(_colors);
        texture.Apply(false);
    }

    void IReactiveModule.OnUpdate() {
        if (!IsPlaying || _gifImage == null || _gifImage.imageData.Count == 0) {
            return;
        }

        var frame = _gifImage.imageData[_currentIndex];

        if (_deltaAccumulated == 0) {
            DrawFrame(frame);
        }

        _deltaAccumulated += Time.deltaTime;
        if (_deltaAccumulated >= frame.graphicControl.fdelay) {
            _deltaAccumulated = 0;
            if (_currentIndex < _gifImage.imageData.Count - 1) {
                _currentIndex++;
            } else {
                _currentIndex = 0;
            }
        }
    }

    void IReactiveModule.OnBind() { }

    void IReactiveModule.OnUnbind() {
        ClearResources();
    }

    #endregion
}