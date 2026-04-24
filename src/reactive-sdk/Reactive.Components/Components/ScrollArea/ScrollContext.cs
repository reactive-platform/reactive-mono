using JetBrains.Annotations;

namespace Reactive.Components;

[PublicAPI]
public readonly record struct ScrollContext(float ScrollPos, bool Immediately) {
    /// <summary>
    /// Scrolls to the specified point.
    /// </summary>
    /// <param name="pos">A pos to scroll to.</param>
    /// <param name="immediately">Whether to apply the scroll immediately.</param>
    /// <returns>An updated context.</returns>
    public ScrollContext ScrollTo(float pos, bool immediately = false) {
        return new() {
            ScrollPos = pos,
            Immediately = immediately
        };
    }
}

[PublicAPI]
public static class ScrollContextExtension {
    /// <summary>
    /// Updates the state with a new scroll context.
    /// </summary>
    /// <param name="pos">A pos to scroll to.</param>
    /// <param name="immediately">Whether to apply the scroll immediately.</param>
    public static void ScrollTo(this IMutableState<ScrollContext> state, float pos, bool immediately = false) {
        state.Value = state.Value.ScrollTo(pos, immediately);
    }
}