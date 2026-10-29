using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

[Flags]
public enum ScrollUpdateType {
    None = 0,
    /// User intent to scroll (e.g. ScrollPos, ScrollItem).
    Intent = 1 << 0,
    /// Controller measurements (e.g. ContentSize, ViewSize).
    Measurements = 1 << 1,
    /// Controller scroll (ActualScrollPos).
    Scroll = 1 << 2,
    /// <summary>
    /// Controller scroll has just finished.
    /// </summary>
    ScrollFinished = 1 << 3
}

[PublicAPI]
public static class ScrollUpdateExtensions {
    extension(ScrollUpdateType type) {
        public bool IsIntent => (type & ScrollUpdateType.Intent) != 0;
        public bool IsMeasurements => (type & ScrollUpdateType.Measurements) != 0;
        public bool IsScroll => (type & ScrollUpdateType.Scroll) != 0;
        public bool IsScrollFinished => (type & ScrollUpdateType.ScrollFinished) != 0;
    }
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
    /// Kinds of updates delivered with the last flush, combined as flags. Kept until the next flush.
    /// </summary>
    public ScrollUpdateType UpdateType { get; private set; }

    private ScrollUpdateType _pendingUpdateType;

    /// <summary>
    /// Helper to get or set scroll position in a 0..1 range.
    /// </summary>
    public float NormalizedScrollPos {
        get => MaxScrollPos > 0f ? Mathf.Clamp01(ActualScrollPos / MaxScrollPos) : 0f;
    }

    public float NormalizedPageHeight {
        get => ContentSize > 0f ? Mathf.Clamp01(ViewSize / ContentSize) : 1f;
    }

    public float MaxScrollPos {
        get => Mathf.Max(0f, ContentSize - ViewSize);
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
        
        if (ScrollPos > MaxScrollPos) {
            ScrollPos = MaxScrollPos;
        }

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
        _pendingUpdateType |= type;
        NotifyValueInvalidated();
    }

    protected override void OnFlush() {
        UpdateType = _pendingUpdateType;
        _pendingUpdateType = ScrollUpdateType.None;
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