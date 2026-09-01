using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components {
    /// <summary>
    /// A color picker component.
    /// </summary>
    [PublicAPI]
    public partial class ColorPicker : ReactiveComponent, ISkewedComponent {
        #region Public API

        [RawState, Required]
        public ColorPickerContext Context {
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

        private StateSubscription? _contextSubscription;

        private void HandleContextUpdated(ColorPickerContext context) {
            var color = context.Focused ? context.Color : context.SelectedColor;

            _sampleColor.Value = color with { a = 1f };
        }

        #endregion

        #region Construct

        private State<Color> _sampleColor = null!;
        private State<float> _skew = null!;

        protected override GameObject Construct() {
            _sampleColor = Remember(Color.white);
            _skew = Remember(0f);

            return new BsAeroButton {
                FlexController = {
                    AlignItems = Align.Stretch,
                    Padding = 0.pt
                },

                sSkew = _skew,

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
                            Color = BeatSaberStyle.SecondaryTextColor
                        },

                        new Image {
                            FlexItem = {
                                Size = new() { x = 4.pt, y = YogaValue.Auto }
                            },

                            Name = "Sample",

                            sSkew = skew.In(),
                            sColor = _sampleColor,

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