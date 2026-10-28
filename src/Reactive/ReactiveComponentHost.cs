using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ReSharper disable ForCanBeConvertedToForeach
namespace Reactive {
    public partial class ReactiveComponent {
        [RequireComponent(typeof(RectTransform))]
        private class ReactiveHost : MonoBehaviour, ILayoutItem, IReactiveModuleBinder {
            #region LayoutItem

            public ILayoutDriver? LayoutDriver => _layoutDriver;

            public ref LayoutNodeState LayoutState => ref _layoutState;

            public ILayoutModifier? LayoutModifier {
                get {
                    if (!_modifier.HasValue && ComponentDefaults.LayoutModifier.TryCreate(out var modifier)) {
                        // Not the best solution but since _modifier is reassigned before calling modifier event it doesn't cause recursion cycles
                        LayoutModifier = modifier;
                    }

                    return _modifier.Value;
                }
                set {
                    var oldModifier = _modifier.Value;

                    if (oldModifier != null) {
                        oldModifier.ModifierUpdatedEvent -= HandleModifierUpdated;
                    }

                    _modifier = Optional<ILayoutModifier?>.Some(value);

                    if (value != null) {
                        value.ModifierUpdatedEvent += HandleModifierUpdated;
                    }

                    HandleModifierUpdated();
                }
            }

            public bool WithinLayout => !IsDestroyed && (gameObject.activeSelf || WithinLayoutIfDisabled);

            public bool IgnoreLayout {
                get;
                set {
                    field = value;
                    NotifyLayoutDataUpdated();
                }
            }

            public bool WithinLayoutIfDisabled {
                get;
                set {
                    field = value;
                    NotifyLayoutDataUpdated();
                }
            }

            public event Action<ILayoutItem>? LayoutDataUpdatedEvent;
            public event Action<ILayoutItem>? LayoutAppliedEvent;

            private ILayoutDriver? _layoutDriver;
            private LayoutNodeState _layoutState;
            private Optional<ILayoutModifier?> _modifier;
            private bool _beingApplied;

            public void AssignParent(ILayoutDriver? driver) {
                _layoutDriver = driver;
                _layoutState.QueuedFor = 0;
                _layoutState.HasApplied = false;

                if (driver == null) {
                    _layoutComponent?.PromotedOnModifierUpdate();
                }
            }

            private void HandleModifierUpdated() {
                NotifyLayoutDataUpdated();
                _layoutComponent?.PromotedOnModifierUpdate();
            }

            public void SetLayoutDriver(ILayoutDriver? driver) {
                if (_layoutDriver == driver) return;
                _layoutDriver?.Children.Remove(this);
                driver?.Children.Add(this);
            }

            public void AttachToParent(Transform? parent) {
                transform.SetParent(parent, false);
            }

            public void ApplyLayout(LayoutDimensions dimensions) {
                ref var state = ref _layoutState;

                if (!state.HasApplied || state.LastApplied != dimensions) {
                    _beingApplied = true;

                    try {
                        // Only the parent's controller positions the item, otherwise the position is left to the user
                        if (LayoutTool.IsControlledByParent(this)) {
                            _rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, dimensions.TopOffset, dimensions.Height);
                            _rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, dimensions.LeftOffset, dimensions.Width);
                        } else {
                            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, dimensions.Width);
                            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, dimensions.Height);
                        }
                    }
                    finally {
                        _beingApplied = false;
                    }

                    state.LastApplied = dimensions;
                    state.HasApplied = true;
                }

                _layoutComponent?.PromotedApplyLayout(dimensions);

                for (var i = 0; i < _components.Count; i++) {
                    _components[i].OnLayoutApply();
                }

                LayoutAppliedEvent?.Invoke(this);
            }

            public void RecalculateLayout() {
                if (LayoutTool.IsControlledByParent(this)) {
                    throw new InvalidOperationException(
                        "Cannot recalculate a layout controlled by its parent, schedule a recalculation instead"
                    );
                }

                _layoutComponent?.PromotedRecalculateLayout();
            }

            public Vector2 Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode) {
                if (_layoutComponent != null) {
                    return _layoutComponent.PromotedMeasure(width, widthMode, height, heightMode);
                }

                return LayoutTool.MeasureNode(Vector2.zero, width, widthMode, height, heightMode);
            }

            public int GetLayoutItemHashCode() {
                return base.GetHashCode();
            }

            public bool EqualsToLayoutItem(ILayoutItem item) {
                return item.GetLayoutItemHashCode() == GetLayoutItemHashCode();
            }

            #endregion

            #region Layout

            public void ScheduleLayoutRecalculation() {
                ReactiveScheduler.Layout.Schedule(this);
            }

            public void NotifyLayoutDataUpdated() {
                LayoutDataUpdatedEvent?.Invoke(this);
            }

            #endregion

            #region Modules

            public IReadOnlyCollection<IReactiveModule> Modules {
                get {
                    // Do not change as _modules ?? [] will produce a hashset instance instead of using Array.Empty<>
                    if (_modules == null) {
                        return [];
                    }

                    return _modules;
                }
            }

            private HashSet<IReactiveModule>? _modules;

            public void BindModule(IReactiveModule module) {
                _modules ??= new();
                _modules.Add(module);
                module.OnBind();
            }

            public void UnbindModule(IReactiveModule module) {
                _modules?.Remove(module);
                module.OnUnbind();
            }

            #endregion

            #region Contexts

            // Own context has higher priority
            public object? LayoutContext => _ownContext ?? _assignedContext;

            private object? _assignedContext;
            private object? _ownContext;
            private IContextMember? _contextMember;

            public void AssignContext(object? context) {
                _assignedContext = context;
            }

            public void ClaimLayoutContext(ReactiveComponent comp, IContextMember? member) {
                if (comp != _layoutComponent) {
                    if (member == null) return;
                    throw new InvalidOperationException("Only the layout component can claim a layout context");
                }

                ReleaseLayoutContext();

                var type = member?.ContextType;
                if (type != null) {
                    // The parent's context of the same type is claimed, so both trees share it
                    _ownContext = _assignedContext?.GetType() == type ? _assignedContext : member!.CreateContext();
                    _contextMember = member;
                    member!.ProvideContext(_ownContext);
                }

                NotifyLayoutDataUpdated();
            }

            private void ReleaseLayoutContext() {
                _contextMember?.ProvideContext(null);
                _contextMember = null;
                _ownContext = null;
            }

            #endregion

            #region Components

            public bool Enabled {
                get => gameObject.activeSelf;
                set {
                    gameObject.SetActive(value);

                    if (!gameObject.activeInHierarchy) {
                        // When turning the object off\on in a disabled hierarchy, OnEnable and OnDisable events are not called.
                        // To force layout recalculation when enabled
                        _wasActuallyDisabled = true;
                        NotifyLayoutDataUpdated();
                    }
                }
            }

            public bool IsStarted { get; private set; }
            public bool IsDestroyed { get; private set; }

            private readonly List<ReactiveComponent> _components = new();
            private ReactiveComponent? _layoutComponent;

            public void PromoteLayoutComponent(ReactiveComponent? comp) {
                if (comp != null && !_components.Contains(comp)) {
                    throw new InvalidOperationException("Cannot promote a component that does not belong to the host");
                }

                if (comp == _layoutComponent) {
                    return;
                }

                if (_contextMember != null) {
                    ReleaseLayoutContext();
                    NotifyLayoutDataUpdated();
                }

                _layoutComponent = comp;
            }

            public void AddComponent(ReactiveComponent comp) {
                _components.Add(comp);

                if (IsStarted) {
                    comp.OnStart();
                }
            }

            public void RemoveComponent(ReactiveComponent comp) {
                _components.Remove(comp);

                if (comp == _layoutComponent) {
                    PromoteLayoutComponent(null);
                }
            }

            #endregion

            #region Events

            private RectTransform _rectTransform = null!;
            private bool _wasActuallyDisabled = true;

            private void Awake() {
                _rectTransform = GetComponent<RectTransform>();
            }

            private void Start() {
                for (var i = 0; i < _components.Count; i++) {
                    _components[i].OnStart();
                }

                IsStarted = true;
            }

            private void Update() {
                for (var i = 0; i < _components.Count; i++) {
                    _components[i].OnUpdate();
                }

                if (_modules != null) {
                    foreach (var module in _modules) {
                        module.OnUpdate();
                    }
                }
            }

            private void LateUpdate() {
                for (var i = 0; i < _components.Count; i++) {
                    _components[i].OnLateUpdate();
                }
            }

            private void OnDestroy() {
                for (var i = 0; i < _components.Count; i++) {
                    _components[i].OnDestroy();
                }

                IsDestroyed = true;

                ReleaseLayoutContext();
            }

            private void OnTransformParentChanged() {
                // Reparented components (e.g. pooled ones) could end up under another canvas
                for (var i = 0; i < _components.Count; i++) {
                    _components[i]._canvas = null;
                }
            }

            private void OnEnable() {
                // Was skipped by the scheduler while inactive
                if (_layoutState.QueuedFor != 0) {
                    ScheduleLayoutRecalculation();
                }

                if (!_wasActuallyDisabled) {
                    return;
                }

                ScheduleLayoutRecalculation();
                NotifyLayoutDataUpdated();

                for (var i = 0; i < _components.Count; i++) {
                    _components[i].OnEnable();
                }
                _wasActuallyDisabled = false;
            }

            private void OnDisable() {
                if (gameObject.activeSelf) {
                    return;
                }

                NotifyLayoutDataUpdated();

                for (var i = 0; i < _components.Count; i++) {
                    _components[i].OnDisable();
                }
                _wasActuallyDisabled = true;
            }

            private void OnRectTransformDimensionsChange() {
                // if we look at how the reactive layout system works
                // we'll see that it goes to the root driver and starts recalculation from there.
                // it means that layout controller will be able to modify rect properties
                // which is not allowed to be performed on the OnRectTransformDimensionsChange stack.
                // that's why we schedule recalculation to the frame's layout layer
                if (!_beingApplied) {
                    // Changed outside the layout system, so the same dimensions have to be applied again
                    _layoutState.HasApplied = false;
                    ScheduleLayoutRecalculation();
                }

                for (var i = 0; i < _components.Count; i++) {
                    _components[i].OnRectDimensionsChanged();
                }
            }

            #endregion
        }
    }
}