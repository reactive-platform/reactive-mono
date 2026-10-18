using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class WebImage : Image {
    #region Public API

    public string? Src {
        get;
        set {
            if (value != field) {
                field = value;
                _ = LoadImage(value);
            }
        }
    }

    public AnimationDuration FadeDuration {
        get => _spinnerAlpha.Duration;
        set => _spinnerAlpha.Duration = value;
    }

    public AnimationCurve FadeCurve {
        get => _spinnerAlpha.Curve;
        set => _spinnerAlpha.Curve = value;
    }

    public float FadeAmount {
        get => _backgroundAlpha;
        set => _backgroundAlpha.Value = value;
    }

    public Spinner Spinner => _spinner;

    /// <summary>
    /// Whether an animated image is playing. Kept when the image changes.
    /// </summary>
    public bool IsPlaying {
        get => _player.IsPlaying;
        set => _player.IsPlaying = value;
    }

    /// <summary>
    /// Whether an animated image shows its first frame. Setting true rewinds to it, setting false does nothing.
    /// </summary>
    public bool IsFirstFrame {
        get => _player.IsFirstFrame;
        set => _player.IsFirstFrame = value;
    }

    private CancellationTokenSource? _tokenSource;

    private async Task LoadImage(string? src) {
        _tokenSource?.Cancel();
        _tokenSource = null;

        if (src == null) {
            ShowImage(null);
            _spinnerAlpha.TargetValue = 0f;
            return;
        }

        var tokenSource = _tokenSource = new();
        _spinnerAlpha.TargetValue = 1f;

        CachedImage? image;

        try {
            image = await ImageLoader.LoadImage(src, tokenSource.Token);
        } catch (OperationCanceledException) {
            return;
        } catch (Exception ex) {
            Debug.LogError($"Image loading has failed: {ex}");
            image = null;
        }

        if (tokenSource.IsCancellationRequested || IsDestroyed) {
            return;
        }

        _tokenSource = null;

        ShowImage(image);
        _spinnerAlpha.TargetValue = 0f;
    }

    private void ShowImage(CachedImage? image) {
        // The player accepts animated images only, static ones are shown as they are
        if (image is { IsAnimated: true }) {
            _player.Image = image;
            Sprite = _player.Sprite;
        } else {
            _player.ClearResources();
            Sprite = image?.Sprite ?? BeatSaberResources.Sprites.transparentPixel;
        }
    }

    #endregion

    #region Construct

    private AnimatedState<float> _spinnerAlpha = null!;
    private State<float> _backgroundAlpha = null!;
    private Spinner _spinner = null!;
    private GifPlayer _player = null!;

    protected override void Construct(RectTransform rect) {
        base.Construct(rect);

        _spinnerAlpha = RememberAnimated(0f, 200.ms, AnimationCurve.EaseInOut);
        _backgroundAlpha = Remember(0.9f);
        _player = this.RememberGifPlayer();

        new Background {
                Children = {
                    new Spinner()
                        .Bind(ref _spinner)
                        .AsFlexItem(size: 80.pct)
                }
            }
            .WithNativeComponent(out CanvasGroup group)
            .AsBackground()
            .On(_backgroundAlpha, (x, y) => x.Color = Color.black.ColorWithAlpha(y))
            .On(_spinnerAlpha, (_, y) => group.alpha = y)
            .AsFlexGroup(justifyContent: Justify.Center, alignItems: Align.Center)
            .WithRectExpand()
            .Use(rect);
    }

    protected override void OnInitialize() {
        Sprite = BeatSaberResources.Sprites.transparentPixel;
    }

    protected override void OnDestroy() {
        _tokenSource?.Cancel();
        _player.ClearResources();
    }

    #endregion
}
