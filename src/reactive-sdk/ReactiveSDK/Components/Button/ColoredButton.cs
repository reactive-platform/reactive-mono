using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public class ColoredButton : ButtonBase {
        #region UI Properties

        public IColorSet? Colors {
            get => _stateColorSet;
            set {
                if (_stateColorSet != null) {
                    _stateColorSet.SetUpdatedEvent -= UpdateColor;
                }
                _stateColorSet = value;
                if (_stateColorSet != null) {
                    _stateColorSet.SetUpdatedEvent += UpdateColor;
                }
                UpdateColor();
                NotifyPropertyChanged();
            }
        }

        private IColorSet? _stateColorSet;

        #endregion

        #region Color

        protected void UpdateColor() {
            ApplyColor(GetColor(Colors));
        }

        protected Color GetColor(IColorSet? colorSet) {
            if (colorSet == null) {
                return Color.clear;
            }
            var state = new GraphicState {
                Active = Active,
                Interactable = Interactable,
                Hovered = IsHovered
            };
            return colorSet.GetColor(state);
        }


        protected override void OnButtonStateChange() {
            UpdateColor();
        }

        protected virtual void ApplyColor(Color color) { }

        #endregion
    }
}