using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

public enum ScrollUpdateType {
    None,
    /// User intent to scroll (e.g. ScrollPos, ScrollItem).
    Intent,
    /// Controller measurements (e.g. ContentSize, ViewSize).
    Measurements,
    /// Controller scroll (ActualScrollPos).
    Scroll,
    /// <summary>
    /// Controller scroll has just finished.
    /// </summary>
    ScrollFinished
}

[PublicAPI]
public class ScrollContext : StateBase<ScrollContext>, IState<ScrollContext> {
    /// <summary>
    /// Determines the current scroll pos. Set by the user.
    /// </summary>
    public float ScrollPos { get; private set; }

    /// Whether the scroll animation should finish immediately. Set by the user.
    public bool Immediately { get; private set; }

    /// <summary>
    /// The actual scroll pos of the scroll controller at the moment.
    /// Set by a component controlling the view.
    /// </summary>
    public float ActualScrollPos { get; private set; }

    /// <summary>
    /// The size of a single line inside the scroll content.
    /// Set by a component controlling the view.
    /// </summary>
    public float LineSize { get; private set; }

    /// <summary>
    /// The size of the scroll content. Set by a component controlling the view.
    /// </summary>
    public float ContentSize { get; private set; }

    /// <summary>
    /// The size of the viewport. Set by a component controlling the view.
    /// </summary>
    public float ViewSize { get; private set; }

    /// <summary>
    /// Determines a kind of the last update. Returns None if called outside the ValueChanged cycle.
    /// </summary>
    public ScrollUpdateType UpdateType { get; private set; }

    /// <summary>
    /// Helper to get or set scroll position in a 0..1 range.
    /// </summary>
    public float NormalizedScrollPos {
        get => Mathf.Clamp01(ActualScrollPos / MaxScrollPos);
    }

    public float NormalizedPageHeight {
        get => Mathf.Clamp01(ViewSize / ContentSize);
    }

    public float MaxScrollPos {
        get => ContentSize - ViewSize;
    }

    public bool CanScrollUp {
        get => ScrollPos > 0;
    }

    public bool CanScrollDown {
        get => ScrollPos < MaxScrollPos;
    }

    public bool CanScroll {
        // Basically the same logic as CanScrollDown, but this might change
        // in the future, so moved into a separate property
        get => ContentSize > ViewSize;
    }

    /// <summary>
    /// Scrolls to the specified point.
    /// </summary>
    /// <param name="pos">The position to scroll to.</param>
    /// <param name="immediately">Whether to apply the scroll immediately.</param>
    /// <returns>An updated context.</returns>
    public void ScrollTo(float pos, bool immediately = false) {
        var newPos = Mathf.Clamp(pos, 0f, MaxScrollPos);

        if (Mathf.Approximately(newPos, ScrollPos)) {
            return;
        }

        ScrollPos = newPos;
        Immediately = immediately;

        NotifyValueChanged(ScrollUpdateType.Intent);
    }

    public void ScrollRelative(float offset, bool immediately = false) {
        ScrollTo(ScrollPos + offset, immediately);
    }

    /// <summary>
    /// Scrolls down for a size of the viewport.
    /// </summary>
    /// <param name="immediately">Whether to apply the scroll immediately.</param>
    public void PageDown(bool immediately = false) {
        ScrollRelative(ViewSize, immediately);
    }

    /// <summary>
    /// Scrolls up for a size of the viewport.
    /// </summary>
    /// <param name="immediately">Whether to apply the scroll immediately.</param>
    public void PageUp(bool immediately = false) {
        ScrollRelative(ViewSize * -1, immediately);
    }

    public void LineDown(bool immediately = false) {
        ScrollRelative(LineSize, immediately);
    }

    public void LineUp(bool immediately = false) {
        ScrollRelative(LineSize * -1, immediately);
    }

    /// <summary>
    /// Sets internal measurements of the scroll controller. Shouldn't be called manually.
    /// </summary>
    /// <param name="contentSize">The size of the content.</param>
    /// <param name="viewSize">The size of the viewport.</param>
    /// <param name="lineSize">The size of a single line. Can be left 0 on non-list views.</param>
    public void ControllerSetMeasurements(float contentSize, float viewSize, float lineSize) {
        ContentSize = contentSize;
        ViewSize = viewSize;
        LineSize = lineSize;
        NotifyValueChanged(ScrollUpdateType.Measurements);
    }

    /// <summary>
    /// Sets internal scroll of the scroll controller at the moment. Shouldn't be called manually.
    /// </summary>
    public void ControllerSetScrollPos(float pos) {
        if (Mathf.Approximately(ActualScrollPos, pos)) {
            return;
        }

        ActualScrollPos = pos;
        NotifyValueChanged(ScrollUpdateType.Scroll);
    }

    public void ControllerNotifyScrollCompleted() {
        NotifyValueChanged(ScrollUpdateType.ScrollFinished);
    }

    #region State Impl

    ScrollContext IState<ScrollContext>.Value => this;

    private void NotifyValueChanged(ScrollUpdateType type) {
        UpdateType = type;

        NotifyValueChanged(this);

        UpdateType = ScrollUpdateType.None;
    }

    public override bool Equals(object? obj) {
        // ReSharper disable CompareOfFloatsByEqualityOperator
        return obj is ScrollContext context &&
            context.Immediately.Equals(Immediately) &&
            context.ScrollPos.Equals(ScrollPos) &&
            context.ActualScrollPos.Equals(ActualScrollPos) &&
            context.ContentSize.Equals(ContentSize) &&
            context.ViewSize.Equals(ViewSize);
    }

    public override int GetHashCode() {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        unchecked {
            var hashCode = ScrollPos.GetHashCode();
            hashCode = (hashCode * 397) ^ Immediately.GetHashCode();
            hashCode = (hashCode * 397) ^ ActualScrollPos.GetHashCode();
            hashCode = (hashCode * 397) ^ ContentSize.GetHashCode();
            hashCode = (hashCode * 397) ^ ViewSize.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}