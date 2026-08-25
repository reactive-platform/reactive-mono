using JetBrains.Annotations;
using Reactive.BeatSaber.Components;
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

        public static readonly BsButtonColors BsButtonColors = new() {
            BackgroundColors = new() {
                Colors = new() {
                    Color = Color.black with { a = 0.5f },
                    ActiveColor = Color.white with { a = 0.5f },
                    HoveredColor = Color.white with { a = 0.3f },
                    NotInteractableColor = Color.black with { a = 0.25f },
                },
                GradientColors0 = ColorSet.White,
                GradientColors1 = new() {
                    Color = Color.white,
                    HoveredColor = Color.white with { a = 0.5f },
                }
            },
            ContentColors = new() {
                Color = Color.white with { a = 0.75f },
                HoveredColor = Color.white,
            },
            UnderlineColors = new() {
                Color = Color.white with { a = 0.5f }
            }
        };

        public static readonly BsPrimaryButtonColors BsPrimaryButtonColors = new() {
            BackgroundColors = new() {
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
            },
            ContentColors = new() {
                Color = Color.white with { a = 0.75f },
                HoveredColor = Color.white,
            },
            BorderColors = new() {
                Color = new(0f, 0.75f, 1f, 0.7f)
            },
            OutlineColors = new() {
                Color = new(0f, 0.75f, 1f, 0.3f)
            }
        };

        public static readonly BsAeroButtonColors BsAeroButtonColors = new() {
            BackgroundColors = new() {
                Colors = new() {
                    Color = Color.black with { a = 0.5f },
                    HoveredColor = new(0.99f, 0.15f, 0.88f),
                },
                GradientColors0 = ColorSet.White,
                GradientColors1 = new() {
                    Color = Color.white,
                    HoveredColor = Color.white with { a = 0.5f },
                }
            },
            ContentColors = new() {
                Color = Color.white with { a = 0.75f },
                HoveredColor = Color.white,
            }
        };

        public static readonly BsSliderColors BsSliderColors = new() {
            BackgroundColors = new() {
                Color = Color.black.ColorWithAlpha(0.5f),
                HoveredColor = Color.magenta.ColorWithAlpha(0.5f),
                NotInteractableColor = Color.black.ColorWithAlpha(0.2f)
            },
            LeftButtonColors = new() {
                Colors = new() {
                    Color = Color.black.ColorWithAlpha(0.5f),
                    HoveredColor = Color.white.ColorWithAlpha(0.2f),
                    NotInteractableColor = Color.black.ColorWithAlpha(0.2f),
                },
                GradientColors0 = new() {
                    Color = Color.white,
                    HoveredColor = Color.white
                },
                GradientColors1 = new() {
                    Color = Color.white,
                    HoveredColor = Color.white.ColorWithAlpha(0.5f)
                }
            },
            RightButtonColors = new() {
                Colors = new() {
                    Color = Color.black.ColorWithAlpha(0.5f),
                    HoveredColor = Color.white.ColorWithAlpha(0.2f),
                    NotInteractableColor = Color.black.ColorWithAlpha(0.2f),
                },
                GradientColors0 = new() {
                    Color = Color.white,
                    HoveredColor = Color.white.ColorWithAlpha(0.5f)
                },
                GradientColors1 = new() {
                    Color = Color.white,
                    HoveredColor = Color.white
                }
            },
            HandleColors = new() {
                Color = Color.white.ColorWithAlpha(0.75f),
            },
            TextColors = new() {
                Color = Color.white
            }
        };

        public static readonly BsInputFieldColors BsInputFieldColors = new() {
            ContentColors = ColorSet.White,
            IconColors = ColorSet.White,
            PlaceholderColors = new() {
                Color = Color.white with { a = 0.25f },
                HoveredColor = Color.white with { a = 0.8f }
            },
            UnderlineColors = new() {
                Color = Color.white with { a = 0.25f }
            }
        };

        public static class BsKeyboard {
            public static readonly CompositeColorSet KeyBackgroundColors = new() {
                Colors = ColorSet.White,
                GradientColors1 = new() {
                    HoveredColor = new(0f, 0.75f, 1f)
                }
            };

            public static readonly ColorSet KeyBorderColors = new() {
                Color = Color.white with { a = 0.25f },
                HoveredColor = Color.white
            };

            public static readonly ColorSet KeyContentColors = new() {
                Color = Color.white with { a = 0.9f },
                HoveredColor = Color.white
            };
        }

        public static class BsDropdown {
            public static readonly ColorSet ItemBackgroundColors = new() {
                HoveredColor = Color.white with { a = 0.2f },
                ActiveColor = Color.white with { a = 0.5f },
            };

            public static readonly ColorSet ItemContentColors = new() {
                Color = Color.white with { a = 0.9f },
                HoveredColor = Color.white,
                ActiveColor = new(0f, 0.75f, 1f)
            };

            public static readonly ColorSet ButtonBackgroundColors = new() {
                Color = Color.black with { a = 0.5f },
                HoveredColor = Color.white with { a = 0.3f },
            };
        }

        public static readonly BsToggleColors BsToggleColors = new() {
            BackgroundColors = new() {
                Color = Color.black with { a = 0.5f },
                HoveredColor = Color.white with { a = 0.3f },
            },

            KnobColors = new() {
                Color = Color.black with { a = 0.5f },
                ActiveColor = new(0f, 0.75f, 1f),
            },

            TextColors = new() {
                Color = Color.white.ColorWithAlpha(0.2f),
                ActiveColor = Color.white
            }
        };

        public static readonly Color TextColor = Color.white;
        public static readonly Color SecondaryTextColor = Color.white * 0.9f;
    }
}