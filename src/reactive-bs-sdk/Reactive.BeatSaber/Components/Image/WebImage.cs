using JetBrains.Annotations;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public class WebImage : Image {
    #region Public API

    public string? Src {
        get => _src;
        set {
            if (value != _src) {
                _src = value;
                UpdateImage();
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

    private string? _src;

    private void UpdateImage() {
        if (_src != null) {
            this.WithWebSource(
                _src,
                () => {
                    _spinnerAlpha.Value = 1f;
                },
                _ => {
                    _spinnerAlpha.Value = 0f;
                }
            );
        } else {
            Sprite = null;
            _spinnerAlpha.Value = 0f;
        }
    }

    #endregion

    #region Construct

    private AnimatedState<float> _spinnerAlpha = null!;
    private State<float> _backgroundAlpha = null!;
    private Spinner _spinner = null!;

    protected override void Construct(RectTransform rect) {
        base.Construct(rect);

        _spinnerAlpha = RememberAnimated(0f, 200.ms(), AnimationCurve.EaseInOut);
        _backgroundAlpha = Remember(0.9f);

        new Background {
                Children = {
                    new Spinner()
                        .Bind(ref _spinner)
                        .AsFlexItem(size: 80.pct())
                }
            }
            .WithNativeComponent(out CanvasGroup group)
            .AsBackground()
            .On(_backgroundAlpha, (x, y) => x.Color = Color.black.ColorWithAlpha(y), applyImmediately: true)
            .On(_spinnerAlpha, (_, y) => group.alpha = y)
            .AsFlexGroup(justifyContent: Justify.Center, alignItems: Align.Center)
            .WithRectExpand()
            .Use(rect);
    }

    protected override void OnInitialize() {
        Sprite = BeatSaberResources.Sprites.transparentPixel;
    }

    #endregion
}