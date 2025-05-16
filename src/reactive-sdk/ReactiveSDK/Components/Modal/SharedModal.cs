using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public interface ISharedModal : IModal {
        /// <summary>
        /// Invoked before the Open method is called. Passes the original modal as an argument.
        /// </summary>
        event Action<IModal>? BeforeModalOpenedEvent;
    }

    [PublicAPI]
    public class SharedModal<T> : ISharedModal where T : ModalBase, new() {
        #region Pool

        public T Modal => _modal ?? throw new InvalidOperationException();

        public event Action<IModal>? BeforeModalOpenedEvent;

        private static readonly ReactivePool<T> modals = new();
        private T? _modal;

        public void BuildImmediate() {
            modals.Preload(1);
        }

        private void SpawnModal() {
            if (_modal != null) {
                return;
            }

            _modal = modals.Spawn();
            _modal.Enabled = false;

            _modal.ModalClosedEvent += HandleModalClosed;
            _modal.ModalOpenedEvent += HandleModalOpened;

            OnSpawn();
        }

        private void DespawnModal() {
            _modal!.ModalClosedEvent -= HandleModalClosed;
            _modal.ModalOpenedEvent -= HandleModalOpened;

            OnDespawn();

            modals.Despawn(_modal);
            _modal = null;
        }

        protected virtual void OnSpawn() { }
        protected virtual void OnDespawn() { }

        #endregion

        #region Modal Adapter

        public event Action<IModal, bool>? ModalClosedEvent;
        public event Action<IModal, bool>? ModalOpenedEvent;
        public event Action<IModal, float>? OpenProgressChangedEvent;

        public ISharedAnimation? OpenAnimation { get; set; }
        public ISharedAnimation? CloseAnimation { get; set; }
        
        public void Pause() {
            Modal.Pause();
        }

        public void Resume() {
            Modal.Resume();
        }

        public virtual void Close(bool immediate) {
            Modal.Close(immediate);
        }

        public void Open(bool immediate) {
            SpawnModal();
            
            Modal.OpenAnimation = OpenAnimation;
            Modal.CloseAnimation = CloseAnimation;
            
            BeforeModalOpenedEvent?.Invoke(Modal);
            Modal.Open(immediate);
        }

        protected virtual void OnOpenInternal(bool finished) { }
        protected virtual void OnCloseInternal(bool finished) { }

        #endregion

        #region ReactiveComponent Adapter

        public GameObject Content => Modal.Content;

        public RectTransform ContentTransform => Modal.ContentTransform;

        public bool IsDestroyed => Modal.IsDestroyed;

        public bool IsInitialized => Modal.IsInitialized;

        public bool Enabled {
            get => Modal.Enabled;
            set => Modal.Enabled = value;
        }

        public GameObject Use(Transform? parent) {
            SpawnModal();
            return Modal.Use(parent);
        }

        #endregion

        #region LayoutItem

        public ILayoutDriver? LayoutDriver {
            get => null;
            set { }
        }

        public ILayoutModifier? LayoutModifier {
            get => null;
            set { }
        }

        public bool WithinLayout { get; set; }

        public event Action<ILayoutItem>? ModifierUpdatedEvent;
        public event Action<ILayoutItem>? StateUpdatedEvent;

        public int GetLayoutItemHashCode() {
            return GetHashCode();
        }

        public bool EqualsToLayoutItem(ILayoutItem item) {
            return false;
        }

        public void RecalculateLayoutImmediate() {
            ((ILayoutRecalculationSource)Modal).RecalculateLayoutImmediate();
        }

        public void ScheduleLayoutRecalculation() {
            ((ILayoutRecalculationSource)Modal).ScheduleLayoutRecalculation();
        }

        public RectTransform BeginApply() {
            throw new NotImplementedException();
        }

        public void EndApply() {
            throw new NotImplementedException();
        }

        #endregion

        #region Callbacks

        private void HandleModalClosed(IModal modal, bool finished) {
            OnCloseInternal(finished);
            if (finished) DespawnModal();
            ModalClosedEvent?.Invoke(this, finished);
        }

        private void HandleModalOpened(IModal modal, bool finished) {
            OnOpenInternal(finished);
            ModalOpenedEvent?.Invoke(this, finished);
        }

        private void HandleOpenProgressChanged(IModal modal, float progress) {
            OpenProgressChangedEvent?.Invoke(this, progress);
        }

        #endregion
    }
}