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
    private readonly List<IOverlay> _overlays = new();

    public void PushOverlay(IOverlay overlay) {
        var insertIndex = InsertOverlay(overlay);
        var trans = overlay.ContentTransform;
        
        trans.SetParent(transform, false);
        trans.SetSiblingIndex(insertIndex);
        trans.gameObject.SetActive(true);
        
        overlay.OnPush();
    }

    public void PopOverlay(IOverlay overlay) {
        if (!_overlays.Remove(overlay)) {
            return;
        }
        
        var trans = overlay.ContentTransform;
        var setBackTrans = overlay.SetBackParent;
        
        trans.SetParent(setBackTrans, false);
        trans.gameObject.SetActive(false); 
        
        overlay.OnPop();
    }

    private int InsertOverlay(IOverlay overlay) {
        var zIndex = overlay.ZIndex;
        
        for (var i = _overlays.Count - 1; i >= 0; i--) {
            if (zIndex >= _overlays[i].ZIndex) {
                _overlays.Insert(i + 1, overlay);
                return i;
            }
        }
        
        _overlays.Insert(0, overlay);
        return 0;
    }

    public static Composition GetComposition(GameObject go) {
        return go.GetComponentInParent<Composition>() ?? throw new Exception("The object is not in the composition stack");
    }
}