using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components.Basic;

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
    public Layout View => _wrapper;

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

    /// <summary>
    /// Pushes the modal to the stack using params from the object.
    /// </summary>
    public bool Push() {
        if (!_overlay.Push()) {
            return false;
        }

        if (PlacementAnchor != null) {
            AlignModal(PlacementAnchor, PlacementData);
        }

        return true;
    }

    /// <summary>
    /// Pushes the modal to the stack using custom params. This method doesn't change the actual data,
    /// so further <see cref="Push"/> calls will still use values from the object.
    /// </summary>
    public bool PushExplicitly(RectTransform anchor, in PlacementData data) {
        if (!_overlay.Push()) {
            return false;
        }

        AlignModal(anchor, data);

        return true;
    }

    /// <summary>
    /// Pops the modal from the stack.
    /// </summary>
    public bool Pop() {
        return _overlay.Pop();
    }

    private void AlignModal(RectTransform anchor, in PlacementData data) {
        PlacementTool.Place(_wrapper.ContentTransform, anchor, data);
    }

    #endregion

    #region Overlay API

    /// <inheritdoc cref="Overlay.ZIndex"/>
    public int ZIndex { get; set; }

    /// <inheritdoc cref="Overlay.IsPushed"/>
    public bool IsPushed { get; private set; }

    /// <inheritdoc cref="Overlay.OnPushed"/>
    public Action? OnPushed { get; set; }

    /// <inheritdoc cref="Overlay.OnPopped"/>
    public Action? OnPopped { get; set; }

    #endregion

    #region Impl

    public ICollection<ILayoutItem> Children => _wrapper.Children;

    public ILayoutController? LayoutController {
        get => _wrapper.LayoutController;
        set => _wrapper.LayoutController = value;
    }

    private Overlay _overlay = null!;
    private Layout _wrapper = null!;
    private Image _blocker = null!;

    protected override GameObject Construct() {
        return new Overlay {
            Children = {
                new Image()
                    .Bind(ref _blocker)
                    .WithRectExpand()
                    .WithPointerEvents(onDown: _ => OnClickOutside?.Invoke()),

                new Layout().Bind(ref _wrapper)
            }
        }.Bind(ref _overlay).Use();
    }

    #endregion
}