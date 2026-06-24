using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components {
    /// <summary>
    /// A Beat Saber styled button without any content.
    /// </summary>
    [PublicAPI]
    public partial class BsButton : ReactiveComponent, ILayoutDriver, ISkewedComponent, IInteractableComponent {
        public delegate ILayoutItem ConstructContentDelegate(IState<GraphicState> graphic, IState<float> skew);

        #region Public API

        public bool ShowUnderline {
            get => _underlineEnabled.Value;
            set => _underlineEnabled.Value = value;
        }

        public float Skew {
            get => _skew.Value;
            set => _skew.Value = value;
        }

        public bool Interactable {
            get => _graphicState.IsInteractable;
            set => _graphicState.IsInteractable = value;
        }

        [Required]
        public ConstructContentDelegate ConstructContent {
            init {
                var layoutItem = value(_graphicState, _skew);
                _background.Children.Add(layoutItem);
            }
        }

        public Action? OnClick { get; set; }

        #endregion

        #region Layout Driver

        public ICollection<ILayoutItem> Children { get; } = Array.Empty<ILayoutItem>();

        public ILayoutController? LayoutController {
            get => _background.LayoutController;
            set => _background.LayoutController = value;
        }

        #endregion

        #region Setup

        private State<bool> _underlineEnabled = null!;
        private State<float> _skew = null!;
        private State<GraphicState> _graphicState = null!;

        private Background _background = null!;

        protected override GameObject Construct() {
            _underlineEnabled = Remember(true);
            _skew = Remember(BeatSaberStyle.Skew);
            _graphicState = Remember(GraphicState.None);

            var bgColor = _graphicState.MapColorSet(BeatSaberStyle.BsButton.BackgroundColors);

            return new Background {
                FlexController = {
                    JustifyContent = Justify.SpaceAround,
                    Padding = new() { left = 1.pt, right = 1.pt }
                },

                FlexItem = {
                    Size = new() { y = 8.pt }
                },

                sColor = bgColor.Map(x => x.Color),
                sGradientColor0 = bgColor.Map(x => x.GradientColor0),
                sGradientColor1 = bgColor.Map(x => x.GradientColor1),

                UseGradient = true,
                Sprite = BeatSaberResources.Sprites.background,
                PixelsPerUnit = 12f,

                sSkew = _skew,

                Do = x => x.WithPointerEvents(
                    onEnter: _ => _graphicState.IsHovered = true,
                    onLeave: _ => _graphicState.IsHovered = false,
                    onDown: _ => {
                        if (_graphicState.IsInteractable) {
                            GameResources.ButtonClickSignal.Raise();
                            OnClick?.Invoke();
                        }
                    }),

                Children = {
                    new Image {
                        Name = "Underline",
                        sEnabled = _underlineEnabled,
                        sSkew = _skew,
                        
                        Sprite = BeatSaberResources.Sprites.backgroundUnderline,
                        ImageType = UnityEngine.UI.Image.Type.Sliced,
                        Color = Color.white with { a = 0.5f },
                        PixelsPerUnit = 12f,
                    }.WithRectExpand()
                }
            }.Bind(ref _background).Use();
        }

        #endregion
    }
}