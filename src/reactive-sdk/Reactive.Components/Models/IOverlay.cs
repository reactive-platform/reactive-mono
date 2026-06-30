using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

/// <summary>
/// An object that displays over the whole reactive composition.
/// Use this if you need a custom implementation. Otherwise, <see cref="Overlay"/> is recommended.
/// </summary>
[PublicAPI]
public interface IOverlay {
    /// <summary>
    /// An index defining an overlay position. Should be set before
    /// pushing the overlay, as after pushing it won't do anything.
    /// </summary>
    int ZIndex { get; }

    /// <summary>
    /// A transform to push.
    /// </summary>
    Transform ContentTransform { get; }
    
    /// <summary>
    /// A transform to reparent back to.
    /// </summary>
    Transform? SetBackParent { get; }

    /// <summary>
    /// Called when the overlay gets pushed.
    /// </summary>
    void OnPush();
    
    /// <summary>
    /// Called when the overlay gets popped.
    /// </summary>
    void OnPop();
}