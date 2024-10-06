using System;
using JetBrains.Annotations;
using UnityEngine;

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
    public abstract class ModalComponentBase : ReactiveComponent, IModal {
        #region Abstraction

        protected virtual bool AllowExternalClose => true;

        protected virtual void OnPause() { }
        protected virtual void OnResume() { }
        protected virtual void OnClose(bool closed) { }
        protected virtual void OnOpen(bool opened) { }

        #endregion

        #region Modal

        public IObjectAnimator<ModalComponentBase>? CloseAnimator { get; set; }
        public IObjectAnimator<ModalComponentBase>? OpenAnimator { get; set; }

        protected bool IsOpened { get; private set; }
        protected bool IsPaused { get; private set; }

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
            if (OpenAnimator != null && !immediate) {
                OpenAnimator.AnimationFinishedEvent += HandleOpenAnimationFinished;
                OpenAnimator.StartAnimation(this, false);
                ModalOpenedEvent?.Invoke(this, false);
            } else {
                ModalOpenedEvent?.Invoke(this, true);
            }
        }

        protected void CloseInternal(bool immediate = false) {
            if (!IsOpened) return;
            IsOpened = false;
            OnClose(false);
            if (CloseAnimator != null && !immediate) {
                CloseAnimator.AnimationFinishedEvent += HandleCloseAnimationFinished;
                CloseAnimator.StartAnimation(this, false);
                ModalClosedEvent?.Invoke(this, false);
            } else {
                Enabled = false;
                ModalClosedEvent?.Invoke(this, true);
            }
        }

        protected override void OnInitialize() {
            Enabled = false;
            OpenAnimator = Animate<ModalComponentBase>(
                (x, y) => x.ContentTransform.localScale = y * Vector3.one,
                "200ms"
            );
            CloseAnimator = Animate<ModalComponentBase>(
                (x, y) => x.ContentTransform.localScale = (1f - y) * Vector3.one,
                "200ms"
            );
        }

        #endregion

        #region Callbacks

        private void HandleOpenAnimationFinished() {
            OpenAnimator!.AnimationFinishedEvent -= HandleOpenAnimationFinished;
            ModalOpenedEvent?.Invoke(this, true);
        }

        private void HandleCloseAnimationFinished() {
            Enabled = false;
            CloseAnimator!.AnimationFinishedEvent -= HandleCloseAnimationFinished;
            ModalClosedEvent?.Invoke(this, true);
        }

        #endregion
    }
}