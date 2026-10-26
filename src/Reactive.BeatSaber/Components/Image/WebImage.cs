using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public readonly record struct WebImageAnimContext(
    WebImage Image,
    Background Background,
    CanvasGroup BackgroundGroup,
    Spinner Spinner
);

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

    /// <summary>
    /// Played when an image starts loading. Fades the background in by default.
    /// </summary>
    public Animation<WebImageAnimContext>? StartAnimation { get; set; } = DefaultStartAnimation;

    /// <summary>
    /// Played when an image finishes loading. Fades the background out by default.
    /// </summary>
    public Animation<WebImageAnimContext>? FinishAnimation { get; set; } = DefaultFinishAnimation;

    /// <summary>
    /// Shown instead of the image when loading fails. Nothing is shown by default.
    /// </summary>
    public Sprite? FallbackSprite { get; set; }

    /// <summary>
    /// Called when loading fails, after the fallback is shown.
    /// </summary>
    public Action? OnLoadFailed { get; set; }

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

        if (string.IsNullOrWhiteSpace(src)) {
            ShowImage(null);
            SetOverlayShown(false);
            return;
        }

        var tokenSource = _tokenSource = new();
        var task = ImageLoader.LoadImage(src, tokenSource.Token);

        // Cached images complete right away, showing the overlay only when there's something to wait for
        if (!task.IsCompleted) {
            SetOverlayShown(true);
        }

        CachedImage? image;
        var failure = false;

        try {
            image = await task;
        } catch (OperationCanceledException) {
            return;
        } catch (Exception ex) {
            image = null;
            failure = true;
        }

        if (tokenSource.IsCancellationRequested || IsDestroyed) {
            return;
        }

        _tokenSource = null;

        ShowImage(image, failure ? FallbackSprite : null);
        SetOverlayShown(false);

        if (failure) {
            Debug.LogError("Image loading has failed");
            OnLoadFailed?.Invoke();
        }
    }

    private void ShowImage(CachedImage? image, Sprite? fallback = null) {
        // The player accepts animated images only, static ones are shown as they are
        if (image is { IsAnimated: true }) {
            _player.Image = image;
            Sprite = _player.Sprite;
        } else {
            _player.ClearResources();
            Sprite = image?.Sprite ?? fallback ?? BeatSaberResources.Sprites.transparentPixel;
        }
    }

    #endregion

    #region Animation Defaults

    public static readonly Animation<WebImageAnimContext> DefaultStartAnimation = new(
        AnimationTracks.Fade(0f, 1f).For((WebImageAnimContext x) => x.Background),
        0.2f,
        AnimationCurve.EaseInOut
    );

    public static readonly Animation<WebImageAnimContext> DefaultFinishAnimation = new(
        AnimationTracks.Fade(1f, 0f).For((WebImageAnimContext x) => x.Background),
        0.2f,
        AnimationCurve.EaseInOut
    );

    #endregion

    #region Construct

    private AnimationPlayer<WebImageAnimContext> _overlayPlayer = null!;
    private GifPlayer _player = null!;
    private bool _overlayShown;

    protected override void Construct(RectTransform rect) {
        base.Construct(rect);

        _player = this.RememberGifPlayer();

        new Background {
                Children = {
                    new Spinner()
                        .Export(out var spinner)
                        .AsFlexItem(size: 80.pct)
                }
            }
            .WithNativeComponent(out CanvasGroup group)
            .AsBackground(color: Color.black.ColorWithAlpha(0.9f))
            .Export(out var background)
            .AsFlexGroup(justifyContent: Justify.Center, alignItems: Align.Center)
            .WithRectExpand()
            .Use(rect);

        group.alpha = 0f;
        _overlayPlayer = RememberAnimationPlayer(new WebImageAnimContext(this, background, group, spinner));
    }

    private void SetOverlayShown(bool show) {
        if (show == _overlayShown) {
            return;
        }

        _overlayShown = show;

        if ((show ? StartAnimation : FinishAnimation) is { } anim) {
            _overlayPlayer.Play(anim);
        } else {
            _overlayPlayer.Stop();
            _overlayPlayer.Target.BackgroundGroup.alpha = show ? 1f : 0f;
        }
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