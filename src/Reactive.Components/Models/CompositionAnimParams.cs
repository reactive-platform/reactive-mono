using JetBrains.Annotations;

namespace Reactive.Components;

/// <summary>
/// Defines how an overlay affects the composition it gets pushed to.
/// </summary>
[PublicAPI]
public readonly record struct CompositionAnimParams {
    public CompositionAnimParams() { }

    /// <summary>
    /// Determines whether the composition should be dimmed while the overlay is pushed.
    /// </summary>
    public bool DimCanvas { get; init; } = true;

    /// <summary>
    /// An alpha applied to the composition while it is dimmed.
    /// </summary>
    public float DimmedAlpha { get; init; } = 0.2f;

    /// <summary>
    /// The duration of the dimming transition in seconds. Used for both dimming and restoring.
    /// </summary>
    public float Duration { get; init; } = 0.15f;

    /// <summary>
    /// A curve applied to the transition progress. Linear if not set.
    /// </summary>
    public AnimationCurve? Curve { get; init; }
}
