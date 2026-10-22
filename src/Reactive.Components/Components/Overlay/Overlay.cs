using System;
using System.Collections.Generic;
using System.Configuration;
using JetBrains.Annotations;
using Reactive.Components;
using UnityEngine;

namespace Reactive.Components;

/// <summary>
/// A component wrapper that displays over the whole reactive composition.
/// Note that this component is bound to the composition once used, so in case you need to rebind it,
/// you have to pop and push it again.
/// For more info see <see cref="Composition"/>. 
/// </summary>
[PublicAPI]
public class Overlay : Layout, IOverlay {
    #region Public API

    /// <summary>
    /// An index defining an overlay position. Should be set before
    /// pushing an overlay, as after pushing it won't do anything.
    /// </summary>
    public int ZIndex { get; set; }

    /// <summary>
    /// Called when the overlay gets pushed to the composition.
    /// </summary>
    public Action? OnPushed { get; set; }

    /// <summary>
    /// Called when the overlay gets popped from the composition.
    /// </summary>
    public Action? OnPopped { get; set; }

    /// <summary>
    /// Determines whether the overlay is pushed or not.
    /// </summary>
    public bool IsPushed {
        get;
        set {
            if (value == field) {
                return;
            }

            if (value) {
                _composition ??= Composition.GetComposition(Content);

                // A pushed overlay belongs to the composition, not to the layout that declared it.
                // Leaving the driver removes it from the parent's layout entirely, so no flag is involved
                _previousDriver = LayoutDriver;
                _previousDriver?.Children.Remove(this);

                _composition!.PushOverlay(this);
                ContentTransform.WithRectExpand();
            } else {
                _composition!.PopOverlay(this);

                // Given back to the declaring layout, where it stays an inactive child until pushed again
                _previousDriver?.Children.Add(this);
                _previousDriver = null;
            }

            field = value;
        }
    }

    private Composition? _composition;
    private ILayoutDriver? _previousDriver;

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