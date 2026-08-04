using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public record struct ColorSet {
        public Color Color { get; init; }
        public Color? HoveredColor { get; init; }
        public Color? ActiveColor { get; init; }
        public Color? HoveredActiveColor { get; init; }
        public Color? NotInteractableColor { get; init; }
        public Color? NotInteractableHoveredColor { get; init; }
        public Color? NotInteractableActiveColor { get; init; }
        public Color? NotInteractableHoveredActiveColor { get; init; }

        public static readonly ColorSet White = new() { Color = Color.white };

        public Color? this[GraphicState state] {
            get {
                return (byte)state switch {
                    1 => HoveredColor,
                    2 => ActiveColor,
                    3 => HoveredActiveColor,
                    4 => NotInteractableColor,
                    5 => NotInteractableHoveredColor,
                    6 => NotInteractableActiveColor,
                    7 => NotInteractableHoveredActiveColor,
                    _ => Color
                };
            }
            init {
                switch ((byte)state) {
                    case 1: HoveredColor = value; break;
                    case 2: ActiveColor = value; break;
                    case 3: HoveredActiveColor = value; break;
                    case 4: NotInteractableColor = value; break;
                    case 5: NotInteractableHoveredColor = value; break;
                    case 6: NotInteractableActiveColor = value; break;
                    case 7: NotInteractableHoveredActiveColor = value; break;
                    default: Color = value ?? Color; break;
                }
            }
        }

        [Pure]
        public ColorSet With(GraphicState state, Color? color) {
            var key = (byte)state;

            return key switch {
                1 => this with { HoveredColor = color },
                2 => this with { ActiveColor = color },
                3 => this with { HoveredActiveColor = color },
                4 => this with { NotInteractableColor = color },
                5 => this with { NotInteractableHoveredColor = color },
                6 => this with { NotInteractableActiveColor = color },
                7 => this with { NotInteractableHoveredActiveColor = color },
                _ => this with { Color = color ?? Color }
            };
        }

        /// <summary>
        /// Resolves the state color, cascading down fallbacks to <see cref="Color"/>.
        /// </summary>
        public Color GetColor(GraphicState state) {
            var stateKey = (byte)state;

            // Using hardcoded values instead of mask math to improve performance
            return stateKey switch {
                // 7: NotInteractable + Hovered + Active
                7 => NotInteractableHoveredActiveColor
                    ?? NotInteractableActiveColor
                    ?? NotInteractableHoveredColor
                    ?? NotInteractableColor
                    ?? Color,

                // 6: NotInteractable + Active
                6 => NotInteractableActiveColor
                    ?? NotInteractableColor
                    ?? Color,

                // 5: NotInteractable + Hovered
                5 => NotInteractableHoveredColor
                    ?? NotInteractableColor
                    ?? Color,

                // 4: NotInteractable
                4 => NotInteractableColor
                    ?? Color,

                // 3: Hovered + Active
                3 => HoveredActiveColor
                    ?? ActiveColor
                    ?? HoveredColor
                    ?? Color,

                // 2: Active
                2 => ActiveColor
                    ?? Color,

                // 1: Hovered
                1 => HoveredColor
                    ?? Color,

                // 0: Idle / Normal
                _ => Color
            };
        }
    }

    [PublicAPI]
    public static class ColorSetExtensions {
        public static MappedState<GraphicState, Color> MapColorSet(this IState<GraphicState> state, ColorSet set) {
            return state.Map(set.GetColor);
        }
    }
}