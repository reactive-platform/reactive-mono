using System;
using System.Collections.Generic;
using HMUI;
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
    public partial class BsPrimaryButton : ReactiveComponent, ILayoutDriver, ISkewedComponent, IInteractableComponent {
        public delegate ILayoutItem ConstructContentDelegate(IState<GraphicState> graphic, IState<float> skew);

        #region Public API

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

        private State<float> _skew = null!;
        private State<GraphicState> _graphicState = null!;

        private Background _background = null!;

        protected override GameObject Construct() {
            _skew = Remember(BeatSaberStyle.Skew);
            _graphicState = Remember(GraphicState.None);

            var bgColor = _graphicState.MapColorSet(BeatSaberStyle.BsPrimaryButton.BackgroundColors);

            return new Background {
                FlexController = {
                    JustifyContent = Justify.SpaceAround,
                    Padding = new() { left = 1.pt, right = 1.pt }
                },

                FlexItem = {
                    Size = new() { y = 8.pt }
                },

                sColors = bgColor.In(),

                UseGradient = true,
                GradientDirection = ImageView.GradientDirection.Vertical,
                Sprite = BeatSaberResources.Sprites.background,
                Material = GameResources.AnimatedButtonMaterial,
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
                        Name = "Border",
                        Sprite = BeatSaberResources.Sprites.frame,
                        Material = GameResources.AnimatedButtonBorderMaterial,
                        Color = BeatSaberStyle.BsPrimaryButton.BorderColor,
                        PixelsPerUnit = 12f,
                        
                        sSkew = _skew,
                    }.WithRectExpand(),

                    new Image {
                        Name = "Outline",
                        
                        ContentTransform = {
                            pivot = Vector2.one * 0.5f,
                            anchorMin = Vector2.zero,
                            anchorMax = Vector2.one,
                            sizeDelta = Vector2.one
                        },

                        sSkew = _skew,
                        sEnabled = _graphicState.Map(x => x.IsHovered),
                        Color = BeatSaberStyle.BsPrimaryButton.OutlineColor,

                        Sprite = BeatSaberResources.Sprites.frame,
                        Material = GameResources.AnimatedButtonBorderMaterial,
                        PixelsPerUnit = 12f,
                    }
                }
            }.Bind(ref _background).Use();
        }

        #endregion
    }
}