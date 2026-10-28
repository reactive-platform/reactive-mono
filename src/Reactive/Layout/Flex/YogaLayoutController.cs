using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Reactive.Yoga {
    [PublicAPI]
    public class YogaLayoutController : ILayoutController {
        #region Properties

        public Overflow Overflow {
            get => HasValidNode ? YogaNode.StyleGetOverflow() : _overflow.GetValueOrDefault();
            set {
                _overflow = value;
                _changedCacheBeforeInit = true;

                if (HasValidNode) {
                    YogaNode.StyleSetOverflow(_overflow.Value);
                    NotifyControllerUpdated();
                }
            }
        }

        public Direction Direction {
            get => HasValidNode ? YogaNode.StyleGetDirection() : _direction.GetValueOrDefault();
            set {
                _direction = value;
                _changedCacheBeforeInit = true;

                if (HasValidNode) {
                    YogaNode.StyleSetDirection(_direction.Value);
                    NotifyControllerUpdated();
                }
            }
        }

        public FlexDirection FlexDirection {
            get => HasValidNode ? YogaNode.StyleGetFlexDirection() : _flexDirection.GetValueOrDefault();
            set {
                _flexDirection = value;
                _changedCacheBeforeInit = true;

                if (HasValidNode) {
                    YogaNode.StyleSetFlexDirection(_flexDirection.Value);
                    NotifyControllerUpdated();
                }
            }
        }

        public Wrap FlexWrap {
            get => HasValidNode ? YogaNode.StyleGetFlexWrap() : _flexWrap.GetValueOrDefault();
            set {
                _flexWrap = value;
                _changedCacheBeforeInit = true;

                if (HasValidNode) {
                    YogaNode.StyleSetFlexWrap(_flexWrap.Value);
                    NotifyControllerUpdated();
                }
            }
        }

        public Justify JustifyContent {
            get => HasValidNode ? YogaNode.StyleGetJustifyContent() : _justifyContent.GetValueOrDefault();
            set {
                _justifyContent = value;
                _changedCacheBeforeInit = true;

                if (HasValidNode) {
                    YogaNode.StyleSetJustifyContent(_justifyContent.Value);
                    NotifyControllerUpdated();
                }
            }
        }

        public Align AlignItems {
            get => HasValidNode ? YogaNode.StyleGetAlignItems() : _alignItems.GetValueOrDefault();
            set {
                _alignItems = value;
                _changedCacheBeforeInit = true;

                if (HasValidNode) {
                    YogaNode.StyleSetAlignItems(_alignItems.Value);
                    NotifyControllerUpdated();
                }
            }
        }

        public Align AlignContent {
            get => HasValidNode ? YogaNode.StyleGetAlignContent() : _alignContent.GetValueOrDefault();
            set {
                _alignContent = value;
                _changedCacheBeforeInit = true;

                if (HasValidNode) {
                    YogaNode.StyleSetAlignContent(_alignContent.Value);
                    NotifyControllerUpdated();
                }
            }
        }

        public YogaFrame Padding {
            get {
                if (HasValidNode) {
                    return new() {
                        top = YogaNode.StyleGetPadding(Edge.Top),
                        bottom = YogaNode.StyleGetPadding(Edge.Bottom),
                        left = YogaNode.StyleGetPadding(Edge.Left),
                        right = YogaNode.StyleGetPadding(Edge.Right)
                    };
                }

                return _padding.GetValueOrDefault();
            }
            set {
                _padding = value;
                _changedCacheBeforeInit = true;

                if (HasValidNode) {
                    RefreshPadding(value);
                    NotifyControllerUpdated();
                }
            }
        }

        public YogaVector Gap {
            get {
                if (HasValidNode) {
                    return new() {
                        x = YogaNode.StyleGetGap(Gutter.Column),
                        y = YogaNode.StyleGetGap(Gutter.Row)
                    };
                }

                return _gap.GetValueOrDefault();
            }
            set {
                _gap = value;
                _changedCacheBeforeInit = true;

                if (HasValidNode) {
                    RefreshGap(value);
                    NotifyControllerUpdated();
                }
            }
        }

        public bool ConstrainVertical {
            get => _constrainVertical;
            set {
                _constrainVertical = value;

                if (HasValidNode) {
                    NotifyControllerUpdated();
                }
            }
        }

        public bool ConstrainHorizontal {
            get => _constrainHorizontal;
            set {
                _constrainHorizontal = value;

                if (HasValidNode) {
                    NotifyControllerUpdated();
                }
            }
        }

        public bool HadOverflow => HasValidNode && YogaNode.HadOverflow();

        public event Action? LayoutControllerUpdatedEvent;

        private Overflow? _overflow;

        private Direction? _direction;

        // To match web defaults
        private FlexDirection? _flexDirection = FlexDirection.Row;
        private Wrap? _flexWrap;
        private Justify? _justifyContent;
        private Align? _alignItems;
        private Align? _alignContent;
        private YogaFrame? _padding;
        private YogaVector? _gap;
        private bool _constrainVertical = true;
        private bool _constrainHorizontal = true;

        private bool _changedCacheBeforeInit;

        private void NotifyControllerUpdated() {
            LayoutControllerUpdatedEvent?.Invoke();
        }

        private void RefreshPadding(YogaFrame value) {
            YogaNode.StyleSetPadding(Edge.Top, value.top);
            YogaNode.StyleSetPadding(Edge.Bottom, value.bottom);
            YogaNode.StyleSetPadding(Edge.Left, value.left);
            YogaNode.StyleSetPadding(Edge.Right, value.right);
        }

        private void RefreshGap(YogaVector value) {
            YogaNode.StyleSetGap(Gutter.Column, value.x);
            YogaNode.StyleSetGap(Gutter.Row, value.y);
        }

        private void RefreshAllProperties() {
            if (_flexDirection.HasValue) {
                YogaNode.StyleSetFlexDirection(_flexDirection.Value);
            }

            // All properties except flexDirection are null by default
            if (!_changedCacheBeforeInit) {
                return;
            }

            if (_overflow.HasValue) {
                YogaNode.StyleSetOverflow(_overflow.Value);
            }
            if (_direction.HasValue) {
                YogaNode.StyleSetDirection(_direction.Value);
            }
            if (_flexWrap.HasValue) {
                YogaNode.StyleSetFlexWrap(_flexWrap.Value);
            }
            if (_justifyContent.HasValue) {
                YogaNode.StyleSetJustifyContent(_justifyContent.Value);
            }
            if (_alignItems.HasValue) {
                YogaNode.StyleSetAlignItems(_alignItems.Value);
            }
            if (_alignContent.HasValue) {
                YogaNode.StyleSetAlignContent(_alignContent.Value);
            }
            if (_gap.HasValue) {
                RefreshGap(_gap.Value);
            }
            if (_padding.HasValue) {
                RefreshPadding(_padding.Value);
            }
        }

        #endregion

        #region Context

        public Type ContextType { get; } = typeof(YogaContext);

        public object CreateContext() => new YogaContext();

        public void ProvideContext(object? context) {
            if (context == null) {
                ReleaseContext();
                return;
            }

            if (_context != null) {
                throw new InvalidOperationException("YogaLayoutController doesn't support context reinitialization, release the context first");
            }

            _context = (YogaContext)context;
            _context.Retain();
            _context.Controller = this;

            _node = _context.YogaNode;
            _node.SetMeasureFunc(null);

            RefreshAllProperties();
        }

        private void ReleaseContext() {
            if (_context == null) {
                return;
            }

            RemoveAllChildren();

            var context = _context;

            context.Controller = null;
            _context = null;
            _node = null;

            // Freed right away if the parent doesn't hold it as a child too
            context.Release();
        }

        #endregion

        #region Calculations

        private bool IsStitched => YogaNode.GetParent() != null;
        private bool HasValidNode => _node?.IsInitialized ?? false;

        private YogaNode YogaNode {
            get {
                if (!HasValidNode) {
                    throw new InvalidOperationException("YogaNode wasn't initialized");
                }
                return _node!;
            }
        }

        private YogaContext? _context;
        private YogaNode? _node;

        public void Recalculate(Vector2 constraints) {
            if (IsStitched) {
                return;
            }

            if (!ConstrainHorizontal) {
                constraints.x = float.NaN;
            }
            if (!ConstrainVertical) {
                constraints.y = float.NaN;
            }

            YogaNode.CalculateLayout(constraints.x, constraints.y, Direction);
        }

        public Vector2 Measure(float width, Reactive.MeasureMode widthMode, float height, Reactive.MeasureMode heightMode) {
            if (!IsStitched) {
                YogaNode.CalculateLayout(
                    widthMode == Reactive.MeasureMode.Exactly ? width : float.NaN,
                    heightMode == Reactive.MeasureMode.Exactly ? height : float.NaN,
                    Direction
                );
            }

            var size = new Vector2(YogaNode.LayoutGetWidth(), YogaNode.LayoutGetHeight());

            return LayoutTool.MeasureNode(size, width, widthMode, height, heightMode);
        }

        public LayoutDimensions GetSelfDimensions() {
            return GetDimensions(YogaNode);
        }

        public LayoutDimensions GetChildDimensions(ILayoutItem child) {
            if (!_nodes.TryGetValue(child, out var context)) {
                return default;
            }

            return GetDimensions(context.YogaNode);
        }

        private static LayoutDimensions GetDimensions(YogaNode node) {
            return new(node.LayoutGetTop(), node.LayoutGetLeft(), node.LayoutGetWidth(), node.LayoutGetHeight());
        }

        #endregion

        #region Children

        public int ChildCount => _nodes.Count;

        private readonly LayoutDictionary<YogaContext> _nodes = new();

        public void InsertChild(ILayoutItem child, int index) {
            if (_nodes.ContainsKey(child)) {
                return;
            }

            // Claim a compatible context or create a new one
            var context = child.LayoutContext as YogaContext ?? new YogaContext();
            context.Retain();
            child.AssignContext(context);

            RefreshChild(child, context);
            YogaNode.InsertChild(context.YogaNode, index);

            _nodes.Add(child, context);
        }

        public void RemoveChild(ILayoutItem child) {
            if (!_nodes.TryGetValue(child, out var context)) {
                return;
            }

            YogaNode.RemoveChild(context.YogaNode);
            context.YogaNode.SetMeasureFunc(null);
            child.AssignContext(null);

            _nodes.Remove(child);
            context.Release();
        }

        public void RemoveAllChildren() {
            if (_nodes.Count == 0) {
                return;
            }

            foreach (var (child, context) in _nodes) {
                context.YogaNode.SetMeasureFunc(null);
                child.AssignContext(null);
            }

            YogaNode.RemoveAllChildren();

            // Released once detached, as the freed nodes can't be in the tree
            foreach (var (_, context) in _nodes) {
                context.Release();
            }

            _nodes.Clear();
        }

        public bool HasChild(ILayoutItem child) {
            return _nodes.ContainsKey(child);
        }

        public void UpdateChild(ILayoutItem child) {
            if (!_nodes.TryGetValue(child, out var context)) {
                return;
            }

            // Claiming or releasing the shared context only changes whether the node is stitched or measured
            RefreshChild(child, context);
        }

        public void UpdateSelf(ILayoutItem item) {
            // A stitched node is styled by the parent controller, which owns the same node
            if (IsStitched) {
                return;
            }

            ApplyModifier(item, YogaNode);
        }

        private static void ApplyModifier(ILayoutItem item, YogaNode node) {
            if (item.LayoutModifier is { } modifier) {
                (modifier as YogaModifier)?.ApplyTo(node);
            } else {
                ApplyDefaultStyle(node);
            }
        }

        private static void RefreshChild(ILayoutItem child, YogaContext context) {
            var node = context.YogaNode;

            ApplyModifier(child, node);

            if (context.Controller != null) {
                context.YogaNode.SetMeasureFunc(null);
                return;
            }

            if (!node.HasMeasureFunc()) {
                node.SetMeasureFunc(CreateMeasureFunc(child));
            }

            node.MarkDirty();
        }

        private static void ApplyDefaultStyle(YogaNode node) {
            node.StyleSetPositionType(PositionType.Relative);
            node.StyleSetAlignSelf(Align.Auto);
            node.StyleSetFlexBasis(YogaValue.Auto);
            node.StyleSetFlexGrow(float.NaN);
            node.StyleSetFlexShrink(float.NaN);
            node.StyleSetFlex(float.NaN);
            node.StyleSetWidth(YogaValue.Auto);
            node.StyleSetHeight(YogaValue.Auto);
            node.StyleSetMinWidth(YogaValue.Undefined);
            node.StyleSetMinHeight(YogaValue.Undefined);
            node.StyleSetMaxWidth(YogaValue.Undefined);
            node.StyleSetMaxHeight(YogaValue.Undefined);
            node.StyleSetAspectRatio(float.NaN);

            for (var edge = Edge.Left; edge <= Edge.Bottom; edge++) {
                node.StyleSetPosition(edge, YogaValue.Undefined);
                node.StyleSetMargin(edge, YogaValue.Undefined);
            }
        }

        private static YGMeasureFunc CreateMeasureFunc(ILayoutMeasurable item) {
            return (_, width, widthMode, height, heightMode) => {
                // An exception can't pass through the native code without crashing the process
                try {
                    var size = item.Measure(width, (Reactive.MeasureMode)widthMode, height, (Reactive.MeasureMode)heightMode);

                    return new() {
                        width = size.x,
                        height = size.y
                    };
                } catch (Exception e) {
                    Debug.LogException(e);
                    return default;
                }
            };
        }

        #endregion
    }
}