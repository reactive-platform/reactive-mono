using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reactive.Components {
    [PublicAPI]
    public abstract class ButtonBase : ReactiveComponent, IComponentHolder<ButtonBase>, IInteractableComponent {
        #region UI Properties

        ButtonBase IComponentHolder<ButtonBase>.Component => this;

        public bool Interactable {
            get => _interactable;
            set {
                if (value == _interactable) return;
                _interactable = value;
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
            private set {
                if (value == _active) return;
                _active = value;
                NotifyPropertyChanged();
                OnButtonStateChange();
                OnStateChanged?.Invoke(value);
            }
        }

        public bool RaycastTarget {
            get => _pointerEventsHandler.enabled;
            set => _pointerEventsHandler.enabled = value;
        }

        public bool IsHovered {
            get => _isHovered;
            private set {
                _isHovered = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsPressed {
            get => _isPressed;
            private set {
                _isPressed = value;
                NotifyPropertyChanged();
            }
        }

        public Action? OnClick { get; set; }
        public Action<bool>? OnStateChanged { get; set; }

        private bool _isPressed;
        private bool _isHovered;
        private bool _interactable = true;
        private bool _latching;
        private bool _active;

        #endregion

        #region Button

        /// <summary>
        /// Emulates UI button click.
        /// </summary>
        /// <param name="state">Determines the toggle state. Valid only if <c>Sticky</c> is turned on</param>
        /// <param name="notifyListeners">Determines should event be invoked or not</param>
        /// <param name="force">Determines should the state be changed or not even if it is the same</param>
        public void Click(bool state = false, bool notifyListeners = false, bool force = false) {
            if (!Interactable) return;
            if (Latching) {
                if (!force && state == _active) return;
                _active = state;
            }
            HandleButtonClick(notifyListeners);
        }

        private void HandleButtonClick(bool notifyListeners) {
            OnButtonStateChange();
            if (!notifyListeners) return;
            OnClick?.Invoke();
            if (Latching) {
                NotifyPropertyChanged(nameof(Active));
                OnStateChanged?.Invoke(Latching ? Active : default);
            }
        }

        protected virtual void OnButtonStateChange() { }
        protected virtual void OnInteractableChange(bool interactable) { }

        #endregion

        #region Setup

        private PointerEventsHandler _pointerEventsHandler = null!;

        protected override void Construct(RectTransform rectTransform) {
            _pointerEventsHandler = rectTransform.gameObject.AddComponent<PointerEventsHandler>();
            _pointerEventsHandler.PointerDownEvent += OnPointerDown;
            _pointerEventsHandler.PointerUpEvent += OnPointerUp;
            _pointerEventsHandler.PointerEnterEvent += OnPointerEnter;
            _pointerEventsHandler.PointerExitEvent += OnPointerExit;
        }

        protected override void OnDisable() {
            OnPointerExit(null!, null!);
        }

        #endregion

        #region Callbacks

        private void OnPointerExit(PointerEventsHandler _, PointerEventData data) {
            if (!Interactable && !IsHovered) return;
            IsHovered = false;
            IsPressed = false;
            OnButtonStateChange();
        }

        private void OnPointerEnter(PointerEventsHandler _, PointerEventData data) {
            if (!Interactable) return;
            IsHovered = true;
            OnButtonStateChange();
        }

        private void OnPointerDown(PointerEventsHandler _, PointerEventData data) {
            if (!Interactable) return;
            if (Latching) {
                _active = !_active;
            }
            IsPressed = true;
            HandleButtonClick(true);
        }

        private void OnPointerUp(PointerEventsHandler _, PointerEventData data) {
            if (!Interactable && !IsPressed) return;
            IsPressed = false;
            OnButtonStateChange();
        }

        #endregion
    }
}