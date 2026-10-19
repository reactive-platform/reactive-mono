using JetBrains.Annotations;
using UnityEngine;

namespace Reactive;

/// <summary>
/// Defines how an element's size is measured.
/// </summary>
[PublicAPI]
public enum MeasureMode {
    /// <summary>No specific measurement mode.</summary>
    Undefined,
    /// <summary>Element must be exactly the specified size.</summary>
    Exactly,
    /// <summary>Element can be up to the specified size.</summary>
    AtMost
}

/// <summary>
/// An object that can be measured inside a layout. Used for intercommunication
/// between different layout trees.
/// </summary>
[PublicAPI]
public interface ILayoutMeasurable {
    /// <summary>
    /// Measures the node dimensions according to the provided policies.
    /// </summary>
    Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode);
}