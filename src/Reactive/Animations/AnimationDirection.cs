namespace Reactive;

/// <summary>
/// Defines which way each iteration of an animation is played.
/// </summary>
public enum AnimationDirection {
    /// <summary>
    /// Every iteration plays from start to end.
    /// </summary>
    Normal,

    /// <summary>
    /// Every iteration plays from end to start.
    /// </summary>
    Reverse,

    /// <summary>
    /// The first iteration plays forwards, then every next one flips the direction.
    /// </summary>
    Alternate,

    /// <summary>
    /// The first iteration plays backwards, then every next one flips the direction.
    /// </summary>
    AlternateReverse
}
