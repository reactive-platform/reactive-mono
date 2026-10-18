using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components {
    [PublicAPI]
    public record struct BsColorPickerColors(
        ColorSet IconColors,
        ColorSet SampleColors
    );

    /// <summary>
    /// A color picker component.
    /// </summary>
    [PublicAPI]
    public partial class BsColorPicker : ReactiveComponent, ISkewedComponent, IInteractableComponent {
        #region Public API

        [RawState, Required]
        public BsColorPickerContext Context {
            get;
            set {
                _contextSubscription?.RemoveCallback();

                field = value;
                _contextSubscription = field.AddCallback(HandleContextUpdated);

                HandleContextUpdated(field);
            }
        }

        public float Skew {
            get => _skew.Value;
            set => _skew.Value = value;
        }

        public bool Interactable {
            get => _graphicState.IsInteractable;
            set => _graphicState.IsInteractable = value;
        }

        public BsColorPickerColors Colors {
            get => _colors.Value;
            set => _colors.Value = value;
        }

        private StateSubscription? _contextSubscription;

        private void HandleContextUpdated(BsColorPickerContext context) {
            var color = context.Focused ? context.Color : context.SelectedColor;

            _sampleColor.Value = color with { a = 1f };
        }

        #endregion

        #region Construct

        private State<Color> _sampleColor = null!;
        private State<float> _skew = null!;
        private State<GraphicState> _graphicState = null!;
        private State<BsColorPickerColors> _colors = null!;

        protected override GameObject Construct() {
            _sampleColor = Remember(Color.white);
            _skew = Remember(0f);
            _graphicState = Remember(GraphicState.None);
            _colors = Remember(BeatSaberStyle.BsColorPickerColors);

            var iconColor = _graphicState.MapColorSet(_colors, x => x.IconColors);
            var sampleTint = _graphicState.MapColorSet(_colors, x => x.SampleColors);
            
            var sampleColor = RememberDerived(
                x => x.color.Value * x.tint.Value,
                (color: _sampleColor, tint: sampleTint)
            );

            return new BsAeroButton {
                FlexController = {
                    AlignItems = Align.Stretch,
                    Padding = 0.pt
                },

                sSkew = _skew,
                sInteractable = _graphicState.Map(x => x.IsInteractable),

                OnClick = () => {
                    Context.Focused = true;
                },

                ConstructContent = (_, skew) => new Layout {
                    FlexController = {
                        JustifyContent = Justify.FlexStart,
                        Padding = new() { left = 2.pt, right = 2.pt },
                        Gap = 1.pt
                    },

                    FlexItem = {
                        MinSize = new() { x = 9.pt }
                    },

                    Children = {
                        new Image {
                            FlexItem = {
                                Size = new() { x = 4.pt, y = YogaValue.Auto }
                            },

                            Name = "Icon",
                            sSkew = skew.In(),

                            Sprite = GameResources.EditIcon,
                            PreserveAspect = true,
                            Skew = BeatSaberStyle.Skew,
                            sColor = iconColor.In()
                        },

                        new Image {
                            FlexItem = {
                                Size = new() { x = 4.pt, y = YogaValue.Auto }
                            },

                            Name = "Sample",

                            sSkew = skew.In(),
                            sColor = sampleColor.In(),

                            Sprite = GameResources.CircleIcon,
                            PreserveAspect = true
                        },
                    }
                }
            }.AsFlexItem().Use();
        }

        #endregion
    }
}