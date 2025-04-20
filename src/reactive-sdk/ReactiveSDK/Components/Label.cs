using JetBrains.Annotations;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.Components.Basic {
    [PublicAPI]
    public class Label : ReactiveComponent, IComponentHolder<Label>, ILeafLayoutItem {
        public string Text {
            get => _text.text;
            set {
                _text.text = value;
                NotifyPropertyChanged();
            }
        }

        public bool RichText {
            get => _text.richText;
            set {
                _text.richText = value;
                NotifyPropertyChanged();
            }
        }

        public float FontSize {
            get => _text.fontSize;
            set {
                _text.fontSize = value;
                NotifyPropertyChanged();
            }
        }

        public float FontSizeMin {
            get => _text.fontSizeMin;
            set {
                _text.fontSizeMin = value;
                NotifyPropertyChanged();
            }
        }

        public float FontSizeMax {
            get => _text.fontSizeMax;
            set {
                _text.fontSizeMax = value;
                NotifyPropertyChanged();
            }
        }

        public bool EnableAutoSizing {
            get => _text.enableAutoSizing;
            set {
                _text.enableAutoSizing = value;
                NotifyPropertyChanged();
            }
        }

        public FontStyles FontStyle {
            get => _text.fontStyle;
            set {
                _text.fontStyle = value;
                NotifyPropertyChanged();
            }
        }

        public TMP_FontAsset Font {
            get => _text.font;
            set {
                _text.font = value;
                NotifyPropertyChanged();
            }
        }

        public Material Material {
            get => _text.material;
            set {
                _text.material = value;
                NotifyPropertyChanged();
            }
        }

        public bool EnableWrapping {
            get => _text.enableWordWrapping;
            set {
                _text.enableWordWrapping = value;
                NotifyPropertyChanged();
            }
        }

        public TextOverflowModes Overflow {
            get => _text.overflowMode;
            set {
                _text.overflowMode = value;
                NotifyPropertyChanged();
            }
        }

        public TextAlignmentOptions Alignment {
            get => _text.alignment;
            set {
                _text.alignment = value;
                NotifyPropertyChanged();
            }
        }

        public Color Color {
            get => _text.color;
            set {
                _text.color = value;
                NotifyPropertyChanged();
            }
        }

        Label IComponentHolder<Label>.Component => this;

        private TextMeshProUGUI _text = null!;

        protected override void Construct(RectTransform rect) {
            _text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            _text.RegisterDirtyLayoutCallback(ScheduleLayoutRecalculation);
        }

        protected override void OnInitialize() {
            FontSize = 4f;
            Alignment = TextAlignmentOptions.Center;
            EnableWrapping = false;
        }

        protected override void OnStart() {
            ScheduleLayoutRecalculation();
        }

        public Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode) {
            var measuredWidth = widthMode == MeasureMode.Undefined ? Mathf.Infinity : width;
            var measuredHeight = heightMode == MeasureMode.Undefined ? Mathf.Infinity : height;

            // Set the preferred width, forcing recalculation
            _text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, measuredWidth);
            _text.ForceMeshUpdate();

            var textSize = _text.GetPreferredValues(measuredWidth, measuredHeight);

            return new() {
                x = widthMode == MeasureMode.Exactly ? width : Mathf.Min(textSize.x, measuredWidth),
                y = heightMode == MeasureMode.Exactly ? height : Mathf.Min(textSize.y, measuredHeight)
            };
        }

    }
}