using JetBrains.Annotations;
using Reactive.Components;
using UnityEngine;

namespace Reactive.BeatSaber;

[PublicAPI]
public readonly record struct CompositeColorSet(
    ColorSet Colors,
    ColorSet GradientColors0,
    ColorSet GradientColors1
) {
    public CompositeColors GetColors(GraphicState state) {
        return new(
            Colors.GetColor(state),
            GradientColors0.GetColor(state),
            GradientColors1.GetColor(state)
        );
    }
}

[PublicAPI]
public readonly record struct CompositeColors(
    Color Color,
    Color GradientColor0,
    Color GradientColor1
);

[PublicAPI]
public static class ColorSetExtensions {
    public static IState<CompositeColors> MapColorSet(this IState<GraphicState> state, CompositeColorSet set) {
        return state.Map(set.GetColors);
    }
}