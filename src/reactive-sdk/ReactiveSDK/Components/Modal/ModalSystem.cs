using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reactive.Components.Basic {
    [PublicAPI]
    public class ModalSystem : ModalSystem<ModalSystem> { }
}

namespace Reactive.Components {
    [PublicAPI]
    public class ModalSystem<T> : ReactiveComponent where T : ModalSystem<T>, new() {
        #region OpenModal

        public static void PresentModal(IModal modal, Transform screen, bool animated = true, bool interruptAll = false) {
            var modalSystem = BorrowOrInstantiateModalSystem(screen);
            PresentModalInternal(modalSystem, modal, animated, interruptAll);
        }

        private static void PresentModalInternal(ModalSystem<T> system, IModal modal, bool animated, bool interruptAll) {
            if (interruptAll) {
                InterruptAllEvent?.Invoke();
            }
            
            system.PresentModal(modal, animated);
        }

        #endregion

        #region ModalSystem Pool

        private static readonly ReactivePool<Transform, T> systemsPool = new() { DetachOnDespawn = false };

        private static T BorrowOrInstantiateModalSystem(Transform viewController) {
            var system = systemsPool.Get(viewController);
            system.Use(viewController.transform);
            system.WithRectExpand();
            return system;
        }

        private void ReleaseModalSystem() {
            systemsPool.Despawn((T)this);
        }

        #endregion

        #region Setup

        private static event Action? InterruptAllEvent;

        protected override void OnInitialize() {
            Content.AddComponent<GraphicRaycaster>();
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
            InterruptAllEvent += InterruptAll;
        }

        protected override void OnDestroy() {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            InterruptAllEvent -= InterruptAll;
            systemsPool.Despawn((T)this);
        }

        private void OnActiveSceneChanged(Scene from, Scene to) {
            InterruptAll();
        }

        private void InterruptAll() {
            if (!HasActiveModal) return;
            while (_modalStack.Count > 0) {
                CloseModal();
            }
        }

        #endregion

        #region Open & Close

        private bool HasActiveModal => _activeModal != null;

        private readonly Stack<IModal> _modalStack = new();
        private bool _firstInvocation;
        private IModal? _activeModal;

        private void PresentModal(IModal modal, bool animated) {
            // Showing modal system if needed
            if (HasActiveModal) {
                _activeModal!.Pause();
            } else {
                ShowModalSystem();
            }

            // Adding modal to the stack
            _modalStack.Push(modal);
            _activeModal = modal;
            modal.ModalClosedEvent += HandleModalClosed;

            // Showing the modal
            modal.Use(ContentTransform);
            modal.ContentTransform.SetAsLastSibling();
            modal.Open(!animated);

            RefreshBlocker();
        }

        private void CloseModal() {
            if (!HasActiveModal || _needToHideModalSystem) {
                return;
            }
            _firstInvocation = true;
            _activeModal!.Close(false);
        }

        private void CloseModalInternal() {
            if (!HasActiveModal) {
                return;
            }
            // Removing current modal
            _modalStack.Pop();

            // Setting new active modal
            if (_modalStack.Count > 0) {
                _activeModal!.ModalClosedEvent -= HandleModalClosed;
                _activeModal = _modalStack.Peek();
                _activeModal.Resume();
            } else {
                _needToHideModalSystem = true;
            }

            RefreshBlocker(0);
        }

        private void RefreshBlocker(int offset = -1) {
            Blocker.SetActive(HasActiveModal);

            if (!HasActiveModal) {
                return;
            }

            var modalIndex = _activeModal!.ContentTransform.GetSiblingIndex();
            _blockerRect.SetSiblingIndex(modalIndex + offset);
        }

        private void HandleModalClosed(IModal modal, bool finished) {
            if (_needToHideModalSystem || (finished && _firstInvocation)) {
                if (!finished) return;
                HideModalSystem();
            }

            if (_modalStack.Count == 0) {
                return;
            }

            var index = _modalStack.FindIndex(modal);
            if (index == -1) {
                return;
            }

            for (var i = 0; i < index + 1; i++) {
                CloseModalInternal();
            }
            _firstInvocation = false;
        }

        #endregion

        #region ModalSystem Open & Close

        private bool _needToHideModalSystem;

        private void ShowModalSystem() {
            ContentTransform.WithRectExpand();
            Enabled = true;
        }

        private void HideModalSystem() {
            _needToHideModalSystem = false;
            _activeModal!.ModalClosedEvent -= HandleModalClosed;
            _activeModal = null;
            ReleaseModalSystem();
        }

        #endregion

        #region Construct

        protected GameObject Blocker { get; private set; } = null!;
        protected Canvas ModalCanvas { get; private set; } = null!;

        private Button _blockerButton = null!;
        private RectTransform _blockerRect = null!;

        protected override void Construct(RectTransform rectTransform) {
            var go = rectTransform.gameObject;
            ModalCanvas = go.AddComponent<Canvas>();
            ModalCanvas.sortingOrder = 10;
            ModalCanvas.overrideSorting = true;

            Blocker = new GameObject("Blocker");
            _blockerButton = Blocker.AddComponent<Button>();
            _blockerButton.onClick.AddListener(HandleBlockerClicked);

            _blockerRect = Blocker.AddComponent<RectTransform>();
            _blockerRect.SetParent(rectTransform, false);
            _blockerRect.WithRectExpand();

            go.AddComponent<CanvasGroup>().ignoreParentGroups = true;
        }

        private void HandleBlockerClicked() {
            CloseModal();
        }

        #endregion
    }
}