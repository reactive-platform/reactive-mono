using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Reactive.Compiler;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Reactive {
    [PublicAPI]
    public partial class ReactiveComponent : IReactiveComponent {
        #region Factory

        [UsedImplicitly]
        private ReactiveComponent(int _) { }

        protected ReactiveComponent(bool constructImmediate) {
            if (constructImmediate) {
                ConstructAndInit();
            }
        }

        public ReactiveComponent() {
            ConstructAndInit();
        }

        private static ConstructorInfo? _lazyConstructor;
        private static readonly object[] dummyParams = { 0 };

        public static T LazyComponent<T>() where T : ReactiveComponent, new() {
            _lazyConstructor ??= typeof(ReactiveComponent).GetConstructor(
                ReflectionUtils.DefaultFlags,
                null,
                new[] { typeof(int) },
                null
            );
            return (T)_lazyConstructor!.Invoke(dummyParams);
        }

        #endregion

        #region UI Props

        /// <summary>
        /// Gets or sets state of the transform.
        /// </summary>
        public bool Enabled {
            get => Host.Enabled;
            set => Host.Enabled = value;
        }

        /// <summary>
        /// Gets or sets name of the content game object.
        /// </summary>
        public string Name {
            get => Content.name;
            set => Content.name = value;
        }

        #endregion

        #region Layout Item

        public ILayoutDriver? LayoutDriver {
            get => Host.LayoutDriver;
        }

        public object? LayoutContext => Host.LayoutContext;

        public ref LayoutNodeState LayoutState => ref Host.LayoutState;

        public void RecalculateLayout() {
            Host.RecalculateLayout();
        }

        public ILayoutModifier? LayoutModifier {
            get => Host.LayoutModifier;
            set => Host.LayoutModifier = value;
        }

        public bool IgnoreLayout {
            get => Host.IgnoreLayout;
            set => Host.IgnoreLayout = value;
        }

        public bool WithinLayout => Host.WithinLayout;

        public bool WithinLayoutIfDisabled {
            get => _reactiveHost!.WithinLayoutIfDisabled;
            set => _reactiveHost!.WithinLayoutIfDisabled = value;
        }

        public event Action<ILayoutItem>? LayoutDataUpdatedEvent {
            add => Host.LayoutDataUpdatedEvent += value;
            remove => Host.LayoutDataUpdatedEvent -= value;
        }

        public event Action<ILayoutItem>? LayoutAppliedEvent {
            add => Host.LayoutAppliedEvent += value;
            remove => Host.LayoutAppliedEvent -= value;
        }

        public void AssignParent(ILayoutDriver? driver) {
            Host.AssignParent(driver);
        }

        public void AssignContext(object? context) {
            Host.AssignContext(context);
        }

        public void AttachToParent(Transform? parent) {
            Host.AttachToParent(parent);
        }

        public void ApplyLayout(LayoutDimensions dimensions) {
            Host.ApplyLayout(dimensions);
        }

        Vector2 ILayoutMeasurable.Measure(float width, MeasureMode widthMode, float height, MeasureMode heightMode) {
            return Host.Measure(width, widthMode, height, heightMode);
        }

        public int GetLayoutItemHashCode() {
            return Host.GetLayoutItemHashCode();
        }

        public bool EqualsToLayoutItem(ILayoutItem item) {
            return Host.EqualsToLayoutItem(item);
        }

        /// <summary>
        /// Notifies the driver that values affecting the layout have changed, e.g. the measured size.
        /// </summary>
        protected void NotifyLayoutDataUpdated() {
            Host.NotifyLayoutDataUpdated();
        }

        protected void ScheduleLayoutRecalculation() {
            Host.ScheduleLayoutRecalculation();
        }

        #endregion

        #region Layout Promotion

        private bool _promotedBeforeHost;

        /// <summary>
        /// Claims the own layout context of the host for the specified member. If the context assigned by the parent
        /// has the same type, it's claimed instead of creating a new one. Available for the layout component only.
        /// </summary>
        /// <param name="member">A member to claim the context for, or null to release the claimed one.</param>
        protected void ClaimLayoutContext(IContextMember? member) => Host.ClaimLayoutContext(this, member);

        /// <summary>
        /// Promotes the specified component to the layout component of the host. The host proxies measuring
        /// and layout application to it, and only it can claim the own layout context.
        /// Only one component can be promoted, so the previous one gets demoted.
        /// </summary>
        /// <param name="component">A component to promote, or null to demote the current one.</param>
        protected void PromoteLayoutComponent(ReactiveComponent? component) {
            if (_reactiveHost == null) {
                _promotedBeforeHost = component == this;
                return;
            }

            Host.PromoteLayoutComponent(component);
        }

        /// <summary>
        /// Called on the layout component when the host is measured.
        /// </summary>
        protected virtual Vector2 PromotedMeasure(float width, MeasureMode widthMode, float height, MeasureMode heightMode) {
            return LayoutTool.MeasureNode(Vector2.zero, width, widthMode, height, heightMode);
        }

        /// <summary>
        /// Called on the layout component after the host has applied the dimensions to itself.
        /// The layout component should apply only the dimensions of its children.
        /// </summary>
        protected virtual void PromotedApplyLayout(LayoutDimensions dimensions) { }

        /// <summary>
        /// Called on the layout component when the host is the root of a hierarchy that needs recalculation.
        /// The layout component should recalculate and apply the hierarchy going down from the host.
        /// </summary>
        protected virtual void PromotedRecalculateLayout() { }

        #endregion

        #region Coroutines

        /// <summary>
        /// Starts a coroutine on the <see cref="ReactiveHost"/> instance.
        /// </summary>
        /// <param name="coroutine">The coroutine to start.</param>
        protected void StartCoroutine(IEnumerator coroutine) {
            _reactiveHost!.StartCoroutine(coroutine);
        }

        /// <summary>
        /// Stops a coroutine on the <see cref="ReactiveHost"/> instance.
        /// </summary>
        /// <param name="coroutine">The coroutine to stop.</param>
        protected void StopCoroutine(IEnumerator coroutine) {
            _reactiveHost!.StopCoroutine(coroutine);
        }

        /// <summary>
        /// Stops all coroutines on the <see cref="ReactiveHost"/> instance.
        /// </summary>
        protected void StopAllCoroutines() {
            _reactiveHost!.StopAllCoroutines();
        }

        #endregion

        #region Modules

        IReadOnlyCollection<IReactiveModule> IReactiveModuleBinder.Modules => Host.Modules;

        private HashSet<IReactiveModule>? _modules;

        public void BindModule(IReactiveModule module) {
            if (LoadModules()) {
                _modules!.Add(module);
            } else {
                Host.BindModule(module);
            }
        }

        public void UnbindModule(IReactiveModule module) {
            if (LoadModules()) {
                _modules!.Remove(module);
            } else {
                Host.UnbindModule(module);
            }
        }

        private bool LoadModules() {
            if (!IsInitialized && _modules == null) {
                _modules = HashSetPool<IReactiveModule>.Get();
            }
            return !IsInitialized;
        }

        private void TransferModules() {
            if (_modules == null) return;
            foreach (var module in _modules) {
                Host.BindModule(module);
            }
            HashSetPool<IReactiveModule>.Release(_modules);
        }

        #endregion

        #region Construct

        public RectTransform ContentTransform => _contentTransform ?? throw new UninitializedComponentException();
        public GameObject Content => _content ?? throw new UninitializedComponentException();

        public bool IsInitialized { get; private set; }
        public bool IsDestroyed { get; private set; }
        bool ILifetimeProvider.IsAlive => !IsDestroyed;

        protected Canvas? Canvas {
            get {
                if (!_canvas) {
                    _canvas = Content.GetComponentInParent<Canvas>();
                }
                return _canvas;
            }
        }

        private ReactiveHost Host => _reactiveHost ?? throw new UninitializedComponentException();

        private Canvas? _canvas;
        private GameObject? _content;
        private RectTransform? _contentTransform;
        private ReactiveHost? _reactiveHost;

        /// <summary>
        /// Constructs and reparents the component if needed.
        /// </summary>
        public GameObject Use(Transform? parent = null) {
            if (IsDestroyed) {
                throw new InvalidOperationException("Unable to manage the object since it's destroyed");
            }

            if (!IsInitialized) {
                ConstructAndInit();
            }

            Host.SetLayoutDriver(null);
            ContentTransform.SetParent(parent, false);

            return Content;
        }

        protected void ConstructAndInit() {
            if (IsInitialized) {
                throw new InvalidOperationException();
            }

            OnInstantiate();

            _content = Construct();
            _content.name = GetType().Name;
            _contentTransform = _content.GetOrAddComponent<RectTransform>();

            _reactiveHost = _content.GetOrAddComponent<ReactiveHost>();
            IsInitialized = true;
            _reactiveHost.AddComponent(this);

            if (_promotedBeforeHost) {
                _reactiveHost.PromoteLayoutComponent(this);
            }

            TransferModules();

            OnInitialize();
        }

        [StateGen]
        protected virtual void Construct(RectTransform rect) { }

        [StateGen]
        protected virtual GameObject Construct() {
            var go = new GameObject();
            var rect = go.AddComponent<RectTransform>();
            Construct(rect);
            return go;
        }

        #endregion

        #region Destroy

        /// <summary>
        /// Destroys the component.
        /// </summary>
        public void Destroy() {
            Object.Destroy(Content);
            DestroyInternal();
        }

        private void DestroyInternal() {
            IsDestroyed = true;
            OnDestroy();
        }

        #endregion

        #region Events

        protected virtual void OnInstantiate() { }
        protected virtual void OnInitialize() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
        protected virtual void OnStart() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected virtual void OnRectDimensionsChanged() { }
        protected virtual void OnLayoutApply() { }

        #endregion
    }
}