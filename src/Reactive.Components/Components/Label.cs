using System;
using JetBrains.Annotations;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace Reactive.Components.Basic {
    [PublicAPI]
    public class Label : ReactiveComponent, IComponentHolder<Label> {
        public string Text {
            get => _text.text;
            set => _text.text = value;
        }

        public bool RichText {
            get => _text.richText;
            set => _text.richText = value;
        }

        public float FontSize {
            get => _text.fontSize;
            set => _text.fontSize = value;
        }

        public float FontSizeMin {
            get => _text.fontSizeMin;
            set => _text.fontSizeMin = value;
        }

        public float FontSizeMax {
            get => _text.fontSizeMax;
            set => _text.fontSizeMax = value;
        }

        public bool EnableAutoSizing {
            get => _text.enableAutoSizing;
            set => _text.enableAutoSizing = value;
        }

        public FontStyles FontStyle {
            get => _text.fontStyle;
            set => _text.fontStyle = value;
        }

        public TMP_FontAsset Font {
            get => _text.font;
            set => _text.font = value;
        }

        public Material Material {
            get => _text.material;
            set => _text.material = value;
        }

        public bool EnableWrapping {
            get => _text.enableWordWrapping;
            set => _text.enableWordWrapping = value;
        }

        public TextOverflowModes Overflow {
            get => _text.overflowMode;
            set => _text.overflowMode = value;
        }

        public TextAlignmentOptions Alignment {
            get => _text.alignment;
            set => _text.alignment = value;
        }

        public Color Color {
            get => _text.color;
            set => _text.color = value;
        }

        Label IComponentHolder<Label>.Component => this;

        private TextMeshProUGUI _text = null!;
        private Vector2 _lastPreferredSize;

        protected override void OnInstantiate() {
            PromoteLayoutComponent(this);
        }

        protected override void Construct(RectTransform rect) {
            _text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            _text.RegisterDirtyLayoutCallback(RequestLeafRecalculationOnDirty);
        }

        protected override void OnInitialize() {
            FontSize = 4f;
            Alignment = TextAlignmentOptions.Center;
            EnableWrapping = false;
        }

        protected override void OnStart() {
            NotifyLayoutDataUpdated();
        }

        protected override bool PromotedHasIntrinsicContent => true;

        protected override Vector2 PromotedMeasure(float width, MeasureMode widthMode, float height, MeasureMode heightMode) {
            _lastPreferredSize = _text.GetPreferredValues();

            return LayoutTool.MeasureNode(_lastPreferredSize, width, widthMode, height, heightMode);
        }

        // TMP also calls this when the rect gets resized, so only notifying when the size actually changes
        private void RequestLeafRecalculationOnDirty() {
            if (_text.GetPreferredValues() != _lastPreferredSize) {
                NotifyLayoutDataUpdated();
            }
        }
    }
}