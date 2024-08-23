using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.EventSystems;

namespace Reactive.Components {
    [PublicAPI]
    public class ButtonBase : DrivingReactiveComponentBase, IClickableComponent, IStatedComponent {
        #region UI Properties

        public bool Interactable {
            get => _interactable;
            set {
                if (value == _interactable) return;
                _interactable = value;
                SetStateEnabled(NonInteractableState, !value);
                OnInteractableChange(value);
                NotifyPropertyChanged();
            }
        }

        public bool Latching {
            get => _latching;
            set {
                if (value == _latching) return;
                _latching = value;
                NotifyPropertyChanged();
            }
        }

        public bool Active {
            get => _active;
            set {
                if (value == _active) return;
                _active = value;
                SetStateEnabled(ActiveState, value);
                NotifyPropertyChanged();
                StateChangedEvent?.Invoke(value);
            }
        }

        public Action? ClickEvent { get; set; }
        public Action<bool>? StateChangedEvent { get; set; }

        event Action? IClickableComponent.ClickEvent {
            add => ClickEvent += value;
            remove => ClickEvent -= value;
        }

        event Action<bool>? IStatedComponent.StateChangedEvent {
            add => StateChangedEvent += value;
            remove => StateChangedEvent -= value;
        }

        private bool _interactable;
        private bool _latching;
        private bool _active;

        #endregion

        #region States

        public static ComponentState ActiveState = "active";
        public static ComponentState NonInteractableState = "non-interactable";

        protected override IEnumerable<ComponentState> ExtraStates { get; } = new[] {
            ActiveState,
            NonInteractableState
        };

        #endregion

        #region Button

        /// <summary>
        /// Emulates UI button click.
        /// </summary>
        /// <param name="state">Determines the toggle state. Valid only if <c>Sticky</c> is turned on</param>
        /// <param name="notifyListeners">Determines should event be invoked or not</param>
        /// <param name="force">Determines should the state be changed or not even if it is the same</param>
        public void Click(bool state = false, bool notifyListeners = false, bool force = false) {
            if (!_interactable) return;
            if (_latching) {
                if (!force && state == Active) return;
                Active = state;
            } else {
                Active = true;
            }
            HandleButtonClick(notifyListeners);
            if (!_latching) {
                Active = false;
            }
        }

        private void HandleButtonClick(bool notifyListeners) {
            OnButtonStateChange(Active);
            if (notifyListeners) {
                NotifyPropertyChanged(nameof(Active));
                ClickEvent?.Invoke();
            }
        }

        protected virtual void OnButtonStateChange(bool state) { }
        protected virtual void OnInteractableChange(bool interactable) { }

        #endregion

        #region Callbacks

        protected override void OnPointerDown(PointerEventData data) {
            Active = !_latching || !_active;
            HandleButtonClick(true);
        }

        protected override void OnPointerUp(PointerEventData data) {
            if (!_latching) {
                Active = false;
            }
        }

        #endregion
    }
}