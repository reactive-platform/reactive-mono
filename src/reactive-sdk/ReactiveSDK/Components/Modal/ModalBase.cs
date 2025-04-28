using System;
using JetBrains.Annotations;

namespace Reactive.Components {
    [PublicAPI]
    public interface IModal {
        event Action<IModal, bool>? ModalClosedEvent;
        event Action<IModal, bool>? ModalOpenedEvent;

        void Pause();
        void Resume();
        void Close(bool immediate);
        void Open(bool immediate);
    }

    [PublicAPI]
    public abstract class ModalBase : ReactiveComponent, IModal {
        #region Abstraction

        protected virtual bool AllowExternalClose => true;

        protected virtual void OnPause() { }
        protected virtual void OnResume() { }
        protected virtual void OnClose(bool closed) { }
        protected virtual void OnOpen(bool opened) { }

        #endregion

        #region Modal

        public ISharedAnimation? CloseAnimation {
            get => _closeAnimation;
            set {
                if (_closeAnimation != null) {
                    UnbindModule(_closeAnimation);
                }
                
                _closeAnimation = value;
                
                if (_closeAnimation != null) {
                    BindModule(_closeAnimation);
                }
            }
        }

        public ISharedAnimation? OpenAnimation {
            get => _openAnimation;
            set {
                if (_openAnimation != null) {
                    UnbindModule(_openAnimation);
                }
                
                _openAnimation = value;
                
                if (_openAnimation != null) {
                    BindModule(_openAnimation);
                }
            }
        }

        protected bool IsOpened { get; private set; }
        protected bool IsPaused { get; private set; }

        private ISharedAnimation? _closeAnimation;
        private ISharedAnimation? _openAnimation;

        public event Action<IModal, bool>? ModalClosedEvent;
        public event Action<IModal, bool>? ModalOpenedEvent;

        public void Pause() {
            if (IsPaused) return;
            IsPaused = true;
            OnPause();
        }

        public void Resume() {
            if (!IsPaused) return;
            IsPaused = false;
            OnResume();
        }

        public void Close(bool immediate) {
            if (!AllowExternalClose) return;
            CloseInternal(immediate);
        }

        public void Open(bool immediate) {
            if (IsOpened) return;
            IsOpened = true;
            OnOpen(false);
            Enabled = true;

            if (OpenAnimation != null && !immediate) {
                OpenAnimation.AnimationFinishedEvent += HandleOpenAnimationFinished;
                OpenAnimation.Play();

                ModalOpenedEvent?.Invoke(this, false);
            } else {
                ModalOpenedEvent?.Invoke(this, false);
                ModalOpenedEvent?.Invoke(this, true);
            }
        }

        protected void CloseInternal(bool immediate = false) {
            if (!IsOpened) return;
            IsOpened = false;
            OnClose(false);

            if (CloseAnimation != null && !immediate) {
                CloseAnimation.AnimationFinishedEvent += HandleCloseAnimationFinished;
                CloseAnimation.Play();

                ModalClosedEvent?.Invoke(this, false);
            } else {
                Enabled = false;

                ModalClosedEvent?.Invoke(this, false);
                ModalClosedEvent?.Invoke(this, true);
            }
        }

        protected override void OnInitialize() {
            Enabled = false;
        }

        #endregion

        #region Callbacks

        private void HandleOpenAnimationFinished() {
            OpenAnimation!.AnimationFinishedEvent -= HandleOpenAnimationFinished;
            ModalOpenedEvent?.Invoke(this, true);
        }

        private void HandleCloseAnimationFinished() {
            Enabled = false;
            CloseAnimation!.AnimationFinishedEvent -= HandleCloseAnimationFinished;
            ModalClosedEvent?.Invoke(this, true);
        }

        #endregion
    }
}