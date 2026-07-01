using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public record struct ColorSet(
        Color Color,
        Color HoveredColor,
        Color ActiveColor,
        Color NotInteractableColor
    ) {
        public static readonly ColorSet White = new(Color.white, Color.white, Color.white, Color.white);
        
        public Color GetColor(GraphicState state) {
            if (state.IsHovered) {
                return state.IsActive ? ActiveColor : HoveredColor;
            }
            if (state.IsActive) {
                return ActiveColor;
            }
            if (!state.IsInteractable) {
                return NotInteractableColor;
            }
            return Color;
        }
    }

    [PublicAPI]
    public static class ColorSetExtensions {
        public static MappedState<GraphicState, Color> MapColorSet(this IState<GraphicState> state, ColorSet set) {
            return state.Map(set.GetColor);
        }
    }
}