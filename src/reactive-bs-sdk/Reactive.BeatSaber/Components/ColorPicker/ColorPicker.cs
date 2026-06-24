using JetBrains.Annotations;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.BeatSaber.Components {
    /// <summary>
    /// A color picker component.
    /// </summary>
    [PublicAPI]
    public class ColorPicker : ReactiveComponent, IComponentHolder<IModal> {
        #region UI Props

        public Color Color {
            get => _color;
            set {
                _color = value;
                _colorSampleImage.Color = value;

                if (_modalOpened) {
                    _circleModal.Modal.ColorCircle.SetColor(value, false);
                }

                NotifyPropertyChanged();
            }
        }

        public RelativePlacement CirclePlacement { get; set; } = RelativePlacement.Center;

        private Color _color;
        private bool _modalOpened;

        #endregion

        #region Construct

        IModal IComponentHolder<IModal>.Component => _circleModal;

        private SharedModal<ColorCircleModal> _circleModal = null!;
        private Image _colorSampleImage = null!;

        protected override GameObject Construct() {
            // Color circle
            new SharedModal<ColorCircleModal>()
                .WithAnchor(
                    this,
                    Lazy(() => CirclePlacement, false),
                    unbindOnceOpened: false
                )
                .WithJumpAnimation()
                .WithOpenListener(HandleModalOpened)
                .WithCloseListener(HandleModalClosed)
                .Bind(ref _circleModal);

            return new BsAeroButton {
                FlexController = {
                    AlignItems = Align.Stretch
                },

                ConstructContent = (_, skew) => new Layout {
                    FlexController = {
                        JustifyContent = Justify.FlexStart,
                        Padding = new() { left = 2.pt, top = 1.pt, right = 2.pt, bottom = 1.pt },
                        Gap = 1.pt
                    },

                    FlexItem = {
                        Flex = 1
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

                            Sprite = GameResources.CircleIcon,
                            PreserveAspect = true
                        }.Bind(ref _colorSampleImage),
                    }
                }
            }.Use();
        }

        #endregion

        #region Callbacks

        private void HandleModalOpened(IModal modal, bool finished) {
            if (finished) {
                return;
            }

            _modalOpened = true;
            _circleModal.Modal.WithListener(
                x => x.ColorCircle.SavedColor,
                HandleColorChanged
            );
            Color = _color;
        }

        private void HandleModalClosed(IModal modal, bool finished) {
            if (finished) {
                return;
            }

            _modalOpened = false;
            _circleModal.Modal.WithoutListener(
                x => x.ColorCircle.SavedColor,
                HandleColorChanged
            );
        }

        private void HandleColorChanged(Color color) {
            _colorSampleImage.Color = color;
            _color = color;
            NotifyPropertyChanged(nameof(Color));
        }

        #endregion
    }
}