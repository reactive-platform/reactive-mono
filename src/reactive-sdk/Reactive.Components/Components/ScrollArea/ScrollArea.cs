using System;
using JetBrains.Annotations;
using Reactive.Compiler;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Reactive.Components.Basic {
    public enum ScrollOrientation {
        Vertical = 1,
        Horizontal = 0
    }

    [PublicAPI]
    public partial class ScrollArea : ReactiveComponent {
        #region Props

        [Required, RawState]
        public ScrollContext ScrollContext {
            get => _scrollContext!;
            set {
                _scrollContext?.ValueChangedEvent -= HandleContextUpdated;
                _scrollContext = value;

                var didInitialUpdate = DoInitialUpdate();
                if (!didInitialUpdate && _scrollContent != null) {
                    RefreshMeasurements();
                }

                _scrollContext.ValueChangedEvent += HandleContextUpdated;
                HandleContextUpdated(_scrollContext);
            }
        }

        [Required]
        public IReactiveComponent ScrollContent {
            get => _scrollContent!;
            set {
                _scrollContent?.Use(null);
                _scrollContent = value;

                _scrollContent.Use(_viewport);
                _contentTransform = _scrollContent.ContentTransform;

                DoInitialUpdate();
            }
        }

        public ScrollOrientation ScrollOrientation {
            get;
            set {
                field = value;
                ReloadContent();
            }
        } = ScrollOrientation.Vertical;

        public float LineSize { get; set; }
        public Func<ScrollContext, float>? FinalizeScroll { get; set; }

        private IReactiveComponent? _scrollContent;
        private RectTransform? _contentTransform;
        private ScrollContext? _scrollContext;
        private bool _needsInitialUpdate = true;

        private bool DoInitialUpdate() {
            if (_needsInitialUpdate && _scrollContext != null && _scrollContent != null) {
                ReloadContent();
                _needsInitialUpdate = false;
            }

            return !_needsInitialUpdate;
        }

        #endregion

        #region Setup

        private float _prevContentSize;
        private float _lastScrollDeltaTime;

        protected override void OnUpdate() {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (_lastScrollDeltaTime != -1f && _lastScrollDeltaTime != Time.deltaTime) {
                FinalizeScrollPos();
                _lastScrollDeltaTime = -1f;
            }

            if (_scrollContent != null) {
                var contentSize = Translate(_contentTransform!.rect);

                if (_prevContentSize != contentSize) {
                    RefreshMeasurements();
                    _prevContentSize = contentSize;
                }
            }

            UpdateContentPos(false);
        }

        private void FinalizeScrollPos() {
            if (FinalizeScroll != null) {
                _destinationPos = FinalizeScroll.Invoke(ScrollContext);
            }
        }

        #endregion

        #region Content

        private bool _posSet;
        private float _destinationPos;

        private void ReloadContent() {
            if (ScrollOrientation is ScrollOrientation.Vertical) {
                _contentTransform!.anchorMin = new(0f, 0f);
                _contentTransform.anchorMax = new(1f, 0f);
                _contentTransform.sizeDelta = new(0f, _contentTransform.sizeDelta.y);
                _contentTransform.pivot = new(1f, 1f);
            } else {
                _contentTransform!.anchorMin = new(0f, 0f);
                _contentTransform.anchorMax = new(0f, 1f);
                _contentTransform.sizeDelta = new(_contentTransform.sizeDelta.x, 0f);
                _contentTransform.pivot = new(1f, 1f);
            }

            RefreshMeasurements();
        }

        private void SetDestinationPos(float pos, bool immediately) {
            if (Mathf.Approximately(_destinationPos, pos)) {
                return;
            }

            RefreshContentSizeIfNeeded();

            _destinationPos = Mathf.Clamp(pos, 0f, ScrollContext.MaxScrollPos);
            _posSet = false;

            if (immediately) {
                FinalizeScrollPos();
                UpdateContentPos(true);
                
                _lastScrollDeltaTime = -1f;
            }
        }

        private void UpdateContentPos(bool immediately) {
            if (_posSet) {
                return;
            }

            // Dynamically checking the content size to reflect changes
            RefreshContentSizeIfNeeded();

            // Calculating pos
            var sourcePos = (Vector2)_contentTransform!.localPosition;
            var destinationPos = Translate(_destinationPos);

            if (!immediately) {
                destinationPos = Vector2.Lerp(sourcePos, destinationPos, Time.deltaTime * 4f);
            }

            var translatedDestinationPos = Translate(destinationPos);

            // Returning if equal
            if (sourcePos == destinationPos) {
                _posSet = true;
                ScrollContext.ControllerSetScrollPos(translatedDestinationPos);
                ScrollContext.ControllerNotifyScrollCompleted();
                return;
            }

            _contentTransform.localPosition = destinationPos;

            ScrollContext.ControllerSetScrollPos(translatedDestinationPos);
        }

        private void RefreshMeasurements() {
            var contentSize = Translate(_contentTransform!.rect);
            var viewSize = Translate(_viewport.rect);

            ScrollContext.ControllerSetMeasurements(contentSize, viewSize, LineSize);
        }

        private void RefreshContentSizeIfNeeded() {
            if (Translate(_contentTransform!.rect) != ScrollContext.ContentSize) {
                RefreshMeasurements();
            }
        }

        #endregion

        #region Translation

        private Vector2 Translate(float value) {
            return ScrollOrientation switch {
                ScrollOrientation.Vertical => new Vector2(0f, value),
                ScrollOrientation.Horizontal => new Vector2(value, 0f),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private float Translate(Vector2 vector) {
            return vector[(int)ScrollOrientation];
        }

        private float Translate(Rect rect) {
            return Translate(rect.size);
        }

        #endregion

        #region Construct

        private PointerEventsHandler _pointerEventsHandler = null!;
        private RectTransform _viewport = null!;

        protected override GameObject Construct() {
            // Container
            return new Background {
                    ContentTransform = {
                        pivot = Vector2.one
                    },

                    Sprite = ReactiveResources.TransparentPixel,
                }
                .WithNativeComponent(out RectMask2D _)
                .WithNativeComponent(out _pointerEventsHandler)
                .With(_ => _pointerEventsHandler.PointerScrollEvent += HandlePointerScroll)
                .Bind(ref _viewport)
                .Use();
        }

        protected override void OnRectDimensionsChanged() {
            // It's assumed that ScrollContext is initialized here because it's a Required prop
            RefreshMeasurements();
            UpdateContentPos(true);
        }

        #endregion

        #region Callbacks

        private void HandlePointerScroll(PointerEventsHandler handler, PointerEventData eventData) {
            var neg = ScrollOrientation is ScrollOrientation.Vertical ? -1 : 1;
            var destinationPos = _destinationPos + eventData.scrollDelta.y * LineSize * neg;

            _lastScrollDeltaTime = Time.deltaTime;

            // Joystick input is considered a user intent
            ScrollContext.ScrollTo(destinationPos, false);
        }

        private void HandleContextUpdated(ScrollContext context) {
            if (context.UpdateType is ScrollUpdateType.Intent) {
                SetDestinationPos(context.ScrollPos, context.Immediately);
            }
        }

        #endregion
    }
}