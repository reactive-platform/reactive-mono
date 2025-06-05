using System;
using B83.Image.GIF;
using UnityEngine;

namespace Reactive.Components;

public class CachedImage {
    #region Constructors

    public readonly Sprite Sprite;
    public readonly bool IsAnimated;

    private readonly GIFImage? _gifImage;
    private readonly RenderTexture? _renderTexture;
    private readonly Color32[]? _colors;
    private int _currentIndex;
    private float _deltaAccumulated;

    internal CachedImage(GIFImage gifImage) {
        IsAnimated = true;

        _renderTexture = new RenderTexture(gifImage.screen.width, gifImage.screen.height, 0, RenderTextureFormat.Default, 10);
        _renderTexture.Create();
        
        Sprite = ReactiveUtils.CreateSprite(_renderTexture)!;

        _colors = Sprite.texture.GetPixels32();
        _colors.Initialize();

        _gifImage = gifImage;
    }

    internal CachedImage(Sprite sprite) {
        IsAnimated = false;
        Sprite = sprite;
    }

    ~CachedImage() {
        _renderTexture?.Release();
    }

    #endregion

    #region Playback

    public void ManualUpdate(float timeDelta) {
        if (!IsAnimated || _gifImage!.imageData.Count == 0) {
            return;
        }

        var originalTexture = Sprite.texture;
        var frame = _gifImage.imageData[_currentIndex];

        if (_deltaAccumulated == 0) {
            frame.Dispose(_colors!, originalTexture.width, originalTexture.height);
            try {
                frame.DrawTo(_colors!, _renderTexture!.width, _renderTexture.height);
            } catch (Exception) {
                originalTexture.SetPixels32(_colors);
                originalTexture.Apply();
                Graphics.Blit(originalTexture, _renderTexture);
                return;
            }

            originalTexture.SetPixels32(_colors);
            originalTexture.Apply();
            Graphics.Blit(originalTexture, _renderTexture);
        }

        _deltaAccumulated += timeDelta;
        if (_deltaAccumulated >= frame.graphicControl.fdelay) {
            _deltaAccumulated = 0;
            if (_currentIndex < _gifImage.imageData.Count - 1) {
                _currentIndex++;
            } else {
                _currentIndex = 0;
            }
        }
    }

    #endregion
}