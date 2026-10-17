using System;
using JetBrains.Annotations;
using Reactive.Components;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

[PublicAPI]
public abstract class ModalBase : ReactiveComponent {
    #region Public API

    /// <summary>
    /// Defines placement params for <see cref="PlacementAnchor"/>.
    /// </summary>
    public PlacementData PlacementData { get; set; } = new() { Placement = RelativePlacement.Center };

    /// <summary>
    /// An anchor to place against. If not set, <see cref="PlacementData"/> won't do anything.
    /// </summary>
    public RectTransform? PlacementAnchor { get; set; }

    /// <summary>
    /// Called when user clicks outside the modal view.
    /// </summary>
    public Action? OnClickOutside { get; set; }

    #endregion

    #region Overlay API

    /// <inheritdoc cref="Overlay.ZIndex"/>
    public int ZIndex {
        get => _overlay.ZIndex;
        set => _overlay.ZIndex = value;
    }

    /// <inheritdoc cref="Overlay.OnPushed"/>
    public Action? OnPushed {
        get => _overlay.OnPushed;
        set => _overlay.OnPushed = value;
    }

    /// <inheritdoc cref="Overlay.OnPopped"/>
    public Action? OnPopped {
        get => _overlay.OnPopped;
        set => _overlay.OnPopped = value;
    }

    /// <inheritdoc cref="Overlay.IsPushed"/>
    public bool IsPushed {
        get;
        set {
            if (field == value) {
                return;
            }

            field = value;
            _blocker.RaycastTarget = value;

            if (value) {
                // Might still be pushed if opened again while closing
                _overlay.IsPushed = true;

                if (OpenAnimation is { } openAnimation) {
                    _animationPlayer.Play(openAnimation);
                } else {
                    _animationPlayer.Stop();
                }
            } else if (CloseAnimation is { } closeAnimation) {
                // The overlay is popped once the animation finishes
                _animationPlayer.Play(closeAnimation, HandleCloseAnimationFinished);
            } else {
                _animationPlayer.Stop();
                _overlay.IsPushed = false;
            }
        }
    }
    
    public Animation<IReactiveComponent>? OpenAnimation { get; set; } = ModalAnimations.JumpOpen;
    public Animation<IReactiveComponent>? CloseAnimation { get; set; } = ModalAnimations.JumpClose;

    #endregion

    #region Impl

    private IReactiveComponent _content = null!;
    private Overlay _overlay = null!;
    private Image _blocker = null!;
    private AnimationPlayer<IReactiveComponent> _animationPlayer = null!;

    protected sealed override GameObject Construct() {
        var go = new Overlay {
            Enabled = false,

            Children = {
                new Image {
                        Name = "Blocker",
                        Sprite = ReactiveResources.TransparentPixel,
                        RaycastTarget = true,
                    }
                    .Bind(ref _blocker)
                    .WithRectExpand()
                    .WithPointerEvents(onDown: _ => OnClickOutside?.Invoke()),

                ConstructContent()
                    .With(x => x.LayoutUpdatedEvent += HandleLayoutUpdated)
                    .Bind(ref _content)
            }
        }.Bind(ref _overlay).Use();

        _animationPlayer = RememberAnimationPlayer(_content);

        return go;
    }

    protected abstract IReactiveComponent ConstructContent();

    private void HandleCloseAnimationFinished() {
        _overlay.IsPushed = false;
    }

    private void HandleLayoutUpdated(ILayoutItem item) {
        // Placing while closing too, as the overlay is still pushed
        if (!_overlay.IsPushed || PlacementAnchor == null) {
            return;
        }

        PlacementTool.Place(_content.ContentTransform, PlacementAnchor, PlacementData);
    }

    #endregion
}