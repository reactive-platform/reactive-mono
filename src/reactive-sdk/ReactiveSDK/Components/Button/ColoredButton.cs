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

        public GraphicState GraphicState => GraphicState.None
            .AddIf(GraphicState.NonInteractable, !Interactable)
            .AddIf(GraphicState.Hovered, Interactable && IsHovered)
            .AddIf(GraphicState.Pressed, Interactable && IsPressed)
            .AddIf(GraphicState.Active, Interactable && Active);
        
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