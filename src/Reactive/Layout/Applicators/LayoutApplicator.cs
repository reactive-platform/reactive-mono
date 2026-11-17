using JetBrains.Annotations;

namespace Reactive;

/// <summary>
/// Defines how an item reaches the dimensions calculated for it, e.g. instantly or with an animation.
/// </summary>
/// <remarks>
/// An applicator may keep the state of the item it's applied to, so it can be used by one component at a time.
/// </remarks>
[PublicAPI]
public abstract class LayoutApplicator {
    #region Factories

    /// <summary>
    /// Animates all values of the item the same way. Use <see cref="AnimatedLayoutApplicator"/> directly
    /// to animate them differently.
    /// </summary>
    /// <param name="timing">How the item moves towards the dimensions, e.g. <c>200.ms</c> or <c>10.fact</c>.</param>
    /// <param name="curve">A curve applied to the progress. Linear if not set.</param>
    public static AnimatedLayoutApplicator Animated(AnimationTiming timing, AnimationCurve? curve = null) {
        return new() { All = new(timing, curve) };
    }

    /// <summary>
    /// Creates an applicator that leaves the application to the callback.
    /// </summary>
    /// <param name="callback">Called in place of <see cref="Apply"/>.</param>
    public static LayoutApplicator Callback(CallbackLayoutApplicator.ApplyCallback callback) {
        return new CallbackLayoutApplicator(callback);
    }

    #endregion
    
    /// <summary>
    /// Called when the item gets new dimensions, then on each Update while it returns true and the item is active.
    /// </summary>
    /// <param name="current">The dimensions returned by the previous call, the target ones if the item has none yet.</param>
    /// <param name="target">The calculated dimensions.</param>
    /// <param name="dimensions">The dimensions to write. The ones returned on Update are written after the layout.</param>
    /// <returns>True to be called again next frame, otherwise False.</returns>
    public abstract bool Apply(LayoutDimensions current, LayoutDimensions target, out LayoutDimensions dimensions);
}
