using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive {
    /// <summary>
    /// A component that can control the layout inside it.
    /// </summary>
    [PublicAPI]
    public class Layout : ReactiveComponent, ILayoutDriver {
        #region Layout Controller

        public ILayoutController? LayoutController {
            get {
                if (!_layoutController.HasValue && ComponentDefaults.LayoutController.TryCreate(out var controller)) {
                    LayoutController = controller;
                }

                return _layoutController.Value;
            }
            set {
                var oldController = _layoutController.Value;

                if (oldController != null) {
                    oldController.RemoveAllChildren();
                    oldController.LayoutControllerUpdatedEvent -= ScheduleLayoutRecalculation;
                }

                _layoutController = Optional<ILayoutController?>.Some(value);

                if (value != null) {
                    value.LayoutControllerUpdatedEvent += ScheduleLayoutRecalculation;
                }

                ClaimLayoutContext(value);

                for (var i = 0; i < _childrenOrdered.Count; i++) {
                    var child = _childrenOrdered[i];

                    if (value != null && IsIncluded(child)) {
                        value.InsertChild(child, value.ChildCount);
                    }
                }

                ScheduleLayoutRecalculation();
            }
        }

        private Optional<ILayoutController?> _layoutController;

        public new void ScheduleLayoutRecalculation() {
            base.ScheduleLayoutRecalculation();
        }

        private static bool IsIncluded(ILayoutItem item) {
            return item is { WithinLayout: true, IgnoreLayout: false };
        }

        private int GetControllerIndex(ILayoutItem item, ILayoutController controller) {
            var index = 0;

            for (var i = 0; i < _childrenOrdered.Count; i++) {
                var child = _childrenOrdered[i];

                if (child.EqualsToLayoutItem(item)) {
                    break;
                }

                if (controller.HasChild(child)) {
                    index++;
                }
            }

            return index;
        }

        #endregion

        #region Children

        /// <summary>
        /// Represents the children of the component. This collection is observable and can be safely used for children modification.
        /// </summary>
        public ICollection<ILayoutItem> Children { get; private set; } = null!;

        private LayoutSet _children = new();
        private List<ILayoutItem> _childrenOrdered = new();

        private void AppendChildInternal(ILayoutItem item) {
            AppendPhysicalChild(item);

            var layoutController = LayoutController;

            item.AssignParent(this);
            _childrenOrdered.Add(item);

            if (layoutController != null && IsIncluded(item)) {
                layoutController.InsertChild(item, GetControllerIndex(item, layoutController));
            }

            item.LayoutDataUpdatedEvent += HandleChildLayoutDataUpdated;

            ScheduleLayoutRecalculation();
            OnChildrenUpdated();
        }

        private void TruncateChildInternal(ILayoutItem item) {
            TruncatePhysicalChild(item);

            item.LayoutDataUpdatedEvent -= HandleChildLayoutDataUpdated;
            _childrenOrdered.RemoveAt(_childrenOrdered.FindLayoutItemIndex(item));

            LayoutController?.RemoveChild(item);
            item.AssignParent(null);

            ScheduleLayoutRecalculation();
            OnChildrenUpdated();
        }

        private void TruncateAllChildrenInternal(IEnumerable<ILayoutItem> items) {
            foreach (var item in items) {
                TruncateChildInternal(item);
            }
        }

        private void HandleChildLayoutDataUpdated(ILayoutItem item) {
            var layoutController = LayoutController;
            if (layoutController == null) {
                return;
            }

            var hasChild = layoutController.HasChild(item);

            if (IsIncluded(item)) {
                if (hasChild) {
                    layoutController.UpdateChild(item);
                } else {
                    layoutController.InsertChild(item, GetControllerIndex(item, layoutController));
                }
            } else if (hasChild) {
                layoutController.RemoveChild(item);
            }

            ScheduleLayoutRecalculation();
        }

        protected virtual void OnChildrenUpdated() { }

        #endregion

        #region Physical Children

        /// <summary>
        /// Determines a physical container for the children to be reparented to.
        /// </summary>
        protected virtual Transform PhysicalContainer => ContentTransform;

        protected virtual void AppendPhysicalChild(ILayoutItem item) {
            if (item is IReactiveComponent comp) {
                comp.Use(PhysicalContainer);
            } else {
                item.AttachToParent(PhysicalContainer);
            }
        }

        protected virtual void TruncatePhysicalChild(ILayoutItem item) {
            if (item is IReactiveComponent comp) {
                comp.Use(null);
            } else {
                item.AttachToParent(null);
            }
        }

        #endregion

        #region Construct

        protected override void OnInstantiate() {
            Children = new ObservableCollectionAdapter<ILayoutItem>(
                _children,
                AppendChildInternal,
                TruncateChildInternal,
                TruncateAllChildrenInternal
            );
        }

        protected override void OnInitialize() {
            PromoteLayoutComponent(this);
        }

        #endregion

        #region Layout Component

        protected override Vector2 PromotedMeasure(float width, MeasureMode widthMode, float height, MeasureMode heightMode) {
            var layoutController = _layoutController.Value;
            if (layoutController == null) {
                return base.PromotedMeasure(width, widthMode, height, heightMode);
            }

            return layoutController.Measure(width, widthMode, height, heightMode);
        }

        protected override void PromotedRecalculateLayout() {
            var layoutController = LayoutController;
            if (layoutController == null || Children.Count == 0) {
                return;
            }

            Vector2 constraints;
            if (ContentTransform.parent is RectTransform parent) {
                constraints = parent.rect.size;
            } else {
                constraints = new Vector2(float.NaN, float.NaN);
            }

            layoutController.Recalculate(constraints);
            LayoutState.RecalculatedFor = ReactiveScheduler.Layout.CurrentRun;

            ApplyLayout(layoutController.GetSelfDimensions());
        }

        protected override void PromotedApplyLayout(LayoutDimensions dimensions) {
            var layoutController = LayoutController;
            if (layoutController == null || _childrenOrdered.Count == 0) {
                return;
            }

            ref var state = ref LayoutState;
            var run = ReactiveScheduler.Layout.CurrentRun;

            // Roots are recalculated before being applied, the rest use the size given by the parent
            if (run == 0 || state.RecalculatedFor != run) {
                // With stitched hierarchies controller has a shared context and can omit recalculation
                layoutController.Recalculate(new Vector2(dimensions.Width, dimensions.Height));
                state.RecalculatedFor = run;
            }

            for (var i = 0; i < _childrenOrdered.Count; i++) {
                var child = _childrenOrdered[i];

                if (layoutController.HasChild(child)) {
                    var childDimensions = layoutController.GetChildDimensions(child);

                    child.ApplyLayout(childDimensions);
                }
            }
        }

        #endregion
    }
}