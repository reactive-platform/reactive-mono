using Reactive.Components;
using UnityEngine;
using UnityEngine.EventSystems;

#if !COMPILE_EDITOR

namespace Reactive.BeatSaber.Components {
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(PointerEventsHandler))]
    internal class VRScrollAdapter : MonoBehaviour {
        // The game's lists: the sqrMagnitude check in HMUI.ScrollView.CheckScrollInput
        private const float DeadzoneSqr = 0.01f;

        private PointerEventsHandler _pointerEventsHandler = null!;
        private IVRPlatformHelper _platformHelper = null!;

        private void Awake() {
            _pointerEventsHandler = GetComponent<PointerEventsHandler>();
            var context = BeatSaberUtils.MenuContainer;
            _platformHelper = context.Resolve<IVRPlatformHelper>();
        }

        private void Update() {
            if (!_pointerEventsHandler.IsHovered) {
                return;
            }
            var delta = _platformHelper.GetAnyJoystickMaxAxis();
            if (delta.sqrMagnitude <= DeadzoneSqr) {
                return;
            }
            var pointerEventData = new PointerEventData(EventSystem.current) {
                scrollDelta = delta
            };
            _pointerEventsHandler.OnScroll(pointerEventData);
        }
    }
}

#endif