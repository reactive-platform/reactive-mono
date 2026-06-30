using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reactive.Components;
using UnityEngine;

namespace Reactive.BeatSaber.Components;

/// <summary>
/// A wrapper overlay that carries a bounded content view.
/// Provides support for placing and managing a view inside an overlay.
/// </summary>
[PublicAPI]
public class Modal : ReactiveComponent, ILayoutDriver {
    #region Public API

    /// <summary>
    /// A component that is displayed under the view.
    /// You can use it to apply effects like background dimming.
    /// </summary>
    public Image Blocker => _blocker;

    /// <summary>
    /// A component that holds the actual content.
    /// </summary>
    public Layout View => _content;

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
        get => _overlay.IsPushed;
        set {
            if (_overlay.IsPushed == value) {
                return;
            }

            _overlay.IsPushed = value;

            if (value && PlacementAnchor != null) {
                PlacementTool.Place(_content.ContentTransform, PlacementAnchor, PlacementData);
            }
        }
    }

    #endregion

    #region Impl

    public ICollection<ILayoutItem> Children => _content.Children;

    public ILayoutController? LayoutController {
        get => _content.LayoutController;
        set => _content.LayoutController = value;
    }

    private Overlay _overlay = null!;
    private Layout _content = null!;
    private Image _blocker = null!;

    protected override GameObject Construct() {
        return new Overlay {
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

                new Layout {
                    Name = "Content",

                    FlexController = {
                        ConstrainHorizontal = false,
                        ConstrainVertical = false
                    },
                }.Bind(ref _content)
            }
        }.Bind(ref _overlay).Use();
    }

    #endregion
}