using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reactive.Components {
    [PublicAPI]
    [RequireComponent(typeof(RectTransform))]
    public class PointerEventsHandler : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IDragHandler,
        IPointerMoveHandler,
        IBeginDragHandler,
        IEndDragHandler,
        IScrollHandler {
        #region Events

        public event Action<PointerEventsHandler, PointerEventData>? PointerUpdatedEvent;

        public event Action<PointerEventsHandler, PointerEventData>? PointerDownEvent;
        public event Action<PointerEventsHandler, PointerEventData>? PointerUpEvent;

        public event Action<PointerEventsHandler, PointerEventData>? PointerEnterEvent;
        public event Action<PointerEventsHandler, PointerEventData>? PointerExitEvent;

        public event Action<PointerEventsHandler, PointerEventData>? PointerDragEvent;
        public event Action<PointerEventsHandler, PointerEventData>? PointerDragBeginEvent;
        public event Action<PointerEventsHandler, PointerEventData>? PointerDragEndEvent;
        
        public event Action<PointerEventsHandler, PointerEventData>? PointerScrollEvent;
        public event Action<PointerEventsHandler, PointerEventData>? PointerMoveEvent;

        #endregion

        #region Helpers

        public bool IsFocused => IsDragging || IsPressed || IsHovered;
        public bool IsPressed { get; private set; }
        public bool IsHovered { get; private set; }
        public bool IsDragging { get; private set; }
        public PointerEventData? EventData { get; private set; }

        private void NotifyPointerUpdated(PointerEventData data) {
            PointerUpdatedEvent?.Invoke(this, data);
        }

        #endregion

        #region Callbacks

        public void OnPointerDown(PointerEventData eventData) {
            IsPressed = true;
            EventData = eventData;
            
            PointerDownEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnPointerUp(PointerEventData eventData) {
            IsPressed = false;
            EventData = eventData;
            
            PointerUpEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            IsHovered = true;
            EventData = eventData;
            
            PointerEnterEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnPointerExit(PointerEventData eventData) {
            IsHovered = false;
            EventData = eventData;
            
            PointerExitEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnDrag(PointerEventData eventData) {
            EventData = eventData;
            
            PointerDragEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnBeginDrag(PointerEventData eventData) {
            IsDragging = true;
            EventData = eventData;
            
            PointerDragBeginEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnEndDrag(PointerEventData eventData) {
            IsDragging = false;
            EventData = eventData;
            
            PointerDragEndEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        public void OnScroll(PointerEventData eventData) {
            EventData = eventData;
            
            PointerScrollEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }
        
        public void OnPointerMove(PointerEventData eventData) {
            EventData = eventData;
            
            PointerMoveEvent?.Invoke(this, eventData);
            NotifyPointerUpdated(eventData);
        }

        #endregion
    }
}