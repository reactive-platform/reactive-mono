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

        protected GraphicState GraphicState => GraphicState.None
            .AddIf(GraphicState.NonInteractable, !Interactable)
            .AddIf(GraphicState.Hovered, IsHovered)
            .AddIf(GraphicState.Pressed, IsPressed)
            .AddIf(GraphicState.Active, Active);
        
        protected void UpdateColor() {
            ApplyColor(GetColor(Colors));
        }

        protected virtual Color GetColor(IColorSet? colorSet) {
            return colorSet?.GetColor(GraphicState) ?? Color.clear;
        }

        protected override void OnButtonStateChange() {
            UpdateColor();
        }

        protected virtual void ApplyColor(Color color) { }

        #endregion
    }
}