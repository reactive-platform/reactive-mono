using System;
using JetBrains.Annotations;

namespace Reactive.BeatSaber.Components;

/// <summary>
/// A stand-in for a single-level modal that is leased from <see cref="GlobalReactivePool"/> only while pushed.
/// Placed in the hierarchy like a regular modal. When pushed, it takes a modal from the pool, attaches it to the composition,
/// applies <see cref="ApplyProps"/> and pushes it. Once closed, the modal goes back to the pool,
/// so a single instance serves every shared modal of the same type.
/// Only modals that explicitly implement <see cref="IPoolableComponent"/> can be shared.
/// </summary>
[PublicAPI]
public class SharedModal<T> : ReactiveComponent where T : ModalBase, IPoolableComponent, new() {
    #region Public API

    /// <summary>
    /// Applied to the modal every time it is taken from the pool, right before it gets pushed.
    /// The modal is reset when returned, so everything it needs has to be set here.
    /// </summary>
    public Action<T>? ApplyProps { get; set; }

    /// <inheritdoc cref="ModalBase.IsPushed"/>
    public bool IsPushed {
        get => _isPushed;
        set {
            if (_isPushed == value) {
                return;
            }

            _isPushed = value;

            if (value) {
                Acquire().IsPushed = true;
            } else if (_modal != null) {
                _modal.IsPushed = false;
            }
        }
    }

    /// <summary>
    /// The leased modal, if any. Still set while the modal is playing the close animation.
    /// </summary>
    public T? Modal => _modal;

    #endregion

    #region Impl

    private bool _isPushed;
    private T? _modal;

    private T Acquire() {
        // Opened again while closing, so it's still ours
        if (_modal != null) {
            return _modal;
        }

        var composition = Composition.GetComposition(Content);
        var modal = GlobalReactivePool.Spawn<T>();

        modal.Use(composition.transform);
        ApplyProps?.Invoke(modal);

        var onPopped = modal.OnPopped;
        modal.OnPopped = () => {
            onPopped?.Invoke();
            HandleModalPopped();
        };

        _modal = modal;
        return modal;
    }

    private void HandleModalPopped() {
        // Could be closed by the modal itself, so the state follows it
        _isPushed = false;
        Release();
    }

    private void Release() {
        if (_modal is not { } modal) {
            return;
        }

        _modal = null;
        GlobalReactivePool.Despawn(modal);
    }

    protected override void OnInitialize() {
        IgnoreLayout = true;
    }

    protected override void OnDestroy() {
        _isPushed = false;
        Release();
    }

    #endregion
}
