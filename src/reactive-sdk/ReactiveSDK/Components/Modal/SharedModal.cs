using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Components {
    [PublicAPI]
    public interface ISharedModal : IModal;

    [PublicAPI]
    public class SharedModal<T> : ISharedModal, IReactiveComponent where T : class, IModal, IReactiveComponent, new() {
        #region Pool

        public T Modal => _modal ?? throw new InvalidOperationException();

        private static readonly ReactivePool<T> modals = new();
        private T? _modal;

        public void BuildImmediate() {
            modals.Preload(1);
        }

        private void SpawnModal() {
            if (_modal != null) return;
            _modal = modals.Spawn();
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