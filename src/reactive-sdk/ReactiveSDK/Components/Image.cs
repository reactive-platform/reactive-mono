using System;
using JetBrains.Annotations;
using Reactive.Yoga;
using UnityEngine;

namespace Reactive.Components.Basic {
    [PublicAPI]
    public class Image : ReactiveComponent, IComponentHolder<Image>, ILeafLayoutItem, IGraphic {
        public Sprite? Sprite {
            get => _image.sprite;
            set {
                _image.sprite = value;
                NotifyPropertyChanged();
            }
        }

        public Color Color {
            get => _image.color;
            set {
                _image.color = value;
                NotifyPropertyChanged();
            }
        }

        public Material? Material {
            get => _image.material;
            set {
                _image.material = value;
                NotifyPropertyChanged();
            }
        }

        public bool PreserveAspect {
            get => _image.preserveAspect;
            set {
                _image.preserveAspect = value;
                NotifyPropertyChanged();
            }
        }

        public UnityEngine.UI.Image.Type ImageType {
            get => _image.type;
            set {
                _image.type = value;
                NotifyPropertyChanged();
            }
        }

        public UnityEngine.UI.Image.FillMethod FillMethod {
            get => _image.fillMethod;
            set {
                _image.fillMethod = value;
                NotifyPropertyChanged();
            }
        }

        public float FillAmount {
            get => _image.fillAmount;
            set {
                _image.fillAmount = value;
                NotifyPropertyChanged();
            }
        }

        public float PixelsPerUnit {
            get => _image.pixelsPerUnitMultiplier;
            set {
                ImageType = UnityEngine.UI.Image.Type.Sliced;
                _image.pixelsPerUnitMultiplier = value;
                NotifyPropertyChanged();
            }
        }

        public bool RaycastTarget {
            get => _image.raycastTarget;
            set => _image.raycastTarget = value;
        }

        Image IComponentHolder<Image>.Component => this;
        
        private UnityEngine.UI.Image _image = null!;

        protected override void Construct(RectTransform rect) {
            _image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
        }

        public event Action<ILeafLayoutItem>? LeafLayoutUpdatedEvent;
        
        public Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode) {
            var nativeSize = _image.sprite.rect.size;

            // Scale to match Image's aspect ratio settings
            var aspectRatio = nativeSize.x / nativeSize.y;
            var measuredWidth = widthMode == MeasureMode.Undefined ? nativeSize.x : width;
            var measuredHeight = heightMode == MeasureMode.Undefined ? nativeSize.y : height;

            if (_image.preserveAspect) {
                if (widthMode == MeasureMode.Exactly) {
                    measuredHeight = measuredWidth / aspectRatio;
                } else if (heightMode == MeasureMode.Exactly) {
                    measuredWidth = measuredHeight * aspectRatio;
                } else {
                    measuredWidth = Mathf.Min(measuredWidth, nativeSize.x);
                    measuredHeight = measuredWidth / aspectRatio;
                }
            }

            return new() {
                x = widthMode == MeasureMode.Exactly ? width : Mathf.Min(measuredWidth, width),
                y = heightMode == MeasureMode.Exactly ? height : Mathf.Min(measuredHeight, height)
            };
        }
    }
}