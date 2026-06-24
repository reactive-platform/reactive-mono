using JetBrains.Annotations;
using Reactive.Components;
using UnityEngine;

namespace Reactive.BeatSaber {
    /// <summary>
    /// Provides default colors and measurements for Beat Saber components.
    /// </summary>
    [PublicAPI]
    public static class BeatSaberStyle {
        public static readonly float Skew = 0.18f;

        public static SimpleColorSet InputColorSet => new() {
            HoveredColor = Color.magenta.ColorWithAlpha(0.5f),
            Color = Color.black.ColorWithAlpha(0.5f),
            NotInteractableColor = Color.black.ColorWithAlpha(0.2f)
        };

        public static SimpleColorSet ControlColorSet => new() {
            HoveredColor = Color.white.ColorWithAlpha(0.2f),
            Color = Color.black.ColorWithAlpha(0.5f),
            NotInteractableColor = Color.black.ColorWithAlpha(0.25f)
        };

        public static SimpleColorSet ControlButtonColorSet => new() {
            ActiveColor = new(0f, 0.75f, 1f, 1f),
            HoveredColor = Color.white.ColorWithAlpha(0.2f),
            Color = Color.black.ColorWithAlpha(0.5f),
            NotInteractableColor = Color.black.ColorWithAlpha(0.35f)
        };

        public static SimpleColorSet TextColorSet => new() {
            ActiveColor = new(0f, 0.75f, 1f, 1f),
            NotInteractableColor = Color.white.ColorWithAlpha(0.2f),
            HoveredColor = Color.white * 0.9f,
            Color = Color.white
        };

        public static ColorSet CellTextColors = new() {
            Color = Color.white with { a = 0.75f },
            NotInteractableColor = Color.white with { a = 0.35f },
            HoveredColor = Color.white,
            ActiveColor = new(0f, 0.75f, 1f, 1f),
        };

        public static ColorSet CellColors = new() {
            Color = Color.black with { a = 0.5f },
            NotInteractableColor = Color.black with { a = 0.25f },
            HoveredColor = Color.white with { a = 0.2f },
            ActiveColor = Color.white with { a = 0.1f }
        };

        public static class BsButton {
            public static CompositeColorSet BackgroundColors = new() {
                Colors = new() {
                    Color = Color.black with { a = 0.5f },
                    ActiveColor = Color.white with { a = 0.5f },
                    HoveredColor = Color.white with { a = 0.3f },
                    NotInteractableColor = Color.black with { a = 0.25f },
                },
                GradientColors1 = new() {
                    Color = Color.white with { a = 0.5f },
                    HoveredColor = Color.white,
                }
            };

            public static ColorSet ContentColors = new() {
                Color = Color.white with { a = 0.75f },
                HoveredColor = Color.white,
            };
        }
        
        public static class BsPrimaryButton {
            public static CompositeColorSet BackgroundColors = new() {
                Colors = new() {
                    Color = Color.white,
                    HoveredColor = Color.white,
                },
                GradientColors0 = new() {
                    Color = new(0f, 0.5f, 1f),
                    HoveredColor = new(0f, 0.7f, 1f),
                },
                GradientColors1 = new() {
                    Color = new(0f, 0.5f, 1f, 0.5f),
                    HoveredColor = new(0f, 0.7f, 1f, 0.5f),
                }
            };
            
            public static ColorSet ContentColors = new() {
                Color = Color.white with { a = 0.75f },
                HoveredColor = Color.white,
            };

            public static readonly Color BorderColor = new(0f, 0.75f, 1f, 0.7f);
            public static readonly Color OutlineColor = new(0f, 0.75f, 1f, 0.3f);
        }

        public static readonly Color PrimaryButtonColor = new(0, 0.5f, 1f);

        public static readonly Color TextColor = Color.white;
        public static readonly Color SelectedTextColor = new(0f, 0.75f, 1f, 1f);
        public static readonly Color InactiveTextColor = Color.white.ColorWithAlpha(0.2f);
        public static readonly Color SecondaryTextColor = Color.white * 0.9f;
    }
}