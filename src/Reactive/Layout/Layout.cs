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
            get => _layoutController;
            set {
                var oldController = _layoutController;

                if (oldController != null) {
                    oldController.RemoveAllChildren();
                    oldController.LayoutControllerUpdatedEvent -= HandleLayoutControllerUpdated;
                }

                _layoutController = value;

                if (value != null) {
                    value.LayoutControllerUpdatedEvent += HandleLayoutControllerUpdated;
                }

                ClaimLayoutContext(value);
                PromotedOnModifierUpdate();

                for (var i = 0; i < _childrenOrdered.Count; i++) {
                    var child = _childrenOrdered[i];

                    if (value != null && child.WithinLayout) {
                        value.InsertChild(child, value.ChildCount);
                    }
                }

                ScheduleLayoutRecalculation();
            }
        }

        private ILayoutController? _layoutController;

        public new void ScheduleLayoutRecalculation() {
            base.ScheduleLayoutRecalculation();
        }

        private void HandleLayoutControllerUpdated() {
            ScheduleLayoutRecalculation();
            NotifyMeasuringParent();
        }
        
        private void NotifyMeasuringParent() {
            if (!SharesParentLayoutContext) {
                NotifyLayoutDataUpdated();
            }
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

            if (layoutController != null && item.WithinLayout) {
                layoutController.InsertChild(item, layoutController.ChildCount);
            }

            item.LayoutDataUpdatedEvent += HandleChildLayoutDataUpdated;

            ScheduleLayoutRecalculation();
            NotifyMeasuringParent();
            OnChildrenUpdated();
        }

        private void TruncateChildInternal(ILayoutItem item) {
            TruncatePhysicalChild(item);

            item.LayoutDataUpdatedEvent -= HandleChildLayoutDataUpdated;
            _childrenOrdered.RemoveAt(_childrenOrdered.FindLayoutItemIndex(item));

            LayoutController?.RemoveChild(item);
            item.AssignParent(null);

            ScheduleLayoutRecalculation();
            NotifyMeasuringParent();
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

            if (item.WithinLayout) {
                if (hasChild) {
                    layoutController.UpdateChild(item);
                } else {
                    layoutController.InsertChild(item, GetControllerIndex(item, layoutController));
                }
            } else if (hasChild) {
                layoutController.RemoveChild(item);
            }

            ScheduleLayoutRecalculation();
            NotifyMeasuringParent();
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

        protected override bool PromotedHasIntrinsicContent => _layoutController != null;

        protected override void PromotedOnModifierUpdate() {
            if (_layoutController is { } layoutController && !LayoutTool.IsControlledByParent(this)) {
                layoutController.UpdateSelf(this);
            }
        }

        protected override Vector2 PromotedMeasure(float width, MeasureMode widthMode, float height, MeasureMode heightMode) {
            var layoutController = _layoutController;
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