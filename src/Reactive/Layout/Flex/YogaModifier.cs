using System;
using JetBrains.Annotations;

namespace Reactive.Yoga;

[PublicAPI]
public class YogaModifier : ILayoutModifier {
    #region Properties

    public PositionType PositionType {
        get => _positionType.GetValueOrDefault();
        set {
            _positionType = value;
            NotifyModifierUpdated();
        }
    }

    public Align AlignSelf {
        get => _alignSelf.GetValueOrDefault();
        set {
            _alignSelf = value;
            NotifyModifierUpdated();
        }
    }

    public YogaValue FlexBasis {
        get => _flexBasis.GetValueOrDefault();
        set {
            _flexBasis = value;
            NotifyModifierUpdated();
        }
    }

    public float FlexGrow {
        get => _flexGrow.GetValueOrDefault();
        set {
            _flexGrow = value;
            NotifyModifierUpdated();
        }
    }

    public float FlexShrink {
        get => _flexShrink.GetValueOrDefault();
        set {
            _flexShrink = value;
            NotifyModifierUpdated();
        }
    }

    public float Flex {
        get => _flex.GetValueOrDefault();
        set {
            _flex = value;
            NotifyModifierUpdated();
        }
    }

    public YogaFrame Position {
        get => _position.GetValueOrDefault();
        set {
            _position = value;
            NotifyModifierUpdated();
        }
    }

    public YogaVector Size {
        get => _size.GetValueOrDefault();
        set {
            _size = value;
            NotifyModifierUpdated();
        }
    }

    public YogaVector MinSize {
        get => _minSize.GetValueOrDefault();
        set {
            _minSize = value;
            NotifyModifierUpdated();
        }
    }

    public YogaVector MaxSize {
        get => _maxSize.GetValueOrDefault();
        set {
            _maxSize = value;
            NotifyModifierUpdated();
        }
    }

    public float AspectRatio {
        get => _aspectRatio.GetValueOrDefault();
        set {
            _aspectRatio = value;
            NotifyModifierUpdated();
        }
    }

    public YogaFrame Margin {
        get => _margin.GetValueOrDefault();
        set {
            _margin = value;
            NotifyModifierUpdated();
        }
    }

    private PositionType? _positionType;
    private Align? _alignSelf;
    private float? _aspectRatio;
    private float? _flex;
    private float? _flexGrow;
    private float? _flexShrink;
    private YogaValue? _flexBasis;
    private YogaFrame? _position;
    private YogaVector? _size;
    private YogaVector? _minSize;
    private YogaVector? _maxSize;
    private YogaFrame? _margin;

    public event Action? ModifierUpdatedEvent;

    private void NotifyModifierUpdated() {
        ModifierUpdatedEvent?.Invoke();
    }

    private static void RefreshMargin(YogaNode node, YogaFrame value) {
        node.StyleSetMargin(Edge.Top, value.top);
        node.StyleSetMargin(Edge.Bottom, value.bottom);
        node.StyleSetMargin(Edge.Left, value.left);
        node.StyleSetMargin(Edge.Right, value.right);
    }

    private static void RefreshPosition(YogaNode node, YogaFrame value) {
        node.StyleSetPosition(Edge.Top, value.top);
        node.StyleSetPosition(Edge.Bottom, value.bottom);
        node.StyleSetPosition(Edge.Left, value.left);
        node.StyleSetPosition(Edge.Right, value.right);
    }

    /// <summary>
    /// Applies the properties to the specified node. Properties that were not set are not applied.
    /// </summary>
    internal void ApplyTo(YogaNode node) {
        if (_positionType.HasValue) {
            node.StyleSetPositionType(_positionType.Value);
        }
        if (_alignSelf.HasValue) {
            node.StyleSetAlignSelf(_alignSelf.Value);
        }
        if (_flexBasis.HasValue) {
            node.StyleSetFlexBasis(_flexBasis.Value);
        }
        if (_flexGrow.HasValue) {
            node.StyleSetFlexGrow(_flexGrow.Value);
        }
        if (_flexShrink.HasValue) {
            node.StyleSetFlexShrink(_flexShrink.Value);
        }
        if (_flex.HasValue) {
            node.StyleSetFlex(_flex.Value);
        }

        if (_position.HasValue) {
            RefreshPosition(node, _position.Value);
        }

        if (_size.HasValue) {
            node.StyleSetWidth(_size.Value.x);
            node.StyleSetHeight(_size.Value.y);
        }

        if (_minSize.HasValue) {
            node.StyleSetMinWidth(_minSize.Value.x);
            node.StyleSetMinHeight(_minSize.Value.y);
        }

        if (_maxSize.HasValue) {
            node.StyleSetMaxWidth(_maxSize.Value.x);
            node.StyleSetMaxHeight(_maxSize.Value.y);
        }

        if (_aspectRatio.HasValue) {
            node.StyleSetAspectRatio(_aspectRatio.Value);
        }

        if (_margin.HasValue) {
            RefreshMargin(node, _margin.Value);
        }
    }

    #endregion
}