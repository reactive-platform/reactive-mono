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
    /// <summary>
    /// Defines placement against anchor object.
    /// </summary>
    public RelativePlacement Placement = placement;

    /// <summary>
    /// Defines an offset which is applied to the calculated position.
    /// </summary>
    public Vector2 Offset = offset;

    /// <summary>
    /// Defines whether the object should be clipped within parent bounds or not.
    /// </summary>
    public bool Clip = clip;
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

        if (data.Clip && parent is RectTransform parentRect) {
            var parentSize = parentRect.rect.size;

            // Translating from any pivot to 0,0
            var translationDelta = parentRect.pivot * parentSize;
            var actualPos = position + translationDelta;

            // Clipping both x and y
            for (var i = 0; i < 2; i++) {
                actualPos[i] = CalculateClippedPos(actualPos[i], target.rect.size[i], pivot[i], parentSize[i]);
            }

            position = actualPos - translationDelta;
        }

        target.localPosition = position;
        target.pivot = pivot;
    }

    public static float CalculateClippedPos(float pos, float size, float pivot, float parentSize) {
        // Calculate the edges of the rect based on pivot and size
        var minPos = pos - pivot * size;
        var maxPos = pos + (1 - pivot) * size;

        // Shift the position if it goes out of bounds
        if (minPos < 0) {
            pos += 0f - minPos;
        } else if (maxPos > parentSize) {
            pos -= maxPos - parentSize;
        }

        return pos;
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