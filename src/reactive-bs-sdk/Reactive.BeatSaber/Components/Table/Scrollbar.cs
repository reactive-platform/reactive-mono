using System;
using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using static Reactive.Components.ScrollUpdateType;

namespace Reactive.BeatSaber.Components {
    public enum ScrollbarScrollMode {
        Page,
        Line,
        Fixed
    }

    /// <summary>
    /// Scrollbar for ReactiveComponent lists
    /// </summary>
    [PublicAPI]
    public partial class Scrollbar : ReactiveComponent {
        #region Props

        [Required, RawState]
        public ScrollContext ScrollContext {
            get => _scrollContext!;
            set {
                if (_scrollContext != null) {
                    _scrollContext.ValueChangedEvent -= HandleContextUpdated;
                }

                _scrollContext = value;
                _scrollContext.ValueChangedEvent += HandleContextUpdated;

                HandleContextUpdated(value);
            }
        }

        /// <summary>
        /// How buttons should affect the target scroll controller.
        /// </summary>
        public ScrollbarScrollMode ScrollMode { get; set; } = ScrollbarScrollMode.Page;

        /// <summary>
        /// Determines the scroll size when <see cref="ScrollMode"/> is set to Fixed.
        /// </summary>
        public float FixedScrollSize { get; set; } = 10f;

        /// <summary>
        /// Whether the scrollbar should be hidden when a controller has nothing to scroll.
        /// </summary>
        public bool HideIfNothingToScroll {
            get => _hideIfNothingToScroll.Value;
            set => _hideIfNothingToScroll.Value = value;
        }

        // Since ScrollContext itself is a state, we cannot bind directly 
        // to it while having a null object, hence we introduce another state 
        // to hold null! value by default and initialize it only when the context is set
        // (states are lazy by default so it won't throw)
        private IMutableState<ScrollContext> _scrollContextState = null!;
        private IMutableState<bool> _hideIfNothingToScroll;
        private ScrollContext? _scrollContext;

        #endregion

        #region Logic

        private float _padding = 0.25f;

        private void RefreshHandle() {
            var pageHeight = _scrollContext?.NormalizedPageHeight ?? 1;

            var areaHeight = _handleContainerRect.rect.size.y - 2f * _padding;
            var handleHeight = pageHeight * areaHeight;

            var pos = _scrollContext?.NormalizedScrollPos ?? 0;
            var handlePos = (areaHeight - handleHeight) * pos;

            _handleRect.sizeDelta = new(0f, pageHeight * areaHeight);
            _handleRect.localPosition = new(0f, handlePos * -1f);
        }

        private void HandleContextUpdated(ScrollContext context) {
            // Update the state to reflect changes in the ui
            _scrollContextState.Value = context;
            RefreshHandle();
        }

        #endregion

        #region Construct

        private RectTransform _handleContainerRect = null!;
        private RectTransform _handleRect = null!;

        [StateGen]
        private static BackgroundButton CreateButton(
            float rotation,
            Align alignItems,
            IState<bool> enabled,
            Action callback
        ) {
            var hovered = Remember(false);
            var interactable = Remember(false);

            return new BackgroundButton {
                FlexItem = {
                    FlexGrow = 1f
                },

                FlexController = {
                    JustifyContent = Justify.Center,
                    AlignItems = alignItems
                },

                Image = {
                    ContentTransform = {
                        slocalScale = hovered.Map(x => Vector3.one * (x ? 1.2f : 1f))
                    },

                    Sprite = BeatSaberResources.Sprites.transparentPixel,
                    Material = null,

                    sColor = RememberDerived(x => {
                        if (x.interactable) {
                            return Color.white.ColorWithAlpha(x.hovered ? 1f : 0.5f);
                        } else {
                            return Color.black.ColorWithAlpha(0.5f);
                        }
                    }, (hovered, interactable)),
                },

                Do = x => x
                    .WithListener(y => y.WrappedButton.Interactable, y => interactable.Value = y)
                    .WithListener(y => y.WrappedButton.IsHovered, y => hovered.Value = y),

                OnClick = callback,

                Children = {
                    new Background {
                        FlexItem = {
                            Size = 2.5f.pt
                        },

                        ContentTransform = {
                            localEulerAngles = new(0f, 0f, rotation)
                        },

                        Sprite = GameResources.ArrowIcon,
                        PreserveAspect = true,
                        Material = GameResources.UINoGlowMaterial
                    }
                }
            };
        }

        protected override GameObject Construct() {
            _scrollContextState = Remember<ScrollContext>(null!);
            _hideIfNothingToScroll = Remember(false);

            return new Layout {
                sEnabled = RememberDerived(
                    x => !x.Item2.Value || x.Item1.Value.CanScroll,
                    (
                        _scrollContextState.Where(x => x.UpdateType is ScrollCompleted or Measurements),
                        _hideIfNothingToScroll
                    )
                ),

                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    AlignItems = Align.Center
                },

                // This will be the default value for the Scrollbar
                // as layout and component properties are shared
                FlexItem = {
                    Size = new() { x = 2.pt }
                },

                // Same as for FlexItem
                WithinLayoutIfDisabled = true,

                Children = {
                    // Handle container
                    new Background {
                        FlexItem = {
                            FlexGrow = 1f,
                            Size = new() { x = 1.4f.pt },
                            Margin = new() { top = 4f, bottom = 4f }
                        },

                        ContentTransform = {
                            pivot = new(0.5f, 1f)
                        },

                        Sprite = BeatSaberResources.Sprites.background,
                        ImageType = UnityEngine.UI.Image.Type.Sliced,
                        PixelsPerUnit = 20f,
                        Color = Color.black.ColorWithAlpha(0.5f),

                        Children = {
                            // Handle
                            new Image {
                                ContentTransform = {
                                    anchorMin = new(0f, 1f),
                                    anchorMax = new(1f, 1f),
                                    pivot = new(0.5f, 1f)
                                },

                                Sprite = GameResources.VerticalIndicatorIcon,
                                Color = Color.white.ColorWithAlpha(0.5f),
                                ImageType = UnityEngine.UI.Image.Type.Sliced
                            }.Bind(ref _handleRect)
                        }
                    }.Bind(ref _handleContainerRect),

                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column
                        },

                        Children = {
                            // Up button
                            CreateButton(
                                rotation: 180f,
                                alignItems: Align.FlexStart,
                                enabled: _scrollContextState
                                    .Where(x => x.UpdateType is Intent)
                                    .Map(x => x.CanScrollUp),
                                callback: () => {
                                    switch (ScrollMode) {
                                        case ScrollbarScrollMode.Page:
                                            ScrollContext.PageUp();
                                            break;

                                        case ScrollbarScrollMode.Line:
                                            ScrollContext.LineUp();
                                            break;

                                        case ScrollbarScrollMode.Fixed:
                                            ScrollContext.ScrollRelative(FixedScrollSize);
                                            break;
                                    }
                                }),

                            // Down button
                            CreateButton(
                                rotation: 0f,
                                alignItems: Align.FlexEnd,
                                enabled: _scrollContextState
                                    .Where(x => x.UpdateType is Intent)
                                    .Map(x => x.CanScrollDown),
                                callback: () => {
                                    switch (ScrollMode) {
                                        case ScrollbarScrollMode.Page:
                                            ScrollContext.PageDown();
                                            break;

                                        case ScrollbarScrollMode.Line:
                                            ScrollContext.LineDown();
                                            break;

                                        case ScrollbarScrollMode.Fixed:
                                            ScrollContext.ScrollRelative(-FixedScrollSize);
                                            break;
                                    }
                                })
                        }
                    }.WithRectExpand()
                }
            }.Use();
        }

        protected override void OnInitialize() {
            RefreshHandle();
        }

        #endregion
    }
}