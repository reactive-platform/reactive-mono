using System;
using JetBrains.Annotations;
using Reactive.Components;
using UnityEngine;

namespace Reactive.Components;

/// <summary>
/// A component wrapper that displays over the whole reactive composition.
/// Note that this component is bound to the composition once used, so in case you need to rebind it,
/// you have to move pop it, move to another composition and call <see cref="Overlay.RefreshComposition"/>.
/// For more info see <see cref="Composition"/>. 
/// </summary>
[PublicAPI]
public class Overlay : ComponentLayout<Overlay>, IOverlay {
    #region Public API

    /// <summary>
    /// An index defining an overlay position. Should be set before
    /// pushing an overlay, as after pushing it won't do anything.
    /// </summary>
    public int ZIndex { get; set; }

    /// <summary>
    /// Defines if the overlay is pushed or not.
    /// </summary>
    public bool IsPushed { get; private set; }
    
    /// <summary>
    /// Called when the overlay gets pushed to the composition.
    /// </summary>
    public Action? OnPushed { get; set; }
    
    /// <summary>
    /// Called when the overlay gets popped from the composition.
    /// </summary>
    public Action? OnPopped { get; set; }

    private Composition? _composition;

    public bool Push() {
        if (IsPushed) {
            return false;
        }

        RefreshComposition();
        _composition!.PushOverlay(this);

        IsPushed = true;
        return true;
    }

    public bool Pop() {
        if (!IsPushed) {
            return false;
        }

        _composition!.PopOverlay(this);
        IsPushed = false;

        return true;
    }

    /// <summary>
    /// Attempts to acquire a new composition instance.
    /// </summary>
    public void RefreshComposition() {
        if (IsPushed) {
            throw new InvalidOperationException("Cannot change composition when presented");
        }

        _composition = Composition.GetComposition(Content);
    }

    #endregion

    #region Overlay Impl

    Transform IOverlay.ContentTransform => base.ContentTransform;
    Transform? IOverlay.SetBackParent => null;

    void IOverlay.OnPush() {
        OnPushed?.Invoke();
    }

    void IOverlay.OnPop() {
        OnPopped?.Invoke();
    }

    #endregion
}