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
            GraphicState state = 0;
            if (!Interactable) {
                state |= GraphicState.NonInteractable;
            }
            if (IsHovered) {
                state |= GraphicState.Hovered;
            }
            if (IsPressed) {
                state |= GraphicState.Pressed;
            }
            if (Active) {
                state |= GraphicState.Active;
            }
            return colorSet.GetColor(state);
        }

        protected override void OnButtonStateChange() {
            UpdateColor();
        }

        protected virtual void ApplyColor(Color color) { }

        #endregion
    }
}