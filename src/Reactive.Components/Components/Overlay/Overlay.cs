using System;
using System.Collections.Generic;
using System.Configuration;
using JetBrains.Annotations;
using Reactive.Components;
using UnityEngine;

namespace Reactive.Components;

/// <summary>
/// A component wrapper that displays over the whole reactive composition.
/// The composition is resolved from the hierarchy on every push.
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
    /// Defines how the overlay affects the composition. See <see cref="CompositionAnimParams"/>.
    /// Should be set before pushing an overlay.
    /// </summary>
    public CompositionAnimParams CompositionAnimation { get; set; } = new();

    /// <summary>
    /// Determines whether the composition transitions caused by the overlay are animated.
    /// Checked when a transition starts, so changing it doesn't affect a running one.
    /// </summary>
    public bool Animated { get; set; } = true;

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
            
            field = value;

            if (value) {
                _composition = Composition.GetComposition(Content);
                _previousParent = ContentTransform.parent;
                
                _previousDriver = LayoutDriver;
                _previousDriver?.Children.Remove(this);

                _composition.PushOverlay(this);
                ContentTransform.WithRectExpand();
            } else {
                var driver = _previousDriver;

                _previousDriver = null;
                _composition!.PopOverlay(this);
                _composition = null;
                _previousParent = null;

                driver?.Children.Add(this);
            }
        }
    }

    private Composition? _composition;
    private ILayoutDriver? _previousDriver;
    private Transform? _previousParent;

    #endregion

    #region Overlay Impl

    Transform IOverlay.ContentTransform => base.ContentTransform;
    Transform? IOverlay.SetBackParent => _previousParent;

    void IOverlay.OnPush() {
        OnPushed?.Invoke();
    }

    void IOverlay.OnPop() {
        OnPopped?.Invoke();
    }

    #endregion
}