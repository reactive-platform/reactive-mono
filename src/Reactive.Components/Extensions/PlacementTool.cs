using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components;

/// <summary>
/// Defines all available placement combinations.
/// </summary>
[PublicAPI]
public enum RelativePlacement {
    LeftTop,
    LeftCenter,
    LeftBottom,
    TopRight,
    TopCenter,
    TopLeft,
    RightTop,
    RightCenter,
    RightBottom,
    BottomRight,
    BottomCenter,
    BottomLeft,
    Center
}

[PublicAPI]
public struct PlacementData(
    RelativePlacement placement,
    Vector2 offset,
    bool clip
) {
    public PlacementData() : this(RelativePlacement.Center, Vector2.zero, true) { }

    /// <summary>
    /// Defines placement against anchor object.
    /// </summary>
    public RelativePlacement Placement { get; init; } = placement;

    /// <summary>
    /// Defines an offset which is applied to the calculated position.
    /// </summary>
    public Vector2 Offset { get; init; } = offset;

    /// <summary>
    /// Defines whether the object should be moved back inside the root canvas when it doesn't fit. Enabled by default.
    /// </summary>
    public bool Clip { get; init; } = clip;
}

[PublicAPI]
public static class PlacementTool {
    /// <summary>
    /// Places an object according to params from data.
    /// </summary>
    /// <param name="target">An object to place.</param>
    /// <param name="anchor">An object to place against.</param>
    /// <param name="data">Placement configuration.</param>
    public static void Place(RectTransform target, RectTransform anchor, in PlacementData data) {
        var parent = target.parent;

        CalculateRelativePlacement(
            parent,
            anchor,
            data.Placement,
            data.Offset,
            out var position,
            out var pivot
        );

        if (data.Clip && TryGetClipBounds(target, out var bounds)) {
            var size = target.rect.size;

            // Clipping both x and y
            for (var i = 0; i < 2; i++) {
                if (size[i] <= 0f) {
                    continue;
                }

                var shift = CalculateClippedPos(position[i], size[i], pivot[i], bounds.min[i], bounds.max[i]) - position[i];

                // Moving the rect by shifting the pivot instead of the position,
                // so the pivot stays at the intended point and animations scale from there
                pivot[i] -= shift / size[i];
            }
        }

        target.localPosition = position;
        target.pivot = pivot;
    }

    /// <summary>
    /// Shifts a position so the rect stays within the bounds. Rects larger than the bounds are aligned to the min edge.
    /// </summary>
    public static float CalculateClippedPos(float pos, float size, float pivot, float boundsMin, float boundsMax) {
        // Calculate the edges of the rect based on pivot and size
        var minPos = pos - pivot * size;
        var maxPos = pos + (1 - pivot) * size;

        // Shift the position if it goes out of bounds
        if (maxPos > boundsMax) {
            pos -= maxPos - boundsMax;
            minPos -= maxPos - boundsMax;
        }

        if (minPos < boundsMin) {
            pos += boundsMin - minPos;
        }

        return pos;
    }

    private static readonly Vector3[] corners = new Vector3[4];

    /// <summary>
    /// Gets the root canvas rect in the local space of the target's parent.
    /// </summary>
    private static bool TryGetClipBounds(RectTransform target, out Rect bounds) {
        var parent = target.parent;
        var canvas = target.GetComponentInParent<Canvas>()?.rootCanvas;

        if (parent == null || canvas == null) {
            bounds = default;
            return false;
        }

        ((RectTransform)canvas.transform).GetWorldCorners(corners);

        // Bottom left and top right, the canvas can be rotated against the parent but not skewed
        var min = (Vector2)parent.InverseTransformPoint(corners[0]);
        var max = (Vector2)parent.InverseTransformPoint(corners[2]);

        bounds = Rect.MinMaxRect(
            Mathf.Min(min.x, max.x),
            Mathf.Min(min.y, max.y),
            Mathf.Max(min.x, max.x),
            Mathf.Max(min.y, max.y)
        );

        return true;
    }

    public static void CalculateRelativePlacement(
        Transform parent,
        RectTransform anchor,
        RelativePlacement placement,
        Vector2 offset,
        out Vector2 position,
        out Vector2 pivot
    ) {
        position = parent.InverseTransformPoint(anchor.position);

        var rect = anchor.rect;
        var anchorHeightDiv = new Vector2(0f, rect.height / 2);
        var anchorWidthDiv = new Vector2(rect.width / 2, 0f);

        position = placement switch {
            RelativePlacement.LeftTop => position - anchorWidthDiv + anchorHeightDiv - offset,
            RelativePlacement.LeftCenter => position - anchorWidthDiv + new Vector2(-offset.x, offset.y),
            RelativePlacement.LeftBottom => position - anchorWidthDiv - anchorHeightDiv + new Vector2(-offset.x, offset.y),
            RelativePlacement.TopLeft => position + anchorHeightDiv - anchorWidthDiv + offset,
            RelativePlacement.TopCenter => position + anchorHeightDiv + offset,
            RelativePlacement.TopRight => position + anchorHeightDiv + anchorWidthDiv + new Vector2(-offset.x, offset.y),
            RelativePlacement.RightTop => position + anchorWidthDiv + anchorHeightDiv + new Vector2(offset.x, -offset.y),
            RelativePlacement.RightCenter => position + anchorWidthDiv + offset,
            RelativePlacement.RightBottom => position + anchorWidthDiv - anchorHeightDiv + offset,
            RelativePlacement.BottomLeft => position - anchorHeightDiv - anchorWidthDiv + new Vector2(offset.x, -offset.y),
            RelativePlacement.BottomCenter => position - anchorHeightDiv + new Vector2(offset.x, -offset.y),
            RelativePlacement.BottomRight => position - anchorHeightDiv + anchorWidthDiv - offset,
            RelativePlacement.Center => position + offset + rect.size * Vector2.one * 0.5f - rect.size * anchor.pivot,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };

        pivot = placement switch {
            RelativePlacement.LeftTop => new(1f, 1f),
            RelativePlacement.LeftCenter => new(1f, 0.5f),
            RelativePlacement.LeftBottom => new(1f, 0f),
            RelativePlacement.TopLeft => new(0f, 0f),
            RelativePlacement.TopCenter => new(0.5f, 0f),
            RelativePlacement.TopRight => new(1f, 0f),
            RelativePlacement.RightTop => new(0f, 1f),
            RelativePlacement.RightCenter => new(0f, 0.5f),
            RelativePlacement.RightBottom => new(0f, 0f),
            RelativePlacement.BottomLeft => new(0f, 1f),
            RelativePlacement.BottomCenter => new(0.5f, 1f),
            RelativePlacement.BottomRight => new(1f, 1f),
            RelativePlacement.Center => Vector2.one * 0.5f,
            _ => throw new ArgumentOutOfRangeException(nameof(placement), placement, null)
        };
    }
}