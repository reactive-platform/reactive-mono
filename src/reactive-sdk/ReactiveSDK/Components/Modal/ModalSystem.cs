using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Reactive.Components {
    [PublicAPI]
    public class ModalSystem : ReactiveComponent {
        #region OpenModal

        public static void PresentModal<T>(
            T modal,
            Transform screen,
            bool animated = true,
            bool interruptAll = false
        ) where T : IModal, IReactiveComponent {
            var modalSystem = BorrowOrInstantiateModalSystem(screen);
            PresentModalInternal(modalSystem, modal, animated, interruptAll);
        }

        private static void PresentModalInternal<T>(
            ModalSystem system,
            T modal,
            bool animated,
            bool interruptAll
        ) where T : IModal, IReactiveComponent {
            if (interruptAll) {
                InterruptAllEvent?.Invoke();
            }
            system.PresentModal(modal, animated);
        }

        #endregion

        #region ModalSystem Pool

        private static readonly ReactivePool<Transform, ModalSystem> systemsPool = new() { DetachOnDespawn = false };

        private static ModalSystem BorrowOrInstantiateModalSystem(Transform viewController) {
            var system = systemsPool.Get(viewController);
            system.Use(viewController.transform);
            return system;
        }

        private void ReleaseModalSystem() {
            systemsPool.Despawn(this);
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
            systemsPool.Despawn(this);
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

        private IReactiveComponent? ReactiveActiveModal => _activeModal as IReactiveComponent;
        private bool HasActiveModal => _activeModal != null;

        private readonly Stack<IModal> _modalStack = new();
        private bool _firstInvocation;
        private IModal? _activeModal;

        private void PresentModal<T>(T modal, bool animated) where T : IModal, IReactiveComponent {
            //showing modal system if needed
            if (HasActiveModal) {
                _activeModal!.Pause();
            } else {
                ShowModalSystem();
            }
            //adding modal to the stack
            _modalStack.Push(modal);
            _activeModal = modal;
            modal.ModalClosedEvent += HandleModalClosed;
            //showing the modal
            modal.Use(ContentTransform);
            modal.ContentTransform.SetAsLastSibling();
            modal.Open(!animated);
            RefreshBlocker();
        }

        private void CloseModal() {
            if (!HasActiveModal || _needToHideModalSystem) return;
            _firstInvocation = true;
            _activeModal!.Close(false);
        }

        private void CloseModalInternal() {
            if (!HasActiveModal) return;
            //removing current modal
            _modalStack.Pop();
            //setting new active modal
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
            _blockerGo.SetActive(HasActiveModal);
            if (!HasActiveModal) return;
            var modalIndex = ReactiveActiveModal!.ContentTransform.GetSiblingIndex();
            _blocker.SetSiblingIndex(modalIndex + offset);
        }

        private void HandleModalClosed(IModal modal, bool finished) {
            if (_needToHideModalSystem || (finished && _firstInvocation)) {
                if (!finished) return;
                HideModalSystem();
            }
            if (_modalStack.Count == 0) return;
            var index = _modalStack.FindIndex(modal);
            if (index == -1) return;
            var count = index + 1;
            for (var i = 0; i < count; i++) {
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

        private Button _blockerButton = null!;
        private GameObject _blockerGo = null!;
        private RectTransform _blocker = null!;

        protected override void Construct(RectTransform rectTransform) {
            var go = rectTransform.gameObject;
            //canvas
            var canvas = go.AddComponent<Canvas>();
            canvas.additionalShaderChannels |=
                AdditionalCanvasShaderChannels.Tangent |
                AdditionalCanvasShaderChannels.TexCoord2;
            //blocker
            _blockerGo = new GameObject("Blocker");
            _blockerButton = _blockerGo.AddComponent<Button>();
            _blocker = _blockerGo.AddComponent<RectTransform>();
            _blocker.SetParent(rectTransform, false);
            _blockerButton.onClick.AddListener(HandleBlockerClicked);
            _blocker.WithRectExpand();
        }

        private void HandleBlockerClicked() {
            CloseModal();
        }

        #endregion
    }
}