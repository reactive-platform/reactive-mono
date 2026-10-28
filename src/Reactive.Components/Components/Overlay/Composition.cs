using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reactive.Components;
using UnityEngine;

namespace Reactive;

/// <summary>
/// Determines a 'root' of the composition. Used by components like <see cref="Overlay"/>.
/// </summary>
[PublicAPI]
public class Composition : MonoBehaviour {
    #region Overlays

    private readonly List<IOverlay> _overlays = new();
    private int _startingSiblingIndex;

    public void PushOverlay(IOverlay overlay) {
        if (_overlays.Count == 0) {
            SetDimmed(overlay.CompositionAnimation, overlay.Animated);
        }

        var insertIndex = InsertOverlay(overlay);
        var trans = overlay.ContentTransform;

        trans.SetParent(transform, false);

        if (_overlays.Count == 1) {
            trans.SetAsLastSibling();
            _startingSiblingIndex = trans.GetSiblingIndex();
        } else {
            trans.SetSiblingIndex(_startingSiblingIndex + insertIndex);
        }

        trans.gameObject.GetOrAddComponent<CanvasGroup>().ignoreParentGroups = _dimmed;
        trans.gameObject.SetActive(true);

        overlay.OnPush();
    }

    public void PopOverlay(IOverlay overlay) {
        if (!_overlays.Remove(overlay)) {
            return;
        }

        var trans = overlay.ContentTransform;
        var setBackTrans = overlay.SetBackParent;

        trans.GetComponent<CanvasGroup>().ignoreParentGroups = false;
        trans.SetParent(setBackTrans, false);
        trans.gameObject.SetActive(false);

        if (_overlays.Count == 0) {
            SetDimmed(_params with { DimCanvas = false }, overlay.Animated);
        }

        overlay.OnPop();
    }

    private int InsertOverlay(IOverlay overlay) {
        var zIndex = overlay.ZIndex;

        for (var i = _overlays.Count - 1; i >= 0; i--) {
            if (zIndex >= _overlays[i].ZIndex) {
                _overlays.Insert(i + 1, overlay);
                return i + 1;
            }
        }

        _overlays.Insert(0, overlay);
        return 0;
    }

    #endregion

    #region Dimming

    private CanvasGroup? _canvasGroup;
    private CompositionAnimParams _params;
    private bool _dimmed;
    private float _startAlpha;
    private float _elapsedTime;

    private float TargetAlpha => _dimmed ? _params.DimmedAlpha : 1f;

    private void SetDimmed(CompositionAnimParams animParams, bool animated) {
        _canvasGroup ??= gameObject.GetOrAddComponent<CanvasGroup>();

        // Restoring uses the params of the overlay that dimmed the composition
        _params = animParams;
        _dimmed = animParams.DimCanvas;
        _startAlpha = _canvasGroup.alpha;
        _elapsedTime = 0f;

        if (animated) {
            enabled = true;
        } else {
            _canvasGroup!.alpha = TargetAlpha;
            enabled = false;
        }
    }

    private void Update() {
        _elapsedTime += Time.deltaTime;

        var progress = _params.Duration > 0f ? Mathf.Clamp01(_elapsedTime / _params.Duration) : 1f;
        var curved = _params.Curve?.Evaluate(progress) ?? progress;

        _canvasGroup!.alpha = Mathf.LerpUnclamped(_startAlpha, TargetAlpha, curved);

        if (progress >= 1f) {
            enabled = false;
        }
    }

    private void Awake() {
        enabled = false;
    }

    #endregion

    public static Composition GetComposition(GameObject go) {
        return go.GetComponentInParent<Composition>() ?? throw new Exception("The object is not in the composition stack");
    }
}