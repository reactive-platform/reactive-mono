using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

/// <summary>
/// A helper class for working with layout.
/// </summary>
[PublicAPI]
public static class LayoutTool {
    /// <summary>
    /// Checks whether the item is laid out by the controller of its parent. Such an item is a part of the parent's
    /// hierarchy, so its layout can't be recalculated on its own.
    /// </summary>
    /// <param name="item">An item to check.</param>
    public static bool IsControlledByParent(ILayoutItem item) {
        return !item.IgnoreLayout && (item.LayoutDriver?.LayoutController?.HasChild(item) ?? false);
    }

    /// <summary>
    /// Calculates a leaf size based on provided values.
    /// </summary>
    /// <param name="preferredSize">A preferred size of the node.</param>
    /// <param name="width">A width constraint.</param>
    /// <param name="widthMode">A width measurement mode.</param>
    /// <param name="height">A height constraint.</param>
    /// <param name="heightMode">A height measurement mode.</param>
    public static Vector2 MeasureNode(Vector2 preferredSize, float width, MeasureMode widthMode, float height, MeasureMode heightMode) {
        width = MeasureNodeAxis(preferredSize.x, width, widthMode);
        height = MeasureNodeAxis(preferredSize.y, height, heightMode);

        return new(width, height);
    }

    /// <summary>
    /// Fits a size into the constraints keeping its aspect. An imposed axis defines the other one, bounded axes
    /// scale the size down uniformly. Meant to be applied to a preferred size before measuring.
    /// </summary>
    /// <param name="size">A size to fit.</param>
    /// <param name="width">A width constraint.</param>
    /// <param name="widthMode">A width measurement mode.</param>
    /// <param name="height">A height constraint.</param>
    /// <param name="heightMode">A height measurement mode.</param>
    public static Vector2 FitPreservingAspect(Vector2 size, float width, MeasureMode widthMode, float height, MeasureMode heightMode) {
        if (size.x <= 0f || size.y <= 0f) {
            return size;
        }

        var aspect = size.x / size.y;

        return (widthMode, heightMode) switch {
            // An imposed axis defines the other one
            (MeasureMode.Exactly, _) => new(width, width / aspect),
            (_, MeasureMode.Exactly) => new(height * aspect, height),

            // Bounded axes scale the size down uniformly
            (MeasureMode.AtMost, MeasureMode.AtMost) => size * Mathf.Min(1f, width / size.x, height / size.y),
            (MeasureMode.AtMost, _) => size * Mathf.Min(1f, width / size.x),
            (_, MeasureMode.AtMost) => size * Mathf.Min(1f, height / size.y),

            _ => size
        };
    }

    /// <summary>
    /// Calculates a leaf size based on provided values.
    /// </summary>
    /// <param name="preferredSize">A preferred size of the node.</param>
    /// <param name="constraint">A size constraint.</param>
    /// <param name="mode">A size measurement mode.</param>
    public static float MeasureNodeAxis(float preferredSize, float constraint, MeasureMode mode) {
        //  Each axis is passed a MeasureMode as a constraint:
        //
        //  Exactly:
        //      The measured length of the given axis is imposed to be the available length. This corresponds to stretch-fit sizing.
        //  Undefined:
        //      The measured length in the given axis should be the maximum natural length of the content. This corresponds to max-content sizing.
        //  AtMost:
        //      The measured length in the given axis should be the minimum of the available space in the axis, and the natural content size. This corresponds to fit-content sizing.
        //
        // see https://www.yogalayout.dev/docs/advanced/external-layout-systems

        switch (mode) {
            case MeasureMode.Undefined:
                return preferredSize;

            case MeasureMode.Exactly:
                return constraint;

            case MeasureMode.AtMost:
                return Mathf.Min(preferredSize, constraint);

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}