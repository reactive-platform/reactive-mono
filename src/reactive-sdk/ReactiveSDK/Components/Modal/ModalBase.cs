using System;
using JetBrains.Annotations;

namespace Reactive.Components {
    [PublicAPI]
    public interface IModal : IReactiveComponent {
        IState<float> OpenProgress { get; }

        event Action<IModal, bool>? ModalClosedEvent;
        event Action<IModal, bool>? ModalOpenedEvent;

        void Pause();
        void Resume();
        void Close(bool immediate);
        void Open(bool immediate);
    }

    [PublicAPI]
    public abstract class ModalBase : ReactiveComponent, IComponentHolder<ModalBase>, IModal {
        #region Abstraction

        protected virtual bool AllowExternalClose => true;

        protected virtual void OnPause() { }
        protected virtual void OnResume() { }
        protected virtual void OnClose(bool closed) { }
        protected virtual void OnOpen(bool opened) { }

        #endregion

        #region Modal

        public IState<float> OpenProgress => _openProgress;

        public AnimationDuration AnimationDuration {
            get => _openProgress.Duration;
            set => _openProgress.Duration = value;
        }

        protected bool IsOpened { get; private set; }
        protected bool IsPaused { get; private set; }

        public event Action<IModal, bool>? ModalClosedEvent;
        public event Action<IModal, bool>? ModalOpenedEvent;

        private AnimatedState<float> _openProgress = null!;

        ModalBase IComponentHolder<ModalBase>.Component => this;

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

            if (!immediate) {
                _openProgress.Value = 1f;
                _openProgress.OnFinish = HandleOpenAnimationFinished;
                ModalOpenedEvent?.Invoke(this, false);
            } else {
                _openProgress.SetValueImmediate(1f);
                ModalOpenedEvent?.Invoke(this, false);
                ModalOpenedEvent?.Invoke(this, true);
            }
        }

        protected void CloseInternal(bool immediate = false) {
            if (!IsOpened) return;
            IsOpened = false;
            OnClose(false);

            if (!immediate) {
                _openProgress.Value = 0f;
                _openProgress.OnFinish = HandleCloseAnimationFinished;
                
                ModalClosedEvent?.Invoke(this, false);
            } else {
                _openProgress.SetValueImmediate(0f);
                Enabled = false;

                ModalClosedEvent?.Invoke(this, false);
                ModalClosedEvent?.Invoke(this, true);
            }
        }

        protected override void OnInitialize() {
            Enabled = false;
            _openProgress = RememberAnimated(0f, 200.ms);
        }

        #endregion

        #region Callbacks

        private void HandleOpenAnimationFinished(float t) {
            _openProgress.OnFinish = null;
            ModalOpenedEvent?.Invoke(this, true);
        }

        private void HandleCloseAnimationFinished(float t) {
            Enabled = false;
            _openProgress.OnFinish = null;
            ModalClosedEvent?.Invoke(this, true);
        }

        #endregion
    }
}