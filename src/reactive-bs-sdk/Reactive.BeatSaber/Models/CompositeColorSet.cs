using JetBrains.Annotations;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using UnityEngine;

namespace Reactive.BeatSaber;

[PublicAPI]
public readonly record struct CompositeColorSet(
    ColorSet Colors,
    ColorSet GradientColors0,
    ColorSet GradientColors1
) {
    public static readonly CompositeColorSet White = new(ColorSet.White, ColorSet.White, ColorSet.White);

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
    
    extension<T>(IComponentHolder<T> holder) where T : Image {
        public CompositeColors Colors {
            set {
                var img = holder.Component;
                img.Color = value.Color;
                img.GradientColor0 = value.GradientColor0;
                img.GradientColor1 = value.GradientColor1;
            }
        }
    }
}